using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[CreateAssetMenu(fileName = "New Biome", menuName ="Biome")]
public class Biome : ScriptableObject
{
    public new string name;
    public TerrainLayer terrainLayer;
    public int relativeFrequency = 1;
    public int heightmapOctaves = 8;

    public float minHeight = 0f;
    public float maxHeight = 1f;
    public float heightExponent = 1f;

    public List<DetailPrototypeData> detailPrototypes =
            new List<DetailPrototypeData>();
    public List<TreePrototypeData> treePrototypes =
            new List<TreePrototypeData>();

    public PostProcessProfile postProcessing;

    public GameObject ambientPrefab;
}
