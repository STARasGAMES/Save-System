using UnityEngine;
using UnityEngine.Events;

namespace SaG.SaveSystem.Samples.Platformer2D.Triggers
{
    public class InventoryTrigger : MonoBehaviour
    {
        [SerializeField] private string _item = default;
        [SerializeField] private int _count = 1;
        [SerializeField] private UnityEvent _event = default;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                var inventory = other.GetComponent<Inventory>();
                if (inventory.ContainsItem(_item, _count))
                {
                    inventory.RemoveItem(_item, _count);
                    _event.Invoke();
                }
            }
        }
    }
}
