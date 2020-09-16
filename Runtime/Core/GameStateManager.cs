using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SaG.SaveSystem.Data;
using UnityEngine;

namespace SaG.SaveSystem.Core
{
    public class GameStateManager : IGameStateManager
    {
        private readonly IList<ISaveableContainer> containers;
        public IGameState GameState { get; set; }
        
        public bool IsApplicationQuitting { get; private set; }

        public bool IsIgnoringStateSynchronization { get; set; }

        public GameStateManager()
        {
            containers = new List<ISaveableContainer>();
            GameState = new GameState();
            DefaultContainer = new SaveableContainerJObject("DefaultContainer");
            RegisterContainer(DefaultContainer);
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
            foreach (var container in containers)
            {
                SaveContainer(container);
            }
            StateSynchronized?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public void LoadState()
        {
            StateLoading?.Invoke(this, EventArgs.Empty);
            foreach (var container in containers)
            {
                LoadContainer(container);
            }
            StateLoaded?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public void RegisterContainer(ISaveableContainer saveableContainer, bool autoLoad = true)
        {
            if (saveableContainer == null)
                throw new ArgumentNullException(nameof(saveableContainer));
            if (containers.Contains(saveableContainer))
                throw new ArgumentException("Collection already has this container", nameof(saveableContainer));
            containers.Add(saveableContainer);
            LoadContainer(saveableContainer);
        }

        /// <inheritdoc/>
        public bool UnregisterContainer(ISaveableContainer saveableContainer, bool autoSave = false)
        {
            if (saveableContainer == null)
                throw new ArgumentNullException(nameof(saveableContainer));
            if (autoSave)
                SaveContainer(saveableContainer); // todo decide to save container or not when it is not registered?
            return containers.Remove(saveableContainer);
        }

        /// <inheritdoc/>
        public void SaveContainer(ISaveableContainer saveableContainer)
        {
            if (IsIgnoringStateSynchronization)
                return;
            GameState.Set(saveableContainer.Id, saveableContainer.Save(), saveableContainer.Context);
        }

        /// <inheritdoc/>
        public void LoadContainer(ISaveableContainer saveableContainer)
        {
            if (GameState.TryGetValue(saveableContainer.Id, out var value))
            {
                saveableContainer.Load((JObject) value);
            }
        }

        /// <inheritdoc/>
        public bool WipeContainer(ISaveableContainer saveableContainer)
        {
            return GameState.Remove(saveableContainer.Id);
        }
    }
}