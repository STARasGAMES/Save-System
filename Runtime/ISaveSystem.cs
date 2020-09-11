namespace SaG.SaveSystem
{
    public interface ISaveSystem
    {
        /// <summary>
        /// Writes currently active game state to disk.
        /// You need to call this method when user explicitly presses the save button or when you you have auto-save.
        /// </summary>
        void WriteStateToDisk();

        /// <summary>
        /// Reads data from disk and applies it to currently active scene.
        /// Note: current game state will be overwritten.
        /// </summary>
        void ReadStateFromDisk();
    }
}