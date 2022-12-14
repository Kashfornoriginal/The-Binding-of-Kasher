using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melt : MonoBehaviour
{
    [SerializeField] private List<MeltRecipe> _meltRecipes;

    [SerializeField] private InventoryObject _meltInventory;

    [SerializeField] private MeltProcessDisplay _meltProccessDisplay;

    [SerializeField] private ItemsData _coal;

    [SerializeField] private float _coalBurningTime;
    
    [SerializeField] private float _itemBurningTime = 0;

    [SerializeField] private bool _isItemProcessing = false;
    [SerializeField] private bool _isCoalBurning = false;

    [SerializeField] private float _currentCoalBurningTime;
    [SerializeField] private float _currentItemBurningTime;

    [SerializeField] private ItemsData _currentItem;
    [SerializeField] private int _currentItemRequiredAmount;
    [SerializeField] private ItemsData _outputItem;
    [SerializeField] private int _outputItemAmount;
    
    private void Update()
    {
        CheckItemsForAmount();
        
        if (_isItemProcessing == false)
        {
            if (_meltInventory.ItemsContainer.Slots[^1].MaxSlotAmount > 0)
            {
                TryMelt();
            }

        }
        else
        {
            _currentItemBurningTime += Time.deltaTime;
            
            _meltProccessDisplay.FillArrow(_currentItemBurningTime / _itemBurningTime);

            if (_currentItemBurningTime >= _itemBurningTime)
            {
                MeltItem(_currentItem, _outputItem);
                ResetOvenItems();
                _meltProccessDisplay.FillArrow(_currentItemBurningTime);
            }
        }

        if (_isCoalBurning == true)
        {
            _currentCoalBurningTime += Time.deltaTime;
            
            _meltProccessDisplay.WithdrawCoal(1 - _currentCoalBurningTime / _coalBurningTime);

            if (_currentCoalBurningTime >= _coalBurningTime)
            {
                BurnCoal();
                _currentCoalBurningTime = 0;
                _meltProccessDisplay.WithdrawCoal(1 - _currentCoalBurningTime / _coalBurningTime);
            }
        }
    }

    private void TryMelt()
    {
        bool _isItemFound = false;
        bool _isCoalFound = false;

        ResetOvenItems();

        for (int i = 0; i < _meltInventory.ItemsContainer.Slots.Length - 1; i++)
        {
            if (_meltInventory.ItemsContainer.Slots[i].Item.ID >= 0)
            {
                
                if (_meltInventory.ItemsContainer.Slots[i].ItemObject == _coal)
                {
                    _isCoalFound = true;
                }
                else
                {
                    _meltProccessDisplay.WithdrawCoal(1 - _currentCoalBurningTime / _coalBurningTime);
                }
                
                foreach (var recipe in _meltRecipes)
                {
                    if (recipe.RequiredItems[0].Item == _meltInventory.ItemsContainer.Slots[i].ItemObject)
                    {
                        _currentItemRequiredAmount = recipe.RequiredItems[0].Amount;

                        if (_meltInventory.ItemsContainer.Slots[i].Amount < _currentItemRequiredAmount)
                        {
                            break;
                        }
                        
                        _outputItemAmount = recipe.OutputItemAmount;
                        _outputItem = recipe.OutputItem;
                        _currentItem = recipe.RequiredItems[0].Item;
                        _itemBurningTime = recipe.MeltTime;
                        _isItemFound = true;
                        break;
                    }
                }
            }
        }

        if (_isItemFound == true && _isCoalFound == true)
        {
            
            if (_meltInventory.ItemsContainer.Slots[^1].ItemObject != _outputItem && _meltInventory.ItemsContainer.Slots[^1].Item.ID >= 0)
            {
                return;
            }
            
            _isItemProcessing = true;
            _isCoalBurning = true;
        }
    }

    private void CheckItemsForAmount()
    {
        for (int i = 0; i < _meltInventory.ItemsContainer.Slots.Length - 1; i++)
        {
            if (_meltInventory.ItemsContainer.Slots[i].Item.ID < 0)
            {
                ResetOvenItems();
            }
        }
    }

    private void ResetOvenItems()
    {
        _isItemProcessing = false;
        _isCoalBurning = false;
        _outputItem = null;
        _currentItem = null;
        _currentItemRequiredAmount = 0;
        _outputItemAmount = 0;
        _itemBurningTime = 0;
        _currentItemBurningTime = 0;
        _meltProccessDisplay.FillArrow(_currentItemBurningTime);
    }

    private void MeltItem(ItemsData item, ItemsData outputItem)
    {
        _meltInventory.RemoveItemAmountFromInventory(_meltInventory.FindItemInInventory(item.Data), _currentItemRequiredAmount);
        AddMoltenItem(outputItem);
    }

    private void AddMoltenItem(ItemsData outputItem)
    {
        _meltInventory.AddItemToInventory(outputItem.Data, _outputItemAmount);
    }

    private void BurnCoal()
    {
        _meltInventory.RemoveItemAmountFromInventory(_meltInventory.FindItemInInventory(_coal.Data), 1);
    }
}
