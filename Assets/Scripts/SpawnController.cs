using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    public List<GameObject> spawnables = new List<GameObject>();
    public int maxDayLivingSpawns = 2;
    public int maxNightLivingSpawns = 5;
    public int maxSimultaneousSpawns = 1;
    public int maxSimultaneousSpawnAttempts = 5;
    public float spawnCheckInterval = 5f;
    public List<GameObject> spawnAround = new List<GameObject>();
    public float minSpawnDistance = 20f;
    public float maxSpawnDistance = 50f;
    public float spawnCheckHeight = 1000f;
    public float spawnCheckDistance = 2000f;

    private List<GameObject> liveSpawns = new List<GameObject>();
    private int maxLivingSpawns;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnCheckCR());
        maxLivingSpawns = Mathf.Max(maxDayLivingSpawns, maxNightLivingSpawns);
    }

    private IEnumerator SpawnCheckCR()
    {
        while (true)
        {
            for (int i = 0; i < liveSpawns.Count; i++)
            {
                if (liveSpawns[i] == null)
                {
                    liveSpawns.RemoveAt(i);
                    i--;
                }
            }

            if (DayNightCycle.cycle != null)
            {
                maxLivingSpawns = DayNightCycle.cycle.IsDayTime() ?
                        maxDayLivingSpawns : maxNightLivingSpawns;
            }

            if (spawnables != null && spawnables.Count > 0 &&
                    spawnAround != null && spawnAround.Count > 0 &&
                    liveSpawns.Count < maxLivingSpawns)
            {
                int toSpawn = Mathf.Min(maxLivingSpawns - liveSpawns.Count,
                        maxSimultaneousSpawns);

                for (int i = 0; i < toSpawn; i++)
                {
                    for (int j = 0; j < maxSimultaneousSpawnAttempts; j++)
                    {
                        int target = Mathf.Clamp((int)(Random.value * spawnAround.Count),
                                0, spawnAround.Count - 1);
                        int spawnee = Mathf.Clamp((int)(Random.value * spawnables.Count),
                                0, spawnables.Count - 1);

                        if (spawnAround[target] == null || spawnables[spawnee] == null)
                            continue;

                        Vector2 loc = Random.insideUnitCircle;
                        loc = loc * (maxSpawnDistance - minSpawnDistance) + 
                                loc.normalized * minSpawnDistance;
                        Vector3 spawnTest = new Vector3(loc.x, spawnCheckHeight, loc.y);

                        RaycastHit hit;
                        if (Physics.Raycast(spawnAround[target].transform.position + spawnTest,
                                Vector3.down, out hit, spawnCheckDistance))
                        {
                            liveSpawns.Add(Instantiate(spawnables[spawnee], hit.point +
                                    new Vector3(0f, 2f, 0f), Quaternion.identity));
                            break;
                        }
                    }
                }
            }

            yield return new WaitForSeconds(spawnCheckInterval);
        }
    }
}
