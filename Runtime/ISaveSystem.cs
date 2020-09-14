﻿using SaG.SaveSystem.Core;

namespace SaG.SaveSystem
{
    public interface ISaveSystem
    {
        /// <summary>
        /// Writes currently active game state to disk.
        /// You need to call this method when user explicitly presses the save button or when you have auto-save.
        /// </summary>
        /// <param name="name"></param>
        /// <remarks>Note: game state will not automatically synchronized before writing to disk. You need to make this manually.</remarks>
        void WriteStateToDisk(string name);

        /// <summary>
        /// Reads game state from file and replaces the current game state.
        /// </summary>
        /// <param name="name"></param>
        /// <remarks>
        /// <para>Note: current game state will be overwritten.</para>
        /// <para>Note: new game state will not load automatically. You need to make this manually.
        /// <code>IGameStateManager.LoadState()</code>
        /// </para>
        /// </remarks>
        void ReadStateFromDisk(string name);
        
        IGameStateManager GameStateManager { get; }
        
        FileUtility FileUtility { get; } 
    }
}