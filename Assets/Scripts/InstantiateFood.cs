using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateFood : MonoBehaviour
{

    public GameObject food;

    private GameObject _instance;

    public int foodSpawner = 15;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnFood());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator SpawnFood()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            foodSpawner--;

            Vector3 position = new Vector3(Random.Range(-15.0f, 15.0f), -25, Random.Range(-15.0f, 15.0f));
            _instance = Instantiate(food, position, Quaternion.identity);

            _instance.name = "Food" + foodSpawner--;
            
            Destroy(_instance, 15.0f);

        }
    }
}
