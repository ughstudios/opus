using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Harvestable : MonoBehaviour
{
    public float ResourceTimerSeconds = 60;
    [SerializeField]
    public Item item;
    private bool bHarvested = false;

    void Start()
    {
        StartCoroutine(ResourceTimer());
    }

    void OnTriggerStay(Collider col)
    {
        if (bHarvested)
        {
            return;
        }

        if (col.gameObject.tag == "Player")
        {
            if (Input.GetButtonDown("Gather"))
            {
                Inventory inventory = col.gameObject.GetComponent<Inventory>();
                Debug.Log("item name: " + item.name);
                inventory.AddItemToInventory(1, item.name);
                bHarvested = true;
            }
        }
    }

    IEnumerator ResourceTimer()
    {
        while (true)
        {
            if (bHarvested)
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

                bHarvested = false;
            }

            yield return new WaitForEndOfFrame();
            
        }
    }
}
