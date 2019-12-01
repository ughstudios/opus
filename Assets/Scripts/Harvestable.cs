using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Harvestable : MonoBehaviour
{
    public float ResourceTimerSeconds = 60;
    [SerializeField]
    public List<Item> items;
    public bool canHarvest = true;
    [Tooltip("Numbers less than 1 are infinite")]
    public int maxSpawns = 0;
    private int numSpawns = 0;

    void OnTriggerStay(Collider col)
    {
        if (!canHarvest)
        {
            return;
        }

        if (col.gameObject.tag == "Player")
        {
            if (Input.GetButtonDown("Gather"))
            {
                Inventory inventory = col.gameObject.GetComponent<Inventory>();
                foreach (Item item in items)
                {
                    Debug.Log("item name: " + item.name);
                    inventory.AddItemToInventory(1, item.name);
                }
                canHarvest = false;

                if (maxSpawns > 0 && numSpawns >= maxSpawns)
                {
                    Destroy(gameObject);
                }
                else
                {
                    StartCoroutine(RespawnTimer());
                }
            }
        }
    }

    private IEnumerator RespawnTimer()
    {
        Renderer[] rs = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rs)
        {
            r.enabled = false;
        }

        yield return new WaitForSeconds(ResourceTimerSeconds);

        foreach (Renderer r in rs)
        {
            r.enabled = true;
        }
        canHarvest = true;
        numSpawns++;
    }
}
