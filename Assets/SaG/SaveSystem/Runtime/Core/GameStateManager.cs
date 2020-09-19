using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SaG.SaveSystem.Data;
using UnityEngine;

namespace SaG.SaveSystem.Core
{
    public class GameStateManager : IGameStateManager
    {
        private readonly IList<ISaveable> saveables;
        public IGameState GameState { get; set; }
        
        public bool IsApplicationQuitting { get; private set; }

        public bool IsIgnoringStateSynchronization { get; set; }

        public GameStateManager()
        {
            saveables = new List<ISaveable>();
            GameState = new GameState();
            DefaultContainer = new SaveableContainerJObject("DefaultContainer");
            RegisterSaveable(DefaultContainer);
            IsApplicationQuitting = false;
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
            StateSynchronizing?.Invoke(this, EventArgs.Empty);
            foreach (var container in saveables)
            {
                SaveSaveable(container);
            }
            StateSynchronized?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public void LoadState()
        {
            StateLoading?.Invoke(this, EventArgs.Empty);
            foreach (var container in saveables)
            {
                LoadSaveable(container);
            }
            StateLoaded?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public void RegisterSaveable(ISaveable saveable, bool autoLoad = true)
        {
            if (saveable == null)
                throw new ArgumentNullException(nameof(saveable));
            if (saveables.Contains(saveable))
                throw new ArgumentException("Collection already has this container", nameof(saveable));
            saveables.Add(saveable);
            LoadSaveable(saveable);
        }

        /// <inheritdoc/>
        public bool UnregisterSaveable(ISaveable saveable, bool autoSave = true)
        {
            if (saveable == null)
                throw new ArgumentNullException(nameof(saveable));
            if (autoSave)
                SaveSaveable(saveable); // todo decide to save container or not when it is not registered?
            return saveables.Remove(saveable);
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