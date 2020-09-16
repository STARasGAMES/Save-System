using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SaG.SaveSystem.Samples.Platformer2D.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private Text text = default;
        private Inventory _playerInventory;

        private void Awake()
        {
            _playerInventory = FindObjectOfType<Player>().GetComponent<Inventory>();
        }

        private void Update()
        {
            StringBuilder builder =new StringBuilder();
            foreach (var itemInfo in _playerInventory.Items)
            {
                builder.Append($"{itemInfo.name}: {itemInfo.count}\n");
            }

            text.text = builder.ToString();
        }
    }
}
