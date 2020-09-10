
using System;

namespace SaG.SaveSystem.Interfaces
{
    public interface ISaveableComponent
    {
        /// <summary>
        /// Returns object type used to save data.
        /// </summary>
        Type SaveDataType { get; }
        
        /// <summary>
        /// Called by a Saveable component. SaveMaster (request save) 
        /// -> notify to all Saveables -> return data to active save file with OnSave()
        /// </summary>
        /// <returns> Data for the save file </returns>
        object Save();

        /// <summary>
        /// Called by a Saveable component. SaveMaster (request load) 
        /// -> notify to all Saveables -> obtain data for this specific component with Load()
        /// </summary>
        /// <param name="data"> Data that gets retrieved from the active save file </param>
        void Load(object data);

        /// <summary>
        /// Returning true will allow the save to occur, else it will skip the save.
        /// This is useful when you want to call Save() only when something has actually changed.
        /// </summary>
        bool OnSaveCondition();
    }
}