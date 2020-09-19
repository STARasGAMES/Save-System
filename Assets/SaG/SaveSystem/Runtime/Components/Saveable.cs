using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using SaG.GuidReferences;
using SaG.SaveSystem.Core;
using SaG.SaveSystem.Data;
#if UNITY_EDITOR
using SaG.SaveSystem.Editor.Tools;
using UnityEditor;
#endif
using UnityEngine;

namespace SaG.SaveSystem.Components
{
    /// <summary>
    /// Attach this to the root of an object that you want to save
    /// </summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(-9001)]
    [AddComponentMenu("Saving/Saveable")]
    [RequireComponent(typeof(IGuidProvider))]
    public class Saveable : MonoBehaviour, ISaveableContainer
    {
        [Header("Save configuration")]
        [SerializeField, Tooltip("Will never allow the object to load data more then once." +
                                 "this is useful for persistent game objects.")]
        private bool loadOnce = false;

        [SerializeField, Tooltip("Save and Load will not be called by the Save System." +
                                 "this is useful for displaying data from a different save file")]
        private bool manualSaveLoad;

        [SerializeField, Tooltip("It will scan other objects for " + nameof(ISaveableComponent) + "s")]
        private List<GameObject> externalListeners = new List<GameObject>();

        [SerializeField]
        private List<CachedSaveableComponent> serializedSaveableComponents = new List<CachedSaveableComponent>();

        private readonly IDictionary<string, ISaveableComponent> _saveableComponents =
            new Dictionary<string, ISaveableComponent>();

        private SaveableContainerJObject _container;

        private IGuidProvider _cachedGuidProvider;

        private bool hasLoaded;
        private bool hasStateReset;

        public bool ManualSaveLoad
        {
            get { return manualSaveLoad; }
            set { manualSaveLoad = value; }
        }


        private void Awake()
        {
            _container = new SaveableContainerJObject("Null");

            foreach (var cachedSaveableComponent in serializedSaveableComponents)
            {
                _saveableComponents.Add(cachedSaveableComponent.identifier,
                    (ISaveableComponent) cachedSaveableComponent.component);
            }

            if (!manualSaveLoad)
            {
                SaveSystemSingleton.Instance.GameStateManager.RegisterSaveable(this);
            }
        }

        private void OnDestroy()
        {
            if (!manualSaveLoad)
            {
                SaveSystemSingleton.Instance.GameStateManager.UnregisterSaveable(this);
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                ValidateHierarchy.Remove(this);
            }
#endif
        }

        /// <summary>
        /// Returns if the object has been destroyed using GameObject.Destroy(obj).
        /// Will return false if it has been destroyed due to the game exiting or scene unloading.
        /// </summary>
        /// <param name="gameObject">Game object</param>
        /// <returns></returns>
        public static bool IsObjectDestroyedExplicitly(GameObject gameObject)
        {
            return gameObject.scene.isLoaded && !SaveSystemSingleton.Instance.GameStateManager.IsApplicationQuitting;
        }

        /// <summary>
        /// Gets and adds a saveable components. This is only required when you want to
        /// create gameobjects dynamically through C#. Keep in mind that changing the component add order
        /// will change the way it gets loaded, because this method will automatically
        /// assign IDs to each <see cref="ISaveableComponent"/> and these IDs depend on component order.
        /// ID example: "Dyn-position-0".
        /// </summary>
        /// <example><code>
        /// var go = new GameObject("My dynamically created game object");
        /// go.SetActive(false); // we need to disable object first to prevent Awake() on <see cref="Saveable"/>.
        /// go.AddComponent{SomeSaveableComponent}(); // adding order of <see cref="ISaveableComponent"/> must not change, otherwise, you will lose saved data about this components.
        /// go.AddComponent{SomeAnotherSaveableComponent}();
        /// var saveable = go.AddComponent{Saveable}(); // adding the main <see cref="Saveable"/> component, without it game object will not save.
        /// saveable.<see cref="ScanAddSaveableComponents"/>();
        /// go.SetActive(true);
        /// </code></example>
        public void ScanAddSaveableComponents()
        {
            ISaveableComponent[] saveables = GetComponentsInChildren<ISaveableComponent>();

            for (int i = 0; i < saveables.Length; i++)
            {
                var saveable = saveables[i];

                AddSaveableComponent($"Dyn-{GetBaseIdForSaveableComponent(saveable)}-{i.ToString()}", saveables[i]);
            }

            // Load it again, to ensure all ISaveableComponent interfaces are updated.
            SaveSystemSingleton.Instance.GameStateManager.LoadSaveable(this);
        }

