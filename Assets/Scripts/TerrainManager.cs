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

    public List<Biome> biomes;
    public float biomeCenterSpacing = 250f;
    public float maxBiomeCenterOffset = 100f;
    public float biomeBlend = 50f;

    private Dictionary<SectionCoord, TerrainSection> terrains =
        new Dictionary<SectionCoord, TerrainSection>();
    private Queue<SectionCoord> toCreate = new Queue<SectionCoord>();
    private Queue<SectionCoord> toRemove = new Queue<SectionCoord>();
    private List<SectionCoord> generating = new List<SectionCoord>();

    private bool automaticUpdates;
    private bool updateRunning = false;
    private bool createRunning = false;
    private bool removeRunning = false;
    private int numGenThreads = 0;

    private int seedHash;
    private float[,] offsets;

    private int totalBiomeFrequency = 0;

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
        public class RNG
        {
            private ulong state = 0x4d595df4d0f33173;
            private ulong mult = 6364136223846793005u;
            private ulong inc = 1442695040888963407u;

            public RNG(int seed)
            {
                unchecked
                {
                    state = (ulong)seed + inc;
                    ValueUInt();
                }
            }

            public uint ValueUInt()
            {
                unchecked
                {
                    ulong val = state;
                    int i = (int)(val >> 59);
                    state = val * mult + inc;

                    val ^= val >> 18;
                    val >>= 27;
                    return (uint)(val >> i | val << (-i & 31));
                }
            }

            public float Value()
            {
                return (float)ValueUInt() / System.UInt32.MaxValue;
            }

            public int ValueInt()
            {
                uint val = ValueUInt();
                if (val > System.Int32.MaxValue)
                    return (int)(val - System.Int32.MaxValue) * -1;
                return (int)val;
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

        public static int Hash2Int(int i, int j)
        {
            return i ^ ((j << 16) ^ (j >> 16));
        }
    }

    private void Start()
    {
        if (seed == null)
            seed = "";
        seedHash = NotRandom.HashString(seed);

        offsets = new float[numOctaves, 2];
        NotRandom.RNG rng = new NotRandom.RNG(seedHash);

        for (int i = 0; i < numOctaves; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                offsets[i, j] = (rng.Value() * maxOffset * 2) - maxOffset;
            }
        }

        if (biomes == null || biomes.Count == 0)
        {
            biomes = Resources.LoadAll<Biome>("Biomes").ToList<Biome>();
        }

        if (biomes != null && biomes.Count > 0)
        {
            for (int i = 0; i < biomes.Count; i++)
                totalBiomeFrequency += biomes[i].relativeFrequency;
        }

        SetAutomaticUpdates(automaticUpdatesStart);

        Dictionary<Biome, float> test = GetBiomes(new Vector3(125f, 0f, 125f));

        //foreach (KeyValuePair<Biome, float> kvp in test)
        //    Debug.Log(kvp.ToString());
        //Debug.Log(totalBiomeFrequency);
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
        Dictionary<SectionCoord, TerrainSection> needed = 
                new Dictionary<SectionCoord, TerrainSection>();

        for (int i = 0; i < follow.Count; i++)
        {
            if (follow[i] == null)
                continue;
            List<SectionCoord> coords = SectionsInRadius(SectionFor(
                    follow[i].transform.position), loadedSectionRadius);
            foreach (SectionCoord loc in coords)
                needed.Add(loc, null);
        }

        List<SectionCoord> exists = terrains.Keys.ToList();
        List<SectionCoord> need = needed.Keys.ToList();
        List<SectionCoord> toGen = need.Except(exists).Except(toCreate).
                Except(toRemove).Except(generating).ToList();
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
                SectionCoord coord = toCreate.Dequeue();
                StartCoroutine(GenerateSection_CR(coord));
                generating.Add(coord);
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
        List<Biome> containedBiomes = null;
        Thread thread = new Thread(() =>
                {
                    heightmap = GenerateHeightmap(coord);
                    alphamaps = GenerateAlphamaps(coord, heightmap, out containedBiomes);
                    done = true;
                });
        thread.Start();
        yield return new WaitUntil(() => done);

        TerrainLayer[] terrainLayers = new TerrainLayer[containedBiomes.Count];

        for (int i = 0; i < containedBiomes.Count; i++)
            terrainLayers[i] = containedBiomes[i].terrainLayer;

        data.SetHeights(0, 0, heightmap);
        data.terrainLayers = terrainLayers;
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
        generating.Remove(coord);
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

    private Dictionary<Biome, float> GetBiomes(Vector3 vLoc)
    {
        Dictionary<Biome, float> output = new Dictionary<Biome, float>();

        Vector2 loc = new Vector2(vLoc.x, vLoc.z);
        Vector2 biomeLoc = loc / biomeCenterSpacing;

        Vector2Int[] seedLocs = {
                new Vector2Int(Mathf.FloorToInt(biomeLoc.x), Mathf.FloorToInt(biomeLoc.y)),
                new Vector2Int(Mathf.FloorToInt(biomeLoc.x), Mathf.CeilToInt(biomeLoc.y)),
                new Vector2Int(Mathf.CeilToInt(biomeLoc.x), Mathf.FloorToInt(biomeLoc.y)),
                new Vector2Int(Mathf.CeilToInt(biomeLoc.x), Mathf.CeilToInt(biomeLoc.y))
        };

        Vector2[] centerOffsets = new Vector2[4];
        Biome[] locBiomes = new Biome[4];

        for (int i = 0; i < 4; i++)
        {
            NotRandom.RNG rng = new NotRandom.RNG(NotRandom.Hash2Int(seedHash,
                    NotRandom.Hash2Int(seedLocs[i].x, seedLocs[i].y)));
            centerOffsets[i].x = (rng.Value() * maxBiomeCenterOffset * 2) - 
                    maxBiomeCenterOffset;
            centerOffsets[i].y = (rng.Value() * maxBiomeCenterOffset * 2) -
                    maxBiomeCenterOffset;
            //int index = (int)(rng.ValueUInt() % totalBiomeFrequency);
            int index = Mathf.FloorToInt(rng.Value() * totalBiomeFrequency);
            for (int j = 0; j < biomes.Count; j++)
            {
                if (index >= 0 && index < biomes[j].relativeFrequency)
                {
                    locBiomes[i] = biomes[j];
                    break;
                }
                index -= biomes[j].relativeFrequency;
            }
        }

        float[] dists = new float[4];
        int[] order = { 0, 1, 2, 3 };

        for (int i = 0; i < 4; i++)
        {
            dists[i] = Vector2.Distance(loc, new Vector2(
                    seedLocs[i].x * biomeCenterSpacing + centerOffsets[i].x,
                    seedLocs[i].y * biomeCenterSpacing + centerOffsets[i].y));
        }

        for (int i = 1; i < 4; i++)
            for (int j = i; j > 0 && dists[order[j - 1]] > dists[order[j]]; j--)
            {
                int k = order[j];
                order[j] = order[j - 1];
                order[j - 1] = k;
            }

        int extra = 3;

        for (; extra >= 0; extra--)
        {
            if (dists[order[extra]] < dists[order[0]] + biomeBlend)
                break;
        }

        //output.Add(locBiomes[order[0]], 1f);

        for (int i = 1; i <= extra; i++)
        {
            float val = 1f - ((dists[order[i]] - dists[order[0]]) / biomeBlend);

            if (output.ContainsKey(locBiomes[order[i]]))
            {
                if (output[locBiomes[order[i]]] < val)
                    output[locBiomes[order[i]]] = val;
            }
            else
                output.Add(locBiomes[order[i]], val);
        }

        float sum = 0f;
        foreach (float val in output.Values)
        {
            sum += val;
        }

        if (!output.ContainsKey(locBiomes[order[0]]))
        {
            output.Add(locBiomes[order[0]], 1f - sum);
        }
        else
            output[locBiomes[order[0]]] += 1f - sum;

        return output;
    }

    private float[,] GenerateHeightmap(SectionCoord coord)
    {
        float[,] heightmap = new float[genSettings.heightMapRes,
                genSettings.heightMapRes];

        float height, nx, nz;
        for (int x = 0; x < genSettings.heightMapRes; x++)
            for (int z = 0; z < genSettings.heightMapRes; z++)
            {
                nx = coord.x - 0.5f + (float)x / (genSettings.heightMapRes - 1);
                nz = coord.z - 0.5f + (float)z / (genSettings.heightMapRes - 1);
                nx *= genSettings.length / noiseScale;
                nz *= genSettings.length / noiseScale;
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

    private float[,,] GenerateAlphamaps(SectionCoord coord,
            float[,] heightmap, out List<Biome> containedBiomes)
    {
        containedBiomes = new List<Biome>();
        Dictionary<Biome, float> locBiomes = null;

        Vector3[] biomeTest = { new Vector3((coord.x - 0.5f) *
                genSettings.length / noiseScale, 0f, (coord.z - 0.5f) *
                genSettings.length / noiseScale),
                new Vector3((coord.x - 0.5f) *
                genSettings.length / noiseScale, 0f, (coord.z + 0.5f) *
                genSettings.length / noiseScale),
                new Vector3((coord.x + 0.5f) *
                genSettings.length / noiseScale, 0f, (coord.z - 0.5f) *
                genSettings.length / noiseScale),
                new Vector3((coord.x + 0.5f) *
                genSettings.length / noiseScale, 0f, (coord.z + 0.5f) *
                genSettings.length / noiseScale)
        };

        for (int i = 0; i < 4; i++)
        {
            locBiomes = GetBiomes(biomeTest[i]);
            foreach (Biome b in locBiomes.Keys)
            {
                if (!containedBiomes.Contains(b))
                    containedBiomes.Add(b);
            }
        }

        float[,,] alphamaps = new float[genSettings.alphaMapRes,
                genSettings.alphaMapRes, containedBiomes.Count];

        float nx, nz;
        
        for (int x = 0; x < genSettings.alphaMapRes; x++)
            for (int z = 0; z < genSettings.alphaMapRes; z++)
            {
                nx = coord.x - 0.5f + (float)x / (genSettings.alphaMapRes - 1);
                nz = coord.z - 0.5f + (float)z / (genSettings.alphaMapRes - 1);
                nx *= genSettings.length / noiseScale;
                nz *= genSettings.length / noiseScale;

                locBiomes = GetBiomes(new Vector3(nx, 0f, nz));

                for (int i = 0; i < containedBiomes.Count; i++)
                {
                    if (locBiomes.ContainsKey(containedBiomes[i]))
                    {
                        alphamaps[z, x, i] = locBiomes[containedBiomes[i]];
                    }
                    else
                    {
                        alphamaps[z, x, i] = 0f;
                    }
                }

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
