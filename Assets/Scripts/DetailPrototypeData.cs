using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Detail Prototype Data",
        menuName = "DetailPrototypeData")]
public class DetailPrototypeData : ScriptableObject
{
    public new string name;

    public float bendFactor;
    public Color dryColor;
    public Color healthyColor;
    public float maxHeight;
    public float maxWidth;
    public float minHeight;
    public float minWidth;
    public float noiseSpread;
    public GameObject prototype;
    public Texture2D prototypeTexture;
    public DetailRenderMode renderMode;

    public int minDensity;
    public int maxDensity;
}
