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
        light.type = LightType.Directional;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (DayNightCycle.cycle == null)
            return;

        // Sun angle
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

        // Twilight color-change
        float dayTimeLength = dayEnd - dayStart;
        float twilightTime = dayTimeLength * twilightProportion;

        if (time > dayStart + twilightTime && time < dayEnd - twilightTime)
        {
            light.color = normal;
        }
        else if (time < dayStart - twilightTime || time > dayEnd + twilightTime)
        {
            light.color = Color.black;
        }
        else if (time > dayStart && time < dayStart + twilightTime)
        {
            light.color = Color.Lerp(twilight, normal, (time - dayStart) / twilightTime);
        }
        else if (time > dayEnd - twilightTime && time < dayEnd)
        {
            light.color = Color.Lerp(normal, twilight, (time - (dayEnd - twilightTime)) / twilightTime);
        }
        else if (time > dayStart - twilightTime && time < dayStart)
        {
            light.color = Color.Lerp(Color.black, twilight, 
                    (time - (dayStart - twilightTime)) / twilightTime);
        }
        else if (time > dayEnd && time < dayEnd + twilightTime)
        {
            light.color = Color.Lerp(twilight, Color.black, (time - dayEnd) / twilightTime);
        }
    }
}
