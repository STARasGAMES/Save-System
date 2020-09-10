using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using SaG.GuidReferences;
using SaG.SaveSystem.Core;
using SaG.SaveSystem.Data;
using SaG.SaveSystem.Editor.Tools;
using SaG.SaveSystem.Interfaces;
using UnityEngine;

namespace SaG.SaveSystem.Components
{
    /// <summary>
    /// Attach this to the root of an object that you want to save
    /// </summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(-9001)]
    [AddComponentMenu("Saving/Saveable")]
    [RequireComponent(typeof(GuidComponent))]
    public class Saveable : MonoBehaviour, ISaveableContainer
    {
        [Header("Save configuration")]
        [SerializeField, Tooltip("Will never allow the object to load data more then once." +
                                 "this is useful for persistent game objects.")]
        private bool loadOnce = false;

        [SerializeField, Tooltip("Save and Load will not be called by the Save System." +
                                 "this is useful for displaying data from a different save file")]
        private bool manualSaveLoad;

        [SerializeField, Tooltip("It will scan other objects for ISaveable components")]
        private List<GameObject> externalListeners = new List<GameObject>();

        [SerializeField]
        private List<CachedSaveableComponent> cachedSaveableComponents = new List<CachedSaveableComponent>();

        //private Dictionary<string, ISaveable> saveableComponentDictionary = new Dictionary<string, ISaveable>();

        private List<string> saveableComponentIds = new List<string>();
        private List<ISaveableComponent> saveableComponentObjects = new List<ISaveableComponent>();
        private SaveableContainerJObject _container;

        private IGuidProvider _cachedGuidProvider;

        public string Id
        {
            get
            {
                if (_cachedGuidProvider == null)
                    _cachedGuidProvider = GetComponent<IGuidProvider>();
                return _cachedGuidProvider.GetStringGuid();
            }
        }

        private bool hasLoaded;
        private bool hasStateReset;

        /// <summary>
        /// Means of storing all saveable components for the ISaveable component.
        /// </summary>
        [Serializable]
        public class CachedSaveableComponent
        {
            public string identifier;
            public Component component;
        }

        public bool ManualSaveLoad
        {
            get { return manualSaveLoad; }
            set { manualSaveLoad = value; }
        }

#if UNITY_EDITOR

