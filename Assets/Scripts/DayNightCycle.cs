using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle cycle = null;

    public float dayLength = 600f;
    public float dayTimeProportion = 0.5f;

    public float time = 0f;
    public int numDays = 0;

    void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        if (time > dayLength)
        {
            time -= dayLength;
            numDays++;
        }
    }

    public bool IsDayTime()
    {
        float dayStart = dayLength / 2f - dayLength * dayTimeProportion / 2f;
        //float dayEnd = dayLength / 2f + dayLength * dayTimeProportion / 2f;
        float dayEnd = dayLength - dayStart;
        return time > dayStart && time < dayEnd;
    }

    public bool IsNightTime()
    {
        return !IsDayTime();
    }

    private void OnDisable()
    {
        cycle = null;
    }

    private void OnEnable()
    {
        if (cycle != null)
        {
            Debug.LogWarningFormat("{0} OnEnable: cycle == {1} ; Overriding", this, cycle);
        }
        cycle = this;
    }
}
