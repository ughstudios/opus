using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateWater : MonoBehaviour
{
    public GameObject water;

    private GameObject _instance;

    public int waterSpawner = 15;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnWater());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SpawnWater()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            waterSpawner--;

            Vector3 position = new Vector3(Random.Range(-15.0f, 15.0f), 25, Random.Range(-15.0f, 15.0f));
            _instance = Instantiate(water, position, Quaternion.identity);

            Destroy(_instance, 15.0f);

        }



    }
}
