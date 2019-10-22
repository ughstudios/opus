using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    private List<ItemData> InventoryItems = new List<ItemData>();

    public GameObject Inventory_ScrolLView;
    public GameObject UI_Item_Prefab;
    public GameObject Inventory_UI;

    public void AddItemToInventory(ItemData item)
    {
        Inventory_UI.SetActive(true);

        ItemData theFoundItem = new ItemData();
        bool itemFound = GetItemByItemName(item.itemName, ref theFoundItem);
        if (itemFound)
        {
            theFoundItem.itemCount += item.itemCount;
        }
        else
        {
            InventoryItems.Add(item);
        }

        GameObject ui_item = Instantiate(UI_Item_Prefab);
        TextMeshProUGUI text = ui_item.GetComponent<TextMeshProUGUI>();
        text.color = new Color32(0, 255, 0, 255);
        text.SetText("Added " + item.itemName + " x" + item.itemCount);


        ui_item.transform.SetParent(Inventory_ScrolLView.transform);

        StartCoroutine(HideAfterInventoryChange(ui_item));
    }

    public void RemoveCountOfItemFromInventory(int removecount, ItemData item)
    {

        Inventory_UI.SetActive(false);


        ItemData theFoundItem = new ItemData();
        bool itemFound = GetItemByItemName(item.itemName, ref theFoundItem);
        if (itemFound)
        {
            theFoundItem.itemCount -= removecount;
            if (theFoundItem.itemCount <= 0)
            {
                InventoryItems.Remove(theFoundItem);
            }
        }

        GameObject ui_item = Instantiate(UI_Item_Prefab);
        TextMeshProUGUI text = ui_item.GetComponent<TextMeshProUGUI>();
        text.color = new Color32(255, 0, 0, 255);
        text.SetText("Removed " + item.itemName + " x" + item.itemCount);

        ui_item.transform.SetParent(Inventory_ScrolLView.transform);

        StartCoroutine(HideAfterInventoryChange(ui_item));

    }

    public bool GetItemByItemName(string inItemName, ref ItemData itemOut)
    {
        foreach (ItemData item in GetInventory())
        {
            if (item.itemName == inItemName)
            {

                itemOut = item;
                return true;
            }
        }

        return false;
    }

    public List<ItemData> GetInventory()
    {
        return this.InventoryItems;
    }

    IEnumerator HideAfterInventoryChange(GameObject ui_item)
    {

        yield return new WaitForSeconds(5);

        Inventory_UI.SetActive(false);
        Destroy(ui_item);
    }


}
