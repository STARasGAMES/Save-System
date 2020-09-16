using System;
using System.Collections.Generic;
using SaG.SaveSystem.Components;
using UnityEngine;

namespace SaG.SaveSystem.Samples.Platformer2D
{
    [SaveableComponentId("inventory")]
    public class Inventory : MonoBehaviour, ISaveableComponent
    {
        [SerializeField] private List<ItemInfo> _items = default;

        [Serializable]
        public class ItemInfo
        {
            public string name;
            public int count;
        }

        public IReadOnlyList<ItemInfo> Items => _items;

        #region ISaveableComponent implementation

        public Type SaveDataType => typeof(List<ItemInfo>);

        public object Save()
        {
            return _items;
        }

        public void Load(object data)
        {
            _items = (List<ItemInfo>) data;
        }

        public bool OnSaveCondition()
        {
            return _items != null;
        }

        #endregion ISaveableComponent implementation

        #region Inventory implementation

        public void AddItem(string itemName, int count = 1)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].name == itemName)
                {
                    _items[i].count += count;
                    return;
                }
            }

            _items.Add(new ItemInfo()
            {
                name = itemName,
                count = count
            });
        }

        public void RemoveItem(string itemName, int count = 1)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].name == itemName)
                {
                    var itemInfo = _items[i];
                    if (itemInfo.count < count)
                        throw new ArgumentException(
                            $"Trying to remove {count} '{itemName}', but there are only {itemInfo.count}",
                            nameof(count));
                    _items[i].count -= count;
                    return;
                }
            }
            throw new ArgumentException($"Trying to remove {count} '{itemName}', but there is no such item.", nameof(itemName));
        }

        public bool HasItem(string itemName, int count = 1)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].name == itemName)
                {
                    return _items[i].count >= count;
                }
            }
            return false;
        }

        #endregion
    }
}