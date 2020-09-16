using SaG.SaveSystem.Components;
using UnityEngine;

namespace SaG.SaveSystem.Samples.Platformer2D
{
    [RequireComponent(typeof(Saveable))]
    [RequireComponent(typeof(SaveVisibility))]
    public class PickableItem : MonoBehaviour
    {
        [SerializeField] private string itemName = default;
        [SerializeField] private int count = 1;

        public string ItemName => itemName;

        public int Count => count;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                var inventory = other.GetComponent<Inventory>();
                inventory.AddItem(itemName, count);
                gameObject.SetActive(false);
            }
        }
    }
}