        /// <summary>
        /// Useful if you want to dynamically add a saveable component. To ensure it 
        /// gets registered.
        /// </summary>
        /// <param name="identifier">The identifier for the component, this is the adress the data will be loaded from </param>
        /// <param name="iSaveableComponent">The interface reference on the component. </param>
        /// <param name="reloadData">Do you want to reload the data on all the components? 
        /// Only call this if you add one component. Else call SaveMaster.ReloadListener(saveable). </param>
        public void AddSaveableComponent(string identifier, ISaveableComponent iSaveableComponent,
            bool reloadData = false)
        {
            _saveableComponents.Add(identifier, iSaveableComponent);

            if (reloadData)
            {
                // Load it again, to ensure all ISaveableComponent interface are updated.
                SaveSystemSingleton.Instance.GameStateManager.LoadSaveable(this);
            }
        }


        /// <summary>
        /// Returns default not unique id for specified saveable component.
        /// Returns Id specified in <see cref="SaveableComponentIdAttribute"/>. If there is no attribute then returns class name.
        /// </summary>
        /// <param name="saveableComponent">Saveable component.</param>
        /// <returns>Default not unique id for specified saveable component.</returns>
        private static string GetBaseIdForSaveableComponent(ISaveableComponent saveableComponent)
        {
            var type = saveableComponent.GetType();
            var idAttribute = (SaveableComponentIdAttribute) type
                .GetCustomAttributes(typeof(SaveableComponentIdAttribute), true).FirstOrDefault();
            var identifier = idAttribute != null ? idAttribute.Id : type.Name;
            return identifier;
        }


        /// <summary>
        /// Used to reset the saveable, as if it was never saved or loaded.
        /// </summary>
        public void ResetState()
        {
            // Since the game uses a new save game, reset loadOnce and hasLoaded
            loadOnce = false;
            hasLoaded = false;
            hasStateReset = true;
        }

        #region ISaveableContainer implementation

        /// <inheritdoc/>
        public string Id
        {
            get
            {
                if (_cachedGuidProvider == null)
                    _cachedGuidProvider = GetComponent<IGuidProvider>();
                return _cachedGuidProvider.GetStringGuid();
            }
        }

        /// <inheritdoc/>
        public string Context => gameObject.scene.name; // todo cache?

        /// <inheritdoc/>
        public JObject Save()
        {
            foreach (var saveableComponent in _saveableComponents)
            {
                string getIdentification = saveableComponent.Key;
                ISaveableComponent getSaveableComponent = saveableComponent.Value;

                if (getSaveableComponent == null)
                {
                    Debug.LogError(string.Format("Failed to save component: {0}. Component is potentially destroyed.",
                        getIdentification));
                }
                else
                {
                    if (!hasStateReset && !getSaveableComponent.OnSaveCondition())
                    {
                        continue;
                    }

                    var data = getSaveableComponent.Save();
                    if (data == null)
                    {
                        Debug.LogError($"Saveable component returned null data. Id: {getIdentification}", gameObject);
                    }
                    else
                    {
                        _container.Set(getIdentification, data);
                    }
                }
            }

            // clear empty records if any
            foreach (var emptyKey in _saveableComponents.Where(pair => pair.Value == null).Select(pair => pair.Key)
                .ToArray())
            {
                _saveableComponents.Remove(emptyKey);
            }

            hasStateReset = false;
            return _container.Save();
        }

        /// <inheritdoc/>
        public void Load(JObject state)
        {
            if (loadOnce && hasLoaded)
            {
                return;
            }

            // Ensure it loads only once with the loadOnce parameter set to true
            hasLoaded = true;

            if (_container == null)
                _container = new SaveableContainerJObject("Saveable GameObject Container");
            _container.Load(state);

            foreach (var pair in _saveableComponents)
            {
                string saveableComponentId = pair.Key;
                ISaveableComponent saveableComponent = pair.Value;

                if (saveableComponent == null)
                {
                    Debug.LogError(
                        $"Failed to load component with id: {saveableComponentId}. Component is potentially destroyed.");
                }
                else
                {
                    if (_container.TryGetValue(saveableComponentId, out var data, saveableComponent.SaveDataType))
                        saveableComponent.Load(data);
                }
            }

            // clear empty records if any
            foreach (var emptyKey in _saveableComponents.Where(pair => pair.Value == null).Select(pair => pair.Key)
                .ToArray())
            {
                _saveableComponents.Remove(emptyKey);
            }
        }

