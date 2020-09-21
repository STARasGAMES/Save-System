using System;

namespace SaG.SaveSystem.GameStateManagement
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
        /// Gets or sets current game state.
        /// </summary>
        IGameState GameState { get; set; }
        
        /// <summary>
        /// Gets a value that indicates is application quitting.  
        /// </summary>
        bool IsApplicationQuitting { get; }
        
        /// <summary>
        /// Gets or sets a value indicating whether all state synchronization requests will be ignored.
        /// </summary>
        bool IsIgnoringStateSynchronization { get; set; }
        
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
        /// Registers saveable to auto synchronization with game state.
        /// </summary>
        /// <param name="saveable">Saveable</param>
        /// <param name="autoLoad">Indicates whether container will be loaded right after registration.</param>
        /// <exception cref="Exception">thrown when state is synchronizing.</exception>
        void RegisterSaveable(ISaveable saveable, bool autoLoad = true);

        /// <summary>
        /// Unregisters saveable from auto synchronization with game state.
        /// Note: no data will be removed.
        /// </summary>
        /// <param name="saveable">Saveable</param>
        /// <param name="autoSave">Indicates whether container will be saved right before unregistration.</param>
        /// <returns><c>true</c> if container is successfully found and removed; otherwise, <c>false</c>.</returns>
        /// <exception cref="Exception">thrown when state is synchronizing.</exception>
        bool UnregisterSaveable(ISaveable saveable, bool autoSave = true);

        /// <summary>
        /// Saves saveable into currently active game state.
        /// </summary>
        /// <param name="saveable"></param>
        void SaveSaveable(ISaveable saveable);

        /// <summary>
        /// Loads savealbe from currently active game state.
        /// </summary>
        /// <param name="saveable"></param>
        /// <remarks>This will not call <c>Load()</c> method on saveable container if there is no data about specified container.</remarks>
        void LoadSaveable(ISaveable saveable);

        /// <summary>
        /// Wipes all data associated with specified saveable in currently active game state.
        /// </summary>
        /// <param name="saveable"></param>
        /// <returns><c>true</c> if data successfully found and wiped; otherwise, <c>false</c>.</returns>
        bool WipeSaveable(ISaveable saveable);
    }
}