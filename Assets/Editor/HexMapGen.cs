#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HexMapGen : EditorWindow
{
    public GameObject hexPrefab;
    public float radius;
    private bool ranGen = false;

    [MenuItem("Window/HexMapGen")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<HexMapGen>();
    }

    private void OnGUI()
    {
        hexPrefab = (GameObject)EditorGUILayout.ObjectField("Hex", hexPrefab,
                typeof(GameObject), false);
        radius = EditorGUILayout.FloatField("Radius", radius);

        if (GUILayout.Button("Generate"))
        {
            if (!ranGen)
            {
                ranGen = true;
                Generate();
            }
        }
        else
            ranGen = false;
    }

    private void Generate()
    {
        GameObject tempHex = GameObject.Instantiate<GameObject>(hexPrefab);
        Collider col = tempHex.GetComponentInChildren<Collider>();
        if (col == null)
            return;
        Bounds bounds = col.bounds;

        Debug.LogFormat("Bounds: {0}", bounds);

        GameObject root = new GameObject("HexGenRoot");
        float xLen = bounds.size.x;
        float zLen = bounds.size.z;
        if (zLen < xLen)
        {
            float sideLen = (zLen / 2f) / Mathf.Cos(60f * Mathf.Deg2Rad);
            float shortLen = (xLen - sideLen) / 2f;
            float medLen = zLen / 2f;

            int steps = Mathf.CeilToInt(radius/zLen) + 1;

            for (int x = -steps; x <= steps; x++)
                for (int z = -steps; z <= steps; z++)
                {
                    Vector3 loc = new Vector3((sideLen + shortLen) * x, 0f,
                            2 * medLen * z + ((x % 2 == 0) ? 0f : medLen));
                    if (loc.x * loc.x + loc.z * loc.z < radius * radius)
                    {
                        GameObject hexHold = new GameObject("HexHolder");
                        hexHold.transform.parent = root.transform;
                        hexHold.transform.localPosition = loc;
                        GameObject hex = (GameObject) PrefabUtility.
                                InstantiatePrefab(hexPrefab, hexHold.transform);
                    }
                }
        }
        if (xLen < zLen)
        {
            float sideLen = (xLen / 2f) / Mathf.Cos(60f * Mathf.Deg2Rad);
            float shortLen = (zLen - sideLen) / 2f;
            float medLen = xLen / 2f;

            int steps = Mathf.CeilToInt(radius / xLen) + 1;

            for (int x = -steps; x <= steps; x++)
                for (int z = -steps; z <= steps; z++)
                {
                    Vector3 loc = new Vector3(2 * medLen * x + ((z % 2 == 0) ? 0f : medLen),
                            0f, (sideLen + shortLen) * z);
                    if (loc.x * loc.x + loc.z * loc.z < radius * radius)
                    {
                        GameObject hexHold = new GameObject("HexHolder");
                        hexHold.transform.parent = root.transform;
                        hexHold.transform.localPosition = loc;
                        GameObject hex = (GameObject)PrefabUtility.
                                InstantiatePrefab(hexPrefab, hexHold.transform);
                    }
                }
        }
        DestroyImmediate(tempHex);
    }
}
#endif
