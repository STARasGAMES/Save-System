using System;

namespace SaG.SaveSystem
{
    public interface IGameStateManager
    {
        /// <summary>
        /// Occurs before synchronizing state.
        /// </summary>
        event EventHandler StateSynchronizing;
        
        /// <summary>
        /// Occurs after state is synchronized.
        /// </summary>
        event EventHandler StateSynchronized;

        /// <summary>
        /// Occurs before loading state.
        /// </summary>
        event EventHandler StateLoading;

        /// <summary>
        /// Occurs after state is loaded.
        /// </summary>
        event EventHandler StateLoaded;
        
        /// <summary>
        /// Gets the container that can be used like a global data storage. Something like PlayerPrefs.
        /// </summary>
        ISaveableContainer DefaultContainer { get; }
        
        /// <summary>
        /// Gathers all data from active saveables and stores it in currently active game state.
        /// You need to call this method before unload scene to save state of objects that are going to be destroyed. 
        /// </summary>
        void SynchronizeState();

        /// <summary>
        /// Loads data from currently active game state into active saveables.
        /// You can use this method to return your game to previous state like checkpoint.
        /// </summary>
        void LoadState();

        /// <summary>
        /// Registers container to auto synchronization with game state.
        /// </summary>
        /// <param name="saveableContainer">Saveable container</param>
        /// <param name="autoLoad">Indicates whether container will be loaded right after registration.</param>
        void RegisterContainer(ISaveableContainer saveableContainer, bool autoLoad = true);

        /// <summary>
        /// Unregisters container from auto synchronization with game state.
        /// Note: no data will be removed.
        /// </summary>
        /// <param name="saveableContainer"></param>
        /// <param name="autoSave"></param>
        /// <returns><c>true</c> if container is successfully found and removed; otherwise, <c>false</c>.</returns>
        bool UnregisterContainer(ISaveableContainer saveableContainer, bool autoSave = false);

        /// <summary>
        /// Saves container to currently active game state.
        /// </summary>
        /// <param name="saveableContainer"></param>
        void SaveContainer(ISaveableContainer saveableContainer);

        /// <summary>
        /// Loads container from currently active game state.
        /// </summary>
        /// <param name="saveableContainer"></param>
        void LoadContainer(ISaveableContainer saveableContainer);

        /// <summary>
        /// Wipes all data associated with specified container in currently active game state.
        /// </summary>
        /// <param name="saveableContainer">Container</param>
        /// <returns><c>true</c> if data successfully found and wiped; otherwise, <c>false</c>.</returns>
        bool WipeContainer(ISaveableContainer saveableContainer);
    }
}