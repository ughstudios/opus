using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XPCollector : MonoBehaviour
{
    public int xp;
    public int level;

    public void ReceiveXP(int xpIn)
    {
        xp += xpIn;
        CalculateLevel();
    }

    private void CalculateLevel()
    {
        level = (int) Mathf.Sqrt(xp);
    }
}
