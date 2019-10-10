using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{

    public ItemData item = new ItemData();


    void OnTriggerEnter(Collider col)
    {
        GameObject go = col.gameObject;
        Inventory inventory = go.GetComponent<Inventory>();
        inventory.AddItemToInventory(item);

        Destroy(gameObject);
    }


}
