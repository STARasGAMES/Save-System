using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SaG.SaveSystem.GameStateManagement
{
    public class GameStateManager : IGameStateManager
    {
        private readonly IList<ISaveable> _saveables;
        
        /// <inheritdoc/>
        public IGameState GameState { get; set; }
        
        /// <inheritdoc/>
        public bool IsApplicationQuitting { get; private set; }
        
        /// <inheritdoc/>
        public bool IsIgnoringStateSynchronization { get; set; }

        private bool _isSynchronizingState;

        public GameStateManager()
        {
            _saveables = new List<ISaveable>();
            GameState = new GameState();
            DefaultContainer = new SaveableContainerJObject("DefaultContainer");
            RegisterSaveable(DefaultContainer);
            IsApplicationQuitting = false;
            _isSynchronizingState = false;
            Application.quitting += () => IsApplicationQuitting = true;
        }

        /// <inheritdoc/>
        public ISaveableContainer DefaultContainer { get; }

        /// <inheritdoc/>
        public event EventHandler StateSynchronizing;

        /// <inheritdoc/>
        public event EventHandler StateSynchronized;

        /// <inheritdoc/>
        public event EventHandler StateLoading;

        /// <inheritdoc/>
        public event EventHandler StateLoaded;

        /// <inheritdoc/>
        public void SynchronizeState()
        {
            try
            {
                StateSynchronizing?.Invoke(this, EventArgs.Empty);
                _isSynchronizingState = true;
                foreach (var saveable in _saveables)
                {
                    SaveSaveable(saveable);
                }

                StateSynchronized?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Debug.LogError("Exception occured when synchronizing state.");
                Debug.LogException(e);
                throw;
            }
            finally
            {
                _isSynchronizingState = false;
            }
        }

        /// <inheritdoc/>
        public void LoadState()
        {
            StateLoading?.Invoke(this, EventArgs.Empty);
            for (int i = 0; i < _saveables.Count; i++)
            {
                LoadSaveable(_saveables[i]);
            }
            StateLoaded?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public void RegisterSaveable(ISaveable saveable, bool autoLoad = true)
        {
            if (_isSynchronizingState)
                throw new Exception($"Can't register saveables when synchronizing state. " +
                                    $"Use {nameof(StateSynchronizing)} event to register saveables.");
            if (saveable == null)
                throw new ArgumentNullException(nameof(saveable));
            if (_saveables.Contains(saveable))
                throw new ArgumentException("Collection already has this container", nameof(saveable));
            _saveables.Add(saveable);
            LoadSaveable(saveable);
        }

        /// <inheritdoc/>
        public bool UnregisterSaveable(ISaveable saveable, bool autoSave = true)
        {
            if (_isSynchronizingState)
                throw new Exception($"Can't unregister saveables when synchronizing state. " +
                                    $"Use {nameof(StateSynchronizing)} event to unregister saveables.");
            if (saveable == null)
                throw new ArgumentNullException(nameof(saveable));
            if (autoSave)
                SaveSaveable(saveable); // todo decide to save container or not when it is not registered?
            return _saveables.Remove(saveable);
        }

        /// <inheritdoc/>
        public void SaveSaveable(ISaveable saveable)
        {
            if (IsIgnoringStateSynchronization)
                return;
            GameState.Set(saveable.Id, saveable.Save(), saveable.Context);
        }

        /// <inheritdoc/>
        public void LoadSaveable(ISaveable saveable)
        {
            if (GameState.TryGetValue(saveable.Id, out var value))
            {
                saveable.Load((JObject) value);
            }
        }

        /// <inheritdoc/>
        public bool WipeSaveable(ISaveable saveable)
        {
            return GameState.Remove(saveable.Id);
        }
    }
}