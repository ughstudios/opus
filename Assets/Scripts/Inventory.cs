using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    private Dictionary<Item, int> InventoryItems = new Dictionary<Item, int>();

    public GameObject Inventory_ScrolLView;
    public GameObject UI_Item_Prefab;
    public GameObject Pickup_UI;
    public GameObject Inventory_UI;
    public GameObject InventoryPanel_UI;

    public GameObject UI_InventoryItemPrefab;

    private CharacterController characterController;
    private PlayerController playerController;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
        LoadAllItems();
    }

    private void LoadAllItems()
    {
        Object[] items = Resources.LoadAll("Items", typeof(Item));

        foreach (var i in items)
        {
            InventoryItems.Add((Item)i, 0); // 0 is just to initialize the inventory with all potential items, this is a crappy way to do it but none the less will work for now. Ideally, we would not want any of these items stored in memory unless we need them. This is just a hack coz im lazy.
        }
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

        foreach (KeyValuePair<Item, int> entry in GetInventory())
        {
            if (entry.Value <= 0) // This is necessary because of the LoadAllItems() method called in start. It loads all POTENTIAL items with a count of 0. It's a lazy hack but works for now. Will cause performance issues if we ever had thousands or hundreds of items. Needs to be tested for performance.
            {
                continue;
            }

            GameObject go = Instantiate(UI_InventoryItemPrefab);

            Image image = go.GetComponent<Image>();
            image.sprite = entry.Key.icon;

            TextMeshProUGUI text = go.GetComponentInChildren<TextMeshProUGUI>();
            text.text = "x" + entry.Value;
            go.transform.SetParent(InventoryPanel_UI.transform);

            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(delegate { ItemClicked(entry.Key); });
        }

    }

    private void ItemClicked(Item item)
    {
        Debug.Log(item.name);

        playerController.food += item.food;
        playerController.health += item.health;
        playerController.water += item.water;

        RemoveCountOfItemFromInventory(item, 1);

        GenerateInventoryUI_Items();
    }

    public void AddItemToInventory(int addcount, string itemName)
    {
        Pickup_UI.SetActive(true);

        Item item = GetItemByItemName(itemName);
        Debug.Log(item.description);
        int newValue = InventoryItems[item] += addcount;

        SetNewValueForInventoryItemByItemName(itemName, newValue);

        GameObject ui_item = Instantiate(UI_Item_Prefab);
        TextMeshProUGUI text = ui_item.GetComponent<TextMeshProUGUI>();
        text.color = new Color32(0, 255, 0, 255);
        text.SetText("Added " + item.name + " x" + newValue);

        ui_item.transform.SetParent(Inventory_ScrolLView.transform);

        StartCoroutine(HideAfterInventoryChange(ui_item));
    }

    public int GetItemCountByName(string itemName)
    {
        foreach (KeyValuePair<Item, int> entry in GetInventory())
        {
            if (entry.Key.name == itemName)
            {
                return entry.Value;
            }
        }
        return 0;
    }

    public void RemoveCountOfItemFromInventory(Item item, int count)
    {
        InventoryItems[item] -= count;
    }

    public void SetNewValueForInventoryItemByItemName(string inItemName, int newValue)
    {
        Item item = GetItemByItemName(inItemName);
        InventoryItems[item] = newValue;
    }

    public Item GetItemByItemName(string inItemName)
    {
        foreach (KeyValuePair<Item, int> entry in GetInventory())
        {
            if (entry.Key.name == inItemName)
            {
                return entry.Key;
            }
        }
        return null;
    }

    public Dictionary<Item, int> GetInventory()
    {
        return InventoryItems;
    }

    IEnumerator HideAfterInventoryChange(GameObject ui_item)
    {

        yield return new WaitForSeconds(5);

        Pickup_UI.SetActive(false);
        Destroy(ui_item);
    }

    public Item GetRandomPlantable()
    {
        List<Item> plantables = new List<Item>();
        int numPlantables = 0;
        foreach (KeyValuePair<Item, int> entry in GetInventory())
        {
            if (entry.Key.canPlant && entry.Value > 0)
            {
                plantables.Add(entry.Key);
                numPlantables += entry.Value;
            }
        }

        int num = Mathf.FloorToInt(numPlantables * Random.value);
        if (num >= numPlantables)
            num = numPlantables - 1;

        int sum = 0;
        for (int i = 0; i < numPlantables; i++)
        {
            sum += InventoryItems[plantables[i]];
            if (sum > num)
                return plantables[i];
        }

        return null;
    }
}