        public void OnValidate()
        {
            if (Application.isPlaying)
                return;

            List<ISaveableComponent> obtainSaveables = new List<ISaveableComponent>();
            GetComponentsInChildren(true, obtainSaveables);
            for (int i = 0; i < externalListeners.Count; i++)
            {
                if (externalListeners[i] != null)
                    obtainSaveables.AddRange(externalListeners[i].GetComponentsInChildren<ISaveableComponent>(true)
                        .ToList());
            }

            for (int i = cachedSaveableComponents.Count - 1; i >= 0; i--)
            {
                if (cachedSaveableComponents[i].component == null)
                {
                    cachedSaveableComponents.RemoveAt(i);
                }
            }

            if (obtainSaveables.Count != cachedSaveableComponents.Count)
            {
                if (cachedSaveableComponents.Count > obtainSaveables.Count)
                {
                    for (int i = cachedSaveableComponents.Count - 1; i >= obtainSaveables.Count; i--)
                    {
                        cachedSaveableComponents.RemoveAt(i);
                    }
                }

                int saveableComponentCount = cachedSaveableComponents.Count;
                for (int i = saveableComponentCount - 1; i >= 0; i--)
                {
                    if (cachedSaveableComponents[i] == null)
                    {
                        cachedSaveableComponents.RemoveAt(i);
                    }
                }

                ISaveableComponent[] cachedSaveables = new ISaveableComponent[cachedSaveableComponents.Count];
                for (int i = 0; i < cachedSaveables.Length; i++)
                {
                    cachedSaveables[i] = cachedSaveableComponents[i].component as ISaveableComponent;
                }

                ISaveableComponent[] missingElements = obtainSaveables.Except(cachedSaveables).ToArray();

                for (int i = 0; i < missingElements.Length; i++)
                {
                    CachedSaveableComponent newSaveableComponent = new CachedSaveableComponent()
                    {
                        identifier = GenerateIdForSaveableComponent(missingElements[i]),
                        component = missingElements[i] as Component
                    };

                    cachedSaveableComponents.Add(newSaveableComponent);
                }

                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }

        private bool IsIdentifierUnique(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return false;

            for (int i = 0; i < cachedSaveableComponents.Count; i++)
            {
                if (cachedSaveableComponents[i].identifier == identifier)
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
        /// Gets and adds a saveable components. This is only required when you want to
        /// create gameobjects dynamically through C#. Keep in mind that changing the component add order
        /// will change the way it gets loaded.
        /// </summary>
        public void ScanAddSaveableComponents()
        {
            ISaveableComponent[] saveables = GetComponentsInChildren<ISaveableComponent>();

            for (int i = 0; i < saveables.Length; i++)
            {
                var saveable = saveables[i];

                AddSaveableComponent($"Dyn-{GetBaseIdForSaveableComponent(saveable)}-{i.ToString()}", saveables[i]);
            }

            // Load it again, to ensure all ISaveable interfaces are updated.
            SaveMaster.ReloadListener(this);
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
            saveableComponentIds.Add(identifier);
            saveableComponentObjects.Add(iSaveableComponent);

            if (reloadData)
            {
                // Load it again, to ensure all ISaveable interfaces are updated.
                SaveMaster.ReloadListener(this);
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

        private void Awake()
        {
            _container = new SaveableContainerJObject("Null");
            // Store the component identifiers into a dictionary for performant retrieval.
            for (int i = 0; i < cachedSaveableComponents.Count; i++)
            {
                saveableComponentIds.Add(cachedSaveableComponents[i].identifier);
                saveableComponentObjects.Add(cachedSaveableComponents[i].component as ISaveableComponent);
            }

            if (!manualSaveLoad)
            {
                SaveMaster.AddListener(this);
            }
        }

        private void OnDestroy()
        {
            if (!manualSaveLoad)
            {
                SaveMaster.RemoveListener(this);
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                ValidateHierarchy.Remove(this);
            }
#endif
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


        public JObject Save()
        {
            int componentCount = saveableComponentIds.Count;

            for (int i = componentCount - 1; i >= 0; i--)
            {
                ISaveableComponent getSaveableComponent = saveableComponentObjects[i];
                string getIdentification = saveableComponentIds[i];

                if (getSaveableComponent == null)
                {
                    Debug.Log(string.Format("Failed to save component: {0}. Component is potentially destroyed.",
                        getIdentification));
                    saveableComponentIds.RemoveAt(i);
                    saveableComponentObjects.RemoveAt(i);
                }
                else
                {
                    if (!hasStateReset && !getSaveableComponent.OnSaveCondition())
                    {
                        continue;
                    }

                    var data = getSaveableComponent.Save();
                    _container.Set(getIdentification, data);
                }
            }

            hasStateReset = false;
            return _container.Save();
        }

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

            int componentCount = saveableComponentIds.Count;

            for (int i = componentCount - 1; i >= 0; i--)
            {
                ISaveableComponent saveableComponent = saveableComponentObjects[i];
                string saveableComponentId = saveableComponentIds[i];

                if (saveableComponent == null)
                {
                    Debug.Log(
                        $"Failed to load component with id: {saveableComponentId}. Component is potentially destroyed.");
                    saveableComponentIds.RemoveAt(i);
                    saveableComponentObjects.RemoveAt(i);
                }
                else
                {
                    var data = _container.Get(saveableComponentId, saveableComponent.SaveDataType);
                    if (data != null)
                        saveableComponent.Load(data);
                }
            }
        }

        public void Set<T>(string key, T value)
        {
            _container.Set(key, value);
        }

        public T Get<T>(string key)
        {
            return _container.Get<T>(key);
        }

        public object Get(string key, Type type)
        {
            return _container.Get(key, type);
        }

        public bool Remove(string key)
        {
            return _container.Remove(key);
        }
    }
}