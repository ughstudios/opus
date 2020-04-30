using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using AmbientSounds;

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

    [Tooltip("Detail prefabs to use in this biome")]
    public List<DetailPrototypeData> detailPrototypes =
            new List<DetailPrototypeData>();

    [Tooltip("Tree prefabs to use in t his biome")]
    public List<TreePrototypeData> treePrototypes =
            new List<TreePrototypeData>();

    [Tooltip("Post processing profile to use for this biome.")]
    public PostProcessProfile postProcessing;
    
    [Tooltip("Sounds that will be played globally in this biome")]
    public Sequence[] globalSounds;

    [Tooltip("Prefab that is spawned in the center of the biome.")]
    public GameObject ambientPrefab;

}
