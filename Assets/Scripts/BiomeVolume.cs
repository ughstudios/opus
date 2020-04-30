using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeVolume : MonoBehaviour
{
    private TerrainManager.BiomeData biomeData;      

    void Start()
    {
        biomeData = GetComponent<TerrainManager.BiomeData>();
        //Debug.Log("test");

        if (biomeData)
        {
            
        }
        
    }





}
