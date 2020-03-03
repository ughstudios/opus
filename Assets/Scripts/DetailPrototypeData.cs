using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Detail Prototype Data",
        menuName = "DetailPrototypeData")]
public class DetailPrototypeData : ScriptableObject
{
    public new string name;

    public float bendFactor;
    public Color dryColor = Color.white;
    public Color healthyColor = Color.white;
    public float maxHeight = 1f;
    public float maxWidth = 1f;
    public float minHeight = 0.1f;
    public float minWidth = 0.1f;
    public float noiseSpread;
    public GameObject prototype;
    public Texture2D prototypeTexture;
    public DetailRenderMode renderMode;

    public int minDensity;
    public int maxDensity;
}