        /// <inheritdoc/>
        public void Set<T>(string key, T value)
        {
            _container.Set(key, value);
        }

        /// <inheritdoc/>
        public T Get<T>(string key)
        {
            return _container.Get<T>(key);
        }

        /// <inheritdoc/>
        public object Get(string key, Type type)
        {
            return _container.Get(key, type);
        }

        /// <inheritdoc/>
        public bool TryGetValue(string key, out object value, Type type)
        {
            return _container.TryGetValue(key, out value, type);
        }

        /// <inheritdoc/>
        public bool TryGetValue<T>(string key, out T value)
        {
            return _container.TryGetValue(key, out value);
        }

        /// <inheritdoc/>
        public bool Remove(string key)
        {
            return _container.Remove(key);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _container.Clear();
        }

        #endregion ISaveableContainer implementation


#if UNITY_EDITOR

        public void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (GetComponent<IGuidProvider>() == null)
            {
                Undo.AddComponent<GuidComponent>(gameObject);
            }

            List<ISaveableComponent> obtainSaveables = new List<ISaveableComponent>();
            GetComponentsInChildren(true, obtainSaveables);
            for (int i = 0; i < externalListeners.Count; i++)
            {
                if (externalListeners[i] != null)
                    obtainSaveables.AddRange(externalListeners[i].GetComponentsInChildren<ISaveableComponent>(true)
                        .ToList());
            }

            for (int i = serializedSaveableComponents.Count - 1; i >= 0; i--)
            {
                if (serializedSaveableComponents[i].component == null)
                {
                    serializedSaveableComponents.RemoveAt(i);
                }
            }

            if (obtainSaveables.Count != serializedSaveableComponents.Count)
            {
                if (serializedSaveableComponents.Count > obtainSaveables.Count)
                {
                    for (int i = serializedSaveableComponents.Count - 1; i >= obtainSaveables.Count; i--)
                    {
                        serializedSaveableComponents.RemoveAt(i);
                    }
                }

                int saveableComponentCount = serializedSaveableComponents.Count;
                for (int i = saveableComponentCount - 1; i >= 0; i--)
                {
                    if (serializedSaveableComponents[i] == null)
                    {
                        serializedSaveableComponents.RemoveAt(i);
                    }
                }

                ISaveableComponent[] cachedSaveables = new ISaveableComponent[serializedSaveableComponents.Count];
                for (int i = 0; i < cachedSaveables.Length; i++)
                {
                    cachedSaveables[i] = serializedSaveableComponents[i].component as ISaveableComponent;
                }

                ISaveableComponent[] missingElements = obtainSaveables.Except(cachedSaveables).ToArray();

                for (int i = 0; i < missingElements.Length; i++)
                {
                    CachedSaveableComponent newSaveableComponent = new CachedSaveableComponent()
                    {
                        identifier = GenerateIdForSaveableComponent(missingElements[i]),
                        component = missingElements[i] as Component
                    };

                    serializedSaveableComponents.Add(newSaveableComponent);
                }

                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }

        private bool IsIdentifierUnique(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return false;

            for (int i = 0; i < serializedSaveableComponents.Count; i++)
            {
                if (serializedSaveableComponents[i].identifier == identifier)
                {
                    return false;
                }
            }

            return true;
        }

        public void Refresh()
        {
            OnValidate();
        }

        /// <summary>
        /// Returns unique id for specified saveable component.
        /// </summary>
        /// <param name="saveableComponent"></param>
        /// <returns></returns>
        private string GenerateIdForSaveableComponent(ISaveableComponent saveableComponent)
        {
            var identifier = GetBaseIdForSaveableComponent(saveableComponent);

            while (!IsIdentifierUnique(identifier))
            {
                int guidLength = SaveSettings.Get().componentGuidLength;
                string guidString = Guid.NewGuid().ToString().Substring(0, guidLength);
                identifier = string.Format("{0}-{1}", identifier, guidString);
            }

            return identifier;
        }

#endif // UNITY_EDITOR

        /// <summary>
        /// Means of storing all saveable components for the ISaveable component.
        /// </summary>
        [Serializable]
        public class CachedSaveableComponent
        {
            public string identifier;
            public Component component;
        }
    }
}