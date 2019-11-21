using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    private List<ItemData> InventoryItems = new List<ItemData>();

    public GameObject Inventory_ScrolLView;
    public GameObject UI_Item_Prefab;
    public GameObject Pickup_UI;
    public GameObject Inventory_UI;
    public GameObject InventoryPanel_UI;

    public GameObject UI_InventoryItemPrefab;

    private CharacterController characterController;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }
        
    private void Update()
    {
        if (Input.GetButtonDown("OpenInventory"))
        {
            Inventory_UI.SetActive(!Inventory_UI.activeSelf);
            if (Inventory_UI.activeSelf)
            {
                GenerateInventoryUI_Items();
            }
        }
    }

    private void GenerateInventoryUI_Items()
    {
        // Destroy all child inventory item widgets.
        foreach (Transform child in InventoryPanel_UI.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (ItemData item in GetInventory())
        {
            GameObject go = Instantiate(UI_InventoryItemPrefab);

            Image image = go.GetComponent<Image>();
            image.sprite = item.itemIcon;

            TextMeshProUGUI text = go.GetComponentInChildren<TextMeshProUGUI>();
            text.text = "x" + item.itemCount;
            go.transform.SetParent(InventoryPanel_UI.transform);

            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(delegate{ItemClicked(item);});
        }

    }

    private void ItemClicked(ItemData item)
    {
        Debug.Log(item.itemName);
        if (item.itemName == "Food")
        {
            RemoveCountOfItemFromInventory(1, item);
            
        }
    }

    public void AddItemToInventory(ItemData item)
    {
        Pickup_UI.SetActive(true);

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

        Pickup_UI.SetActive(false);
        
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

        Pickup_UI.SetActive(false);
        Destroy(ui_item);
    }


}
