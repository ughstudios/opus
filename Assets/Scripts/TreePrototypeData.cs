using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tree Prototype Data",
        menuName = "TreePrototypeData")]
public class TreePrototypeData : ScriptableObject
{
    public new string name;

    public GameObject prefab;
    public float bendFactor;

    public Color color = Color.white;
    public Color lightmapColor = Color.white;
    public float minHeightScale = 0.75f;
    public float minWidthScale = 0.75f;
    public float maxHeightScale = 1.25f;
    public float maxWidthScale = 1.25f;

    public float minDistance = 1f;
	public int relativeFrequency = 1;
}
