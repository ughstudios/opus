using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public string seed;

    public float noiseScale = 1000f;
    public TerrainSettings genSettings = new TerrainSettings();

    public bool automaticUpdatesStart = true;
    public float updateInterval = 5f;
    public int loadedSectionRadius = 5;
    public int numOctaves = 8;
    public float maxOffset = 32f;

    public int maxGenThreads = 8;
    public List<GameObject> follow = new List<GameObject>();
    public List<TerrainLayer> terrainLayers = new List<TerrainLayer>();

    private Dictionary<SectionCoord, TerrainSection> terrains =
        new Dictionary<SectionCoord, TerrainSection>();
    private Queue<SectionCoord> toCreate = new Queue<SectionCoord>();
    private Queue<SectionCoord> toRemove = new Queue<SectionCoord>();

    private bool automaticUpdates;
    private bool updateRunning = false;
    private bool createRunning = false;
    private bool removeRunning = false;
    private int numGenThreads = 0;

    private int seedHash;
    private float[,] offsets;

    [System.Serializable]
    public class TerrainSettings
    {
        public float length = 64f;
        public float height = 60f;
        public int heightMapRes = 257;
        public int alphaMapRes = 257;
    }

    private class TerrainSection
    {
        public SectionCoord coord;
        public Terrain terrain;

        public TerrainSection(SectionCoord coord = null, Terrain terrain = null)
        {
            this.coord = coord;
            this.terrain = terrain;
        }
    }

    private class SectionCoord
    {
        public int x, z;

        public SectionCoord(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public override bool Equals(object obj)
        {
            SectionCoord other = obj as SectionCoord;

            return other != null && other.x == x && other.z == z;
        }

        public override int GetHashCode()
        {
            return x ^ ((z << 16) ^ (z >> 16));
        }
    }

    // Collection of pseudorandom helpers, mostly from Wikipedia
    private class NotRandom
    {
        private static ulong state = 0x4d595df4d0f33173;
        private static ulong mult = 6364136223846793005u;
        private static ulong inc = 1442695040888963407u;

        public static float Value()
        {
            unchecked
            {
                ulong val = state;
                int i = (int)(val >> 59);
                state = val * mult + inc;

                val ^= val >> 18;
                val >>= 27;
                uint rnd = (uint)(val >> i | val << (-i & 31));
                return (float)rnd / System.UInt32.MaxValue;
            }
        }

        public static float Value(int seed)
        {
            unchecked
            {
                state = (ulong)seed + inc;
                Value();
                return Value();
            }
        }

        public static int HashString(string toHash)
        {
            unchecked
            {
                int hash = 0;

                for (int i = 0; i < toHash.Length; i++)
                {
                    hash += toHash[i];
                    hash += hash << 10;
                    hash ^= hash >> 6;
                }
                hash += hash << 3;
                hash ^= hash >> 11;
                hash += hash << 15;

                return hash;
            }
        }
    }

    private void Start()
    {
        if (seed == null)
            seed = "";
        seedHash = NotRandom.HashString(seed);

        offsets = new float[numOctaves, 2];
        NotRandom.Value(seedHash);

        for (int i = 0; i < numOctaves; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                offsets[i, j] = (NotRandom.Value() * maxOffset * 2) - maxOffset;
            }
        }

        SetAutomaticUpdates(automaticUpdatesStart);
    }

    public void SetAutomaticUpdates(bool automatic)
    {
        automaticUpdates = automatic;
        if (automaticUpdates && !updateRunning)
        {
            updateRunning = true;
            StartCoroutine(AutomaticUpdate_CR());
        }
    }

    public bool UpdateTerrains()
    {
        Dictionary<SectionCoord, TerrainSection> needed = new Dictionary<SectionCoord, TerrainSection>();

        for (int i = 0; i < follow.Count; i++)
        {
            if (follow[i] == null)
                continue;
            List<SectionCoord> coords = SectionsInRadius(SectionFor(follow[i].transform.position),
                    loadedSectionRadius);
            foreach (SectionCoord loc in coords)
                needed.Add(loc, null);
        }

        List<SectionCoord> exists = terrains.Keys.ToList();
        List<SectionCoord> need = needed.Keys.ToList();
        List<SectionCoord> toGen = need.Except(exists).Except(toCreate).Except(toRemove).ToList();
        List<SectionCoord> remove = exists.Except(need).Except(toRemove).ToList();

        if (toGen.Count > 0)
        {
            foreach (SectionCoord loc in toGen)
                toCreate.Enqueue(loc);
            StartCreateCR();
        }

        if (remove.Count > 0)
        {
            foreach (SectionCoord loc in remove)
                toRemove.Enqueue(loc);
            StartRemoveCR();
        }

        bool updated = toGen.Count + remove.Count > 0;

        return updated;
    }

    private IEnumerator AutomaticUpdate_CR()
    {
        yield return null;
        while (automaticUpdates)
        {
            UpdateTerrains();
            yield return new WaitForSeconds(updateInterval);
        }
        updateRunning = false;
    }

    private IEnumerator Create_CR()
    {
        while (toCreate.Count > 0)
        {
            while (numGenThreads < maxGenThreads && toCreate.Count > 0)
            {
                numGenThreads++;
                StartCoroutine(GenerateSection_CR(toCreate.Dequeue()));
            }
            yield return new WaitUntil(() => numGenThreads < maxGenThreads);
        }
        createRunning = false;
    }

    private IEnumerator GenerateSection_CR(SectionCoord coord)
    {
        TerrainSection sec;
        if (terrains.TryGetValue(coord, out sec) && sec != null &&
                sec.terrain != null)
        {
            if (numGenThreads > 0)
                numGenThreads--;
            yield break;
        }
        if (sec == null)
            sec = new TerrainSection(coord);

        float[,] heightmap = null;
        float[,,] alphamaps = null;
        TerrainData data = new TerrainData();
        data.heightmapResolution = genSettings.heightMapRes;
        data.alphamapResolution = genSettings.alphaMapRes;
        data.size = new Vector3(genSettings.length, genSettings.height,
                genSettings.length);

        bool done = false;
        Thread thread = new Thread(() =>
                {
                    heightmap = GenerateHeightmap(coord);
                    alphamaps = GenerateAlphamaps(coord, heightmap);
                    done = true;
                });
        thread.Start();
        yield return new WaitUntil(() => done);

        data.SetHeights(0, 0, heightmap);
        data.terrainLayers = terrainLayers.ToArray();
        data.SetAlphamaps(0, 0, alphamaps);

        GameObject obj = Terrain.CreateTerrainGameObject(data);
        obj.transform.position = new Vector3(coord.x * genSettings.length, 0f,
                coord.z * genSettings.length);
        sec.terrain = obj.GetComponent<Terrain>();
        sec.terrain.allowAutoConnect = true;
        sec.terrain.Flush();
        terrains.Add(coord, sec);
        if (numGenThreads > 0)
            numGenThreads--;
    }

    private IEnumerator Remove_CR()
    {
        while (toRemove.Count > 0)
        {
            for (int i = 0; i < maxGenThreads && toRemove.Count > 0; i++)
            {
                SectionCoord coord = toRemove.Dequeue();
                TerrainSection sec = terrains[coord];
                if (sec != null && sec.terrain != null)
                {
                    Destroy(sec.terrain.gameObject);
                    terrains.Remove(coord);
                }
            }
            yield return null;
        }
        removeRunning = false;
    }

    private void StartCreateCR()
    {
        if (!createRunning)
        {
            createRunning = true;
            StartCoroutine(Create_CR());
        }
    }

    private void StartRemoveCR()
    {
        if (!removeRunning)
        {
            removeRunning = true;
            StartCoroutine(Remove_CR());
        }
    }

    private float[,] GenerateHeightmap(SectionCoord coord)
    {
        float[,] heightmap = new float[genSettings.heightMapRes,
                genSettings.heightMapRes];

        float height, nx, nz;
        for (int x = 0; x < genSettings.heightMapRes; x++)
            for (int z = 0; z < genSettings.heightMapRes; z++)
            {
                nx = coord.x - 0.5f + (float)x / genSettings.heightMapRes;
                nz = coord.z - 0.5f + (float)z / genSettings.heightMapRes;
                nx *= genSettings.heightMapRes / noiseScale;
                nz *= genSettings.heightMapRes / noiseScale;
                height = 0f;
                for (int i = 0; i < numOctaves; i++)
                {
                    height += Mathf.Pow(2f, -i) * Mathf.PerlinNoise(
                            nx * Mathf.Pow(2, i) + offsets[i, 0],
                            nz * Mathf.Pow(2, i) + offsets[i, 1]);
                }

                heightmap[z, x] = height / (2f - Mathf.Pow(2, -(numOctaves - 1)));
            }

        return heightmap;
    }

    private float[,,] GenerateAlphamaps(SectionCoord coord, float[,] heightmap)
    {
        float[,,] alphamaps = new float[genSettings.alphaMapRes,
                genSettings.alphaMapRes, terrainLayers.Count];

        for (int x = 0; x < genSettings.alphaMapRes; x++)
            for (int z = 0; z < genSettings.alphaMapRes; z++)
            {
                alphamaps[x, z, 0] = 1f;
            }

        return alphamaps;
    }

    private List<SectionCoord> SectionsInRadius(SectionCoord coord, int radius)
    {
        List<SectionCoord> sections = new List<SectionCoord>();

        sections.Add(coord);

        for (int x = -radius; x <= radius; x++)
            for (int z = -radius; z <= radius; z++)
            {
                if (x == 0 && z == 0)
                    continue;
                if (x * x + z * z < radius * radius + 1)
                    sections.Add(new SectionCoord(coord.x + x, coord.z + z));
            }

        return sections;
    }

    private SectionCoord SectionFor(Vector3 vLoc)
    {
        return new SectionCoord(Mathf.FloorToInt(vLoc.x / genSettings.length),
                Mathf.FloorToInt(vLoc.z / genSettings.length));
    }

    private bool SectionExists(SectionCoord coord)
    {
        return terrains.ContainsKey(coord) && !toRemove.Contains(coord);
    }

    public bool TerrainExistsAt(Vector3 vLoc)
    {
        return SectionExists(SectionFor(vLoc));
    }

    public float TerrainHeightAt(Vector3 vLoc)
    {
        SectionCoord coord = SectionFor(vLoc);
        if (!SectionExists(coord))
            return 0f;
        TerrainSection sec = terrains[coord];
        if (sec == null || sec.terrain == null)
            return 0f;
        return sec.terrain.SampleHeight(vLoc);
    }

    public void DestroyAll()
    {
        foreach (SectionCoord coord in terrains.Keys)
        {
            if (!toRemove.Contains(coord))
                toRemove.Enqueue(coord);
        }

        StartRemoveCR();
    }
}
