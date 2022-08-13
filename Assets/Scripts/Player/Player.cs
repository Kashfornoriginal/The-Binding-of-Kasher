using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject _woodInterface;

    [SerializeField] private int _healthPoint;
    [SerializeField] private int _hungerPoint;
    [SerializeField] private int _waterPoint;

    [SerializeField] private PlayerStatsChanger _playerStatsChanger;

    [SerializeField] private InventoryObject _playerInventory;
    
    public MouseItem MouseItem = new MouseItem();

    public int HealthPoint => _healthPoint;
    public int HungerPoint => _hungerPoint;
    public int WaterPoint => _waterPoint;

    private void Start()
    {
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            _playerInventory.SaveInventory();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            _playerInventory.LoadInventory();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            _playerInventory.ClearInventory();
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Tree")
        {
            _woodInterface.SetActive(true);
            _woodInterface.GetComponent<CollectWoodDisplay>().StartCollectWoodButton(true);
        }

        if (collider.GetComponent<GroundItem>() == true)
        {
            _playerInventory.AddItemToInventory(new Item(collider.GetComponent<GroundItem>().Item), collider.GetComponent<GroundItem>().Amount);
            Destroy(collider.gameObject);
        }
    }
    private void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "Tree")
        {
            _woodInterface.SetActive(false);
            _woodInterface.GetComponent<CollectWoodDisplay>().StartCollectWoodButton(false);
        }
    }
    private void IncreaseHeath(int value)
    {
        _healthPoint += value;
    }
    private void DecreaseHealth(int value)
    {
        _healthPoint -= value;

        if (_healthPoint <= 0)
        {
            Die();
        }
    }
    private void IncreaseHunger(int value)
    {
        _hungerPoint += value;
    }
    private void DecreaseHunger(int value)
    {
        _hungerPoint -= value;
    }
    private void IncreaseWater(int value)
    {
        _waterPoint += value;
    }
    private void DecreaseWater(int value)
    {
        _waterPoint -= value;
    }

    private void Die()
    {
        Debug.Log("Умер");
    }

    private void OnEnable()
    {
        _playerStatsChanger.HungerIsDecreased += DecreaseHunger;
        _playerStatsChanger.WaterIsDecreased += DecreaseWater;
        _playerStatsChanger.HealthIsDecreased += DecreaseHealth;
        
        
    }
    private void OnDisable()
    {
        _playerStatsChanger.HungerIsDecreased -= DecreaseHunger;
        _playerStatsChanger.WaterIsDecreased -= DecreaseWater;
        _playerStatsChanger.HealthIsDecreased -= DecreaseHealth;
    }

    private void OnApplicationQuit()
    {
        _playerInventory.ItemsContainer.ClearItems();
    }
}
