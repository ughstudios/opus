using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Harvestable : MonoBehaviour
{
    public float ResourceTimerSeconds = 60;
    [SerializeField]
    public ItemData item = new ItemData();
    public bool bHarvested = false;

    void Start()
    {
        StartCoroutine(ResourceTimer());
    }

    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (Input.GetButtonDown("Gather"))
            {
                Inventory inventory = col.gameObject.GetComponent<Inventory>();
                inventory.AddItemToInventory(item);
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
                GetComponentsInChildren(MeshCollider)
                yield return new WaitForSeconds(ResourceTimerSeconds);
                gameObject.SetActive(true);
            }

            yield return new WaitForEndOfFrame();
            
        }
    }
}
