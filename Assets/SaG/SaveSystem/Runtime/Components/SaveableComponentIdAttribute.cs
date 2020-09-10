using System;

namespace SaG.SaveSystem.Components
{
    /// <summary>
    /// Attribute that allows to specify custom IDs for saveable components.
    /// This is helpful in several ways:
    /// 1. most likely this ID would be much shorter than default one which is class name;
    /// 2. class names are subject to change, so with this attribute you won't break save file. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SaveableComponentIdAttribute : Attribute
    {
        public readonly string Id;

        public SaveableComponentIdAttribute(string id)
        {
            Id = id;
        }
    }
}