using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryObject", menuName = "ScriptableObject/Inventory")]
public class InventoryObject : ScriptableObject
{
    [SerializeField] private List<InventorySlot> _itemsContainer = new List<InventorySlot>();
    
    public List<InventorySlot> ItemContainer => _itemsContainer;
    
    public void AddItemToInventory(ItemsData item, int amount)
    {
        bool _hasItemInInventory = false;

        for(int i = 0; i < _itemsContainer.Count; i++)
        {
            if (_itemsContainer[i].Item == item)
            {
                _itemsContainer[i].AddItemAmount(amount);
                _hasItemInInventory = true;
                break;
            }
        }
        if (_hasItemInInventory == false)
        {
            _itemsContainer.Add(new InventorySlot(item, amount));
        }
    }

    public void RemoveItemFromInventory(ItemsData item, int amount)
    {
        for(int i = 0; i < _itemsContainer.Count; i++)
        {
            if (_itemsContainer[i].Item == item)
            {
                _itemsContainer[i].RemoveItemAmount(amount);
                if (_itemsContainer[i].Amount <= 0)
                {
                    _itemsContainer.RemoveAt(i);
                }
                break;
            }
        }
    }
}
