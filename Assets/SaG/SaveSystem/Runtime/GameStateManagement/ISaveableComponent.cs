
using System;

namespace SaG.SaveSystem.GameStateManagement
{
    public interface ISaveableComponent
    {
        /// <summary>
        /// Returns object type used to save data.
        /// </summary>
        Type SaveDataType { get; }
        
        /// <summary>
        /// Returns object that contains save data.
        /// </summary>
        /// <returns>Save data</returns>
        object Save();

        /// <summary>
        /// Loads state from provided save data.
        /// </summary>
        /// <param name="data">Save data</param>
        void Load(object data);

        /// <summary>
        /// Returning true will allow the save to occur, else it will skip the save.
        /// This is useful when you want to call Save() only when something has actually changed.
        /// </summary>
        bool OnSaveCondition();
    }
}