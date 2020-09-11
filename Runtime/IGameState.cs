using System.Collections.Generic;

namespace SaG.SaveSystem
{
    public interface IGameState
    {
        /// <summary>
        /// Determines whether GameState contains the specified id.
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>t<c>true</c> if the GameState contains an element with the specified key; otherwise, <c>false</c>.</returns>
        bool ContainsId(string id);
        
        /// <summary>
        /// Assign any data to the given ID. If data is already present within the ID, then it will be overwritten.
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <param name="value">Data</param>
        /// <param name="context">Scene name or custom context</param>
        void Set(string id, object value, string context);

        /// <summary>
        /// Gets the value associated with the specified id.
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <param name="value">Value</param>
        /// <returns><c>true</c> if contains an element with the specified id; otherwise, <c>false</c></returns>
        bool TryGetValue(string id, out object value);
        
        /// <summary>
        /// Gets the value associated with the specified id.
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException ">If the specified key is not found</exception>
        object Get(string id);
        
        /// <summary>
        /// Removes record with the specified id from the GameState.
        /// Returns <c>true</c> if the element is successfully found and removed; otherwise, <c>false</c>.
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns><c>true</c> if the element is successfully found and removed; otherwise, <c>false</c>.</returns>
        bool Remove(string id);

        /// <summary>
        /// Removes records with the specified context from the GameState.
        /// </summary>
        /// <param name="context">Scene name or custom context</param>
        /// <returns>Count of removed records.</returns>
        int RemoveContext(string context);
    }
}