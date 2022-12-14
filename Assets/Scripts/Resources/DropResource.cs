
using UnityEngine;

public class DropResource : MonoBehaviour
{
    [SerializeField] private ItemsDataBase _itemsDataBase;

    [SerializeField] private GameObject _player;
    
    public void DropItem(Item item, int amount)
    {
        for (int i = 0; i < _itemsDataBase.ItemsData.Length; i++)
        {
            if (_itemsDataBase.ItemsData[i].Data.ID == item.ID)
            {

                var prefab = _itemsDataBase.ItemsData[i].Prefab;

                var position = _player.transform.position;
                var obj = Instantiate(prefab, new Vector3(position.x, position.y + 2, position.z), Quaternion.identity);
                var component = obj.TryGetComponent(out GroundItem groundItem);
                
                obj.GetComponent<Rigidbody>().AddForce(Vector3.back * 3, ForceMode.Impulse);
                obj.GetComponent<Rigidbody>().AddForce(Vector3.down * 3, ForceMode.Impulse);
                
                if (component == true)
                {
                    obj.GetComponent<GroundItem>().SetAmount(amount);
                    obj.GetComponent<GroundItem>().SetItem(_itemsDataBase.ItemsData[i]);
                }
            }
        }
    }
    public void PlaceItem(Item item, int amount, Vector3 position)
    {
        for (int i = 0; i < _itemsDataBase.ItemsData.Length; i++)
        {
            if (_itemsDataBase.ItemsData[i].Data.ID == item.ID)
            {
                var prefab = _itemsDataBase.ItemsData[i].Prefab;
                
                var obj = Instantiate(prefab, new Vector3(position.x, position.y, position.z), Quaternion.identity);
                
                var component = obj.TryGetComponent(out GroundItem groundItem);
                if (component == true)
                {
                    obj.GetComponent<GroundItem>().SetAmount(amount);
                    obj.GetComponent<GroundItem>().SetItem(_itemsDataBase.ItemsData[i]);
                }
            }
        }
    }
}
