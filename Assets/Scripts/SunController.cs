using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Light))]
public class SunController : MonoBehaviour
{
    new private Light light;

    public Vector3 rotAxis = Vector3.right;
    public Vector3 baseRot = new Vector3(0f, 90f, 0f);

    public Color normal = Color.white;
    public Color twilight = Color.red;

    public float twilightProportion = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        light = GetComponent<Light>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (DayNightCycle.cycle == null)
            return;

        float time = DayNightCycle.cycle.time;
        float dayLength = DayNightCycle.cycle.dayLength;
        float noon = dayLength / 2f;
        float dayTimeProportion = DayNightCycle.cycle.dayTimeProportion;
        float dayStart = (dayLength - dayLength * dayTimeProportion) / 2f;
        float dayEnd = dayLength - dayStart;

        float angle = 0f;

        if (time > dayStart && time < dayEnd)
        {
            angle = 180f * ((dayEnd - time) / (dayEnd - dayStart));
        }
        else if (time < dayStart)
        {
            angle = 180f + 90f * ((dayStart - time) / dayStart);
        }
        else
        {
            angle = -90f * ((time - dayEnd) / (dayLength - dayEnd));
        }

        Vector3 orientation = baseRot + rotAxis * angle;

        transform.eulerAngles = orientation;

        //TODO: Twilight
    }
}
