//#define DEBUG_THREADS

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public string seed;

    public float noiseScale = 250f;
    public TerrainSettings genSettings = new TerrainSettings();

    public bool automaticUpdatesStart = true;
    public bool genOnStart = false;
    public float updateInterval = 5f;
    public int loadedSectionRadius = 5;
    public float maxOffset = 32f;

    public int numDetailOctaves = 3;
    public float detailNoiseScale = 25f;

    public int maxSimultaneousGens = 8;
    public List<GameObject> follow = new List<GameObject>();

    public List<Biome> biomes;
    public float biomeCenterSpacing = 250f;
    public float maxBiomeCenterOffset = 100f;
    public float biomeBlend = 100f;

    public float minTreeDistance = 1f;
    public float maxMinTreeDistance = 20f;
    public float minBorderTreeDistance = 0.5f;
    public int treeTestPoints = 30;
    public float minBiomeTreeStrength = 0.8f;

    private Dictionary<SectionCoord, TerrainSection> terrains =
            new Dictionary<SectionCoord, TerrainSection>();
    private Queue<SectionCoord> toCreate = new Queue<SectionCoord>();
    private Queue<SectionCoord> toRemove = new Queue<SectionCoord>();
    private List<SectionCoord> generating = new List<SectionCoord>();

    private Dictionary<SectionCoord, BiomeCenter> biomeCenters =
            new Dictionary<SectionCoord, BiomeCenter>();

    private bool automaticUpdates;
    private bool updateRunning = false;
    private bool createRunning = false;
    private bool removeRunning = false;
    private bool loadingSection = false;
    private SectionCoord sectionLoading;
    private int numGenThreads = 0;

    private int seedHash;
    private float[,] offsets;
    private float[,] detailOffsets;

    private int totalBiomeFrequency = 0;

    [System.Serializable]
    public class TerrainSettings
    {
        public float length = 64f;
        public float height = 256f;
        public int heightMapRes = 257;
        public int alphaMapRes = 257;
        public int detailMapRes = 65;
        public int detailMapResPerPatch = 16;
    }

    private class TerrainSection
    {
        public SectionCoord coord;
        public Terrain terrain;

        public TerrainSection(SectionCoord coord = new SectionCoord(), Terrain terrain = null)
        {
            this.coord = coord;
            this.terrain = terrain;
        }
    }

    private struct SectionCoord
    {
        public int x, z;

        public SectionCoord(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SectionCoord))
                return false;
            SectionCoord other = (SectionCoord)obj;

            return other.x == x && other.z == z;
        }

        public override int GetHashCode()
        {
            return x ^ ((z << 16) ^ (z >> 16));
        }
    }

    private class BiomeCenter
    {
        public SectionCoord coord;
        public Biome biome;
        public Vector3 center;
        public Dictionary<SectionCoord, Plane> properBounds =
                new Dictionary<SectionCoord, Plane>();
        public List<Plane> weakBounds = new List<Plane>();
        public bool boundsCalculated = false;
    }

    private class BiomeStrength
    {
        public Biome biome;
        public float strength;

        public BiomeStrength(Biome b, float s)
        {
            biome = b;
            strength = s;
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
        if (genOnStart)
            StartGeneration();
    }


    public void StartGeneration()
    {
#if UNITY_SERVER
        return;
#endif

        if (seed == null)
            seed = "";
        seedHash = NotRandom.HashString(seed);

        if (biomes == null || biomes.Count == 0)
        {
            biomes = Resources.LoadAll<Biome>("Biomes").ToList();
        }

        int maxHeightmapOctaves = 0;
        if (biomes != null && biomes.Count > 0)
        {
            for (int i = 0; i < biomes.Count; i++)
            {
                totalBiomeFrequency += biomes[i].relativeFrequency;
                if (biomes[i].heightmapOctaves > maxHeightmapOctaves)
                    maxHeightmapOctaves = biomes[i].heightmapOctaves;
            }
        }

        offsets = new float[maxHeightmapOctaves, 2];
        NotRandom.RNG rng = new NotRandom.RNG(seedHash);

        for (int i = 0; i < maxHeightmapOctaves; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                offsets[i, j] = (rng.Value() * maxOffset * 2) - maxOffset;
            }
        }

        detailOffsets = new float[numDetailOctaves, 2];
        for (int i = 0; i < numDetailOctaves; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                detailOffsets[i, j] = (rng.Value() * maxOffset * 2) - maxOffset;
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
        List<SectionCoord> needed = new List<SectionCoord>();

        for (int i = 0; i < follow.Count; i++)
        {
            if (follow[i] == null)
                continue;
            List<SectionCoord> coords = SectionsInRadius(SectionFor(
                    follow[i].transform.position), loadedSectionRadius);
            for (int j = 0; j < coords.Count; j++)
                needed.Add(coords[j]);
        }

        List<SectionCoord> exists = terrains.Keys.ToList();
        List<SectionCoord> toGen = needed.Except(exists).Except(toCreate).
                Except(toRemove).Except(generating).ToList();
        List<SectionCoord> remove = exists.Except(needed).Except(toRemove).ToList();

        if (toGen.Count > 0)
        {
            for (int i = 0; i < toGen.Count; i++)
                toCreate.Enqueue(toGen[i]);
            StartCreateCR();
        }

        if (remove.Count > 0)
        {
            for (int i = 0; i < remove.Count; i++)
                toRemove.Enqueue(remove[i]);
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
            while (numGenThreads < maxSimultaneousGens && toCreate.Count > 0)
            {
                numGenThreads++;
                SectionCoord coord = toCreate.Dequeue();
                StartCoroutine(GenerateSection_CR(coord));
                generating.Add(coord);
                yield return null;
            }
            yield return new WaitUntil(() => numGenThreads < maxSimultaneousGens);
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

        List<Biome> containedBiomes = null;
        List<DetailPrototypeData> detailPrototypeDatas = null;
        List<int[,]> detailMaps = null;
        List<TreePrototypeData> treePrototypeDatas = null;
        List<TreeInstance> treeInstances = null;

        Thread heightThread = new Thread(() =>
                {
#if DEBUG_THREADS
                        UnityEngine.Profiling.Profiler.BeginThreadProfiling(
                                "Heightmap", "" + coord.x + ":" + coord.z);
#endif
                    heightmap = GenerateHeightmap(coord);
#if DEBUG_THREADS
                        UnityEngine.Profiling.Profiler.EndThreadProfiling();
#endif
                });
        Thread alphaThread = new Thread(() =>
                {
#if DEBUG_THREADS
                        UnityEngine.Profiling.Profiler.BeginThreadProfiling(
                                "Alphamap", "" + coord.x + ":" + coord.z);
#endif
                    alphamaps = GenerateAlphamaps(coord, out containedBiomes);
#if DEBUG_THREADS
                        UnityEngine.Profiling.Profiler.EndThreadProfiling();
#endif
                });
        Thread detailThread = new Thread(() =>
                {
#if DEBUG_THREADS
                        UnityEngine.Profiling.Profiler.BeginThreadProfiling(
                                "Detailmap", "" + coord.x + ":" + coord.z);
#endif
                    detailMaps = GenerateDetailMaps(coord,
                            out detailPrototypeDatas);
#if DEBUG_THREADS
                        UnityEngine.Profiling.Profiler.EndThreadProfiling();
#endif
                });
        Thread treeThread = new Thread(() =>
                {
#if DEBUG_THREADS
                        UnityEngine.Profiling.Profiler.BeginThreadProfiling(
                                "Treemap", "" + coord.x + ":" + coord.z);
#endif
                    treeInstances = GenerateTreeInstances(coord,
                            out treePrototypeDatas);
#if DEBUG_THREADS
                        UnityEngine.Profiling.Profiler.EndThreadProfiling();
#endif
                });
        yield return null;
        treeThread.Start();
        yield return null;
        heightThread.Start();
        yield return null;
        alphaThread.Start();
        yield return null;
        detailThread.Start();
        yield return new WaitUntil(() => (heightmap != null &&
                alphamaps != null && detailMaps != null && 
                treeInstances != null && !loadingSection));

        TerrainLayer[] terrainLayers = new TerrainLayer[containedBiomes.Count];
        loadingSection = true;
        sectionLoading = coord;

        TerrainData data = new TerrainData();
        data.heightmapResolution = genSettings.heightMapRes;
        yield return null;
        data.alphamapResolution = genSettings.alphaMapRes;
        data.SetDetailResolution(genSettings.detailMapRes,
                genSettings.detailMapResPerPatch);
        data.size = new Vector3(genSettings.length, genSettings.height,
                genSettings.length);
        yield return null;

        data.SetHeights(0, 0, heightmap);
        yield return null;

        for (int i = 0; i < containedBiomes.Count; i++)
            terrainLayers[i] = containedBiomes[i].terrainLayer;

        DetailPrototype[] detailPrototypes = null;
        if (detailPrototypeDatas.Count > 0)
        {
            detailPrototypes = new DetailPrototype[detailPrototypeDatas.Count];
        }
        for (int i = 0; i < detailPrototypeDatas.Count; i++)
        {
            DetailPrototype dp = detailPrototypes[i] = new DetailPrototype();
            DetailPrototypeData dpd = detailPrototypeDatas[i];
            dp.bendFactor = dpd.bendFactor;
            dp.dryColor = dpd.dryColor;
            dp.healthyColor = dpd.healthyColor;
            dp.maxHeight = dpd.maxHeight;
            dp.maxWidth = dpd.maxWidth;
            dp.minHeight = dpd.minHeight;
            dp.minWidth = dpd.minWidth;
            dp.noiseSpread = dpd.noiseSpread;
            if (dpd.prototype != null)
            {
                dp.prototype = dpd.prototype;
                dp.usePrototypeMesh = true;
            }
            else
            {
                dp.prototypeTexture = dpd.prototypeTexture;
                dp.usePrototypeMesh = false;
            }
            dp.renderMode = dpd.renderMode;
        }
        yield return null;

        data.terrainLayers = terrainLayers;
        data.SetAlphamaps(0, 0, alphamaps);
        yield return null;

        TreePrototype[] treePrototypes = null;
        if (treePrototypeDatas.Count > 0)
        {
            treePrototypes = new TreePrototype[treePrototypeDatas.Count];
        }
        for (int i = 0; i < treePrototypeDatas.Count; i++)
        {
            TreePrototype tp = treePrototypes[i] = new TreePrototype();
            tp.bendFactor = treePrototypeDatas[i].bendFactor;
            tp.prefab = treePrototypeDatas[i].prefab;
        }
        yield return null;
        if (detailPrototypes != null)
        {
            data.detailPrototypes = detailPrototypes;
            yield return null;
            for (int i = 0; i < detailMaps.Count; i++)
            {
                data.SetDetailLayer(0, 0, i, detailMaps[i]);
            }
            yield return null;
        }
        if (treePrototypes != null)
        {
            data.treePrototypes = treePrototypes;
            yield return null;
            data.SetTreeInstances(treeInstances.ToArray(), true);
        }
        yield return null;
        data.RefreshPrototypes();

        yield return null;

        GameObject obj = Terrain.CreateTerrainGameObject(data);
        sec.terrain = obj.GetComponent<Terrain>();
        
        //sec.terrain.treeBillboardDistance = 150;

        sec.terrain.allowAutoConnect = true;
        obj.transform.position = new Vector3(coord.x * genSettings.length -
                genSettings.length / 2, 0f, coord.z * genSettings.length -
                genSettings.length / 2);
        yield return null;
        sec.terrain.Flush();
        terrains.Add(coord, sec);
        if (numGenThreads > 0)
            numGenThreads--;
        generating.Remove(coord);
        if (coord.Equals(sectionLoading))
            loadingSection = false;
    }

    private IEnumerator Remove_CR()
    {
        while (toRemove.Count > 0)
        {
            for (int i = 0; i < maxSimultaneousGens && toRemove.Count > 0; i++)
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

    public Biome GetBiome(Vector3 loc)
    {
        List<BiomeStrength> biomes = GetBiomes(loc);
        int max = 0;
        for (int i = 0; i < biomes.Count; i++)
        {
            if (biomes[i].strength > biomes[max].strength)
                max = i;
        }

        return biomes[max].biome;
    }

    private BiomeCenter GenerateBiomeCenter(SectionCoord coord)
    {
        BiomeCenter center = new BiomeCenter();

        center.coord = coord;
        NotRandom.RNG rng = new NotRandom.RNG(NotRandom.Hash2Int(seedHash,
                center.coord.GetHashCode()));

        float nx = center.coord.x * biomeCenterSpacing + ((rng.Value() *
                maxBiomeCenterOffset * 2) - maxBiomeCenterOffset);
        float nz = center.coord.z * biomeCenterSpacing + ((rng.Value() *
                maxBiomeCenterOffset * 2) - maxBiomeCenterOffset);

        center.center = new Vector3(nx, 0f, nz);

        int index = (int)(rng.ValueUInt() % totalBiomeFrequency);
        for (int i = 0; i < biomes.Count; i++)
        {
            if (index >= 0 && index < biomes[i].relativeFrequency)
            {
                center.biome = biomes[i];
                break;
            }
            index -= biomes[i].relativeFrequency;
        }
        return center;
    }

    private BiomeCenter SafeGetBiomeCenter(SectionCoord coord)
    {
        BiomeCenter bc, test;

        if (!biomeCenters.TryGetValue(coord, out bc))
        {
            bc = GenerateBiomeCenter(coord);
            lock(biomeCenters)
            {
                if (!biomeCenters.TryGetValue(coord, out test))
                {
                    biomeCenters.Add(coord, bc);
                }
                else
                {
                    bc = test;
                }
            }
        }

        return bc;
    }

    private void CalculateBiomeBounds(SectionCoord coord)
    {
        BiomeCenter target = SafeGetBiomeCenter(coord);
        
        if (target.boundsCalculated)
            return;

        List<BiomeCenter> neighbors = new List<BiomeCenter>();
        Dictionary<SectionCoord, Plane> properBounds =
                new Dictionary<SectionCoord, Plane>();
        List<Plane> weakBounds = new List<Plane>();
        Plane plane;
        Vector3 self = target.center;
        Vector3 other, middle;
        for (int i = 0; i < 9; i++)
        {
            if (i == 4)
                continue;
            neighbors.Add(SafeGetBiomeCenter(new SectionCoord(coord.x +
                    (i % 3) - 1, coord.z + (i / 3) - 1)));
        }

        for (int i = 0; i < neighbors.Count; i++)
        {
            other = neighbors[i].center;
            middle = (self + other) / 2f;
            if (neighbors[i].boundsCalculated)
            {
                plane = neighbors[i].properBounds[coord].flipped;
                properBounds.Add(neighbors[i].coord, plane);
            }
            else
            {
                plane = new Plane((self - other).normalized, middle);
                properBounds.Add(neighbors[i].coord, plane);
            }
            weakBounds.Add(new Plane(plane.normal,
                    middle - plane.normal * (biomeBlend / 2f)));
        }
        lock (target)
        {
            if (!target.boundsCalculated)
            {
                target.properBounds = properBounds;
                target.weakBounds = weakBounds;
                target.boundsCalculated = true;
            }
        }
    }

    private List<BiomeStrength> GetBiomes(Vector3 vLoc)
    {
        List<BiomeStrength> output = new List<BiomeStrength>();

        Vector3 loc = new Vector3(vLoc.x, 0f, vLoc.z);
        Vector3 biomeLoc = loc / biomeCenterSpacing;
        SectionCoord coord = new SectionCoord(Mathf.RoundToInt(biomeLoc.x),
                Mathf.RoundToInt(biomeLoc.z));

        List<SectionCoord> coords = SectionsInRadius(coord, 2);
        BiomeCenter center;

        bool weak;
        float weakDist, dist;

        for (int i = 0; i < coords.Count; i++)
        {
            center = SafeGetBiomeCenter(coords[i]);
            if (!center.boundsCalculated)
                CalculateBiomeBounds(coords[i]);

            weak = true;
            weakDist = Mathf.Infinity;

            for (int k = 0; weak && k < center.properBounds.Count; k++)
            {
                if (weak)
                {
                    weak = center.weakBounds[k].GetSide(loc);
                    dist = center.weakBounds[k].GetDistanceToPoint(loc);
                    if (dist < weakDist)
                        weakDist = dist;
                }
            }
            if (weak && weakDist > biomeBlend)
            {
                output.Clear();
                output.Add(new BiomeStrength(center.biome, 1f));
                return output;
            }
            if (weak)
            {
                int j = 0;
                for (; j < output.Count; j++)
                {
                    if (output[j].biome == center.biome)
                    {
                        output[j].strength += (weakDist / biomeBlend);
                        break;
                    }
                }
                if (j == output.Count)
                {
                    output.Add(new BiomeStrength(center.biome, weakDist / biomeBlend));
                }
            }
        }

        for (int i = 0; i < output.Count; i++)
        {
            output[i].strength =
                    Mathf.Pow(Mathf.Clamp01(output[i].strength), 2f);
        }
        return output;
    }

    private float[,] GenerateHeightmap(SectionCoord coord)
    {
        float[,] heightmap = new float[genSettings.heightMapRes,
                genSettings.heightMapRes];
        Biome b;
        float height, weight, nx, nz, bx, bz, totalHeight, totalWeight;
        List<BiomeStrength> biomes;
        for (int x = 0; x < genSettings.heightMapRes; x++)
            for (int z = 0; z < genSettings.heightMapRes; z++)
            {
                nx = coord.x - 0.5f + (float)x / (genSettings.heightMapRes - 1);
                nz = coord.z - 0.5f + (float)z / (genSettings.heightMapRes - 1);
                bx = nx * genSettings.length;
                bz = nz * genSettings.length;
                nx *= genSettings.length / noiseScale;
                nz *= genSettings.length / noiseScale;
                biomes = GetBiomes(new Vector3(bx, 0f, bz));
                totalHeight = 0f;
                totalWeight = 0f;
                for (int i = 0; i < biomes.Count; i++)
                {
                    b = biomes[i].biome;
                    if (b == null)
                        continue;
                    weight = biomes[i].strength;
                    totalWeight += weight;

                    height = 0f;
                    for (int j = 0; j < b.heightmapOctaves; j++)
                    {
                        height += Mathf.Pow(2f, -j) * Mathf.PerlinNoise(
                                nx * Mathf.Pow(2, j) + offsets[j, 0],
                                nz * Mathf.Pow(2, j) + offsets[j, 1]);
                    }
                    height /= (2f - Mathf.Pow(2, -(b.heightmapOctaves - 1)));
                    height = Mathf.Pow(height, b.heightExponent);
                    height = height * (b.maxHeight - b.minHeight) + b.minHeight;

                    totalHeight += height * weight;
                }

                heightmap[z, x] = totalHeight / totalWeight;
            }

        return heightmap;
    }

    private float[,,] GenerateAlphamaps(SectionCoord coord,
            out List<Biome> containedBiomes)
    {
        List<float[,]> alphamapList = new List<float[,]>();
        containedBiomes = new List<Biome>();
        Biome b;
        List<BiomeStrength> locBiomes;
        float[,] alphamap;

        float nx, nz;

        for (int x = 0; x < genSettings.alphaMapRes; x++)
            for (int z = 0; z < genSettings.alphaMapRes; z++)
            {
                nx = coord.x - 0.5f + (float)x / (genSettings.alphaMapRes - 1);
                nz = coord.z - 0.5f + (float)z / (genSettings.alphaMapRes - 1);
                nx *= genSettings.length;
                nz *= genSettings.length;

                locBiomes = GetBiomes(new Vector3(nx, 0f, nz));
                for (int i = 0; i < locBiomes.Count; i++)
                {
                    b = locBiomes[i].biome;
                    if (b == null)
                        continue;
                    if (!containedBiomes.Contains(b))
                    {
                        containedBiomes.Add(b);
                        alphamapList.Add(new float[genSettings.alphaMapRes,
                                genSettings.alphaMapRes]);
                    }
                    alphamapList[containedBiomes.IndexOf(b)][z, x] =
                            locBiomes[i].strength;
                }

            }
        float[,,] alphamaps = new float[genSettings.alphaMapRes,
                genSettings.alphaMapRes, containedBiomes.Count];

        for (int i = 0; i < containedBiomes.Count; i++)
        {
            alphamap = alphamapList[i];
            for (int x = 0; x < genSettings.alphaMapRes; x++)
                for (int z = 0; z < genSettings.alphaMapRes; z++)
                {
                    alphamaps[x, z, i] = alphamap[x, z];
                }
        }

        return alphamaps;
    }

    private List<int[,]> GenerateDetailMaps(SectionCoord coord,
            out List<DetailPrototypeData> detailPrototypes)
    {
        List<int[,]> detailmaps = new List<int[,]>();
        detailPrototypes = new List<DetailPrototypeData>();

        float nx, nz, bx, bz, density;
        List<BiomeStrength> biomes = null;
        Biome b;
        DetailPrototypeData dpd;

        for (int x = 0; x < genSettings.detailMapRes; x++)
            for (int z = 0; z < genSettings.detailMapRes; z++)
            {
                nx = coord.x - 0.5f + (float)x / (genSettings.detailMapRes - 1);
                nz = coord.z - 0.5f + (float)z / (genSettings.detailMapRes - 1);
                bx = nx * genSettings.length;
                bz = nz * genSettings.length;
                nx *= genSettings.length / detailNoiseScale;
                nz *= genSettings.length / detailNoiseScale;

                biomes = GetBiomes(new Vector3(bx, 0f, bz));

                for (int i = 0; i < biomes.Count; i++)
                {
                    b = biomes[i].biome;
                    if (b == null)
                        continue;
                    for (int j = 0; j < b.detailPrototypes.Count; j++)
                    {
                        dpd = b.detailPrototypes[j];
                        if (!detailPrototypes.Contains(dpd))
                        {
                            detailPrototypes.Add(dpd);
                            detailmaps.Add(new int[genSettings.detailMapRes,
                                    genSettings.detailMapRes]);
                        }

                        density = 0f;
                        for (int k = 0; k < numDetailOctaves; k++)
                        {
                            density += Mathf.Pow(2f, -k) * Mathf.PerlinNoise(
                                    nx * Mathf.Pow(2, k) + detailOffsets[k, 0],
                                    nz * Mathf.Pow(2, k) + detailOffsets[k, 1]);
                        }
                        density /= (2f - Mathf.Pow(2,
                                -(numDetailOctaves - 1)));

                        detailmaps[detailPrototypes.IndexOf(dpd)][z, x] =
                                Mathf.FloorToInt(((density * (dpd.maxDensity -
                                dpd.minDensity)) + dpd.minDensity) *
                                Mathf.Pow(biomes[i].strength, 2f));
                    }
                }
            }

        return detailmaps;
    }

    private List<TreeInstance> GenerateTreeInstances(SectionCoord coord,
            out List<TreePrototypeData> treePrototypes)
    {
        treePrototypes = new List<TreePrototypeData>();
        List<TreeInstance> treeInstances = new List<TreeInstance>();

        float cellSize = minTreeDistance / Mathf.Sqrt(2);
        int numCells = Mathf.CeilToInt(genSettings.length / cellSize);

        int unprocessed = -1;
        int noTree = -2;

        int[,] trees = new int[numCells, numCells];
        Vector3[,] points = new Vector3[numCells, numCells];

        NotRandom.RNG rng = new NotRandom.RNG(NotRandom.Hash2Int(seedHash, 
                coord.GetHashCode()));

        List<Vector3> toProcess = new List<Vector3>();
        List<Vector3> processed = new List<Vector3>();
        List<Vector3> selected = new List<Vector3>();
        List<TreePrototypeData> selectedTree = new List<TreePrototypeData>();
        Vector3 test, next;
        List<BiomeStrength> biomes;
        Biome b = null;
        int p, ax, az, anx, anz, annx, annz, range, tree, totalTreeFreq, maxX, maxZ;
        float bx, bz, a, r, str;
        bool canTree;
        next = new Vector3(rng.Value() * (genSettings.length -
                minBorderTreeDistance * 2) + minBorderTreeDistance, 0f,
                rng.Value() * (genSettings.length -
                minBorderTreeDistance * 2) + minBorderTreeDistance);
        anx = Mathf.FloorToInt(next.x / cellSize);
        anz = Mathf.FloorToInt(next.z / cellSize);
        trees[anx, anz] = unprocessed;
        points[anx, anz] = next;
        toProcess.Add(next);

        while (toProcess.Count > 0)
        {
            p = (int) (rng.ValueUInt() % toProcess.Count);
            test = toProcess[p];
            toProcess.RemoveAt(p);
            ax = Mathf.FloorToInt(test.x / cellSize);
            az = Mathf.FloorToInt(test.z / cellSize);

            canTree = true;
            range = Mathf.CeilToInt(maxMinTreeDistance / cellSize);
            maxX = Mathf.Min(range, numCells - 1 - ax);
            maxZ = Mathf.Min(range, numCells - 1 - az);
            for (int x = Mathf.Max(-range, -ax); canTree && x <= maxX; x++)
                for (int z = Mathf.Max(-range, -az); canTree && z <= maxZ; z++)
                {
                    anx = ax + x;
                    anz = az + z;
                    tree = trees[anx, anz];
                    if (tree > 0 && Vector3.Distance(test, points[anx, anz]) <
                            selectedTree[tree - 1].minDistance)
                    {
                        canTree = false;
                    }
                }

            if (canTree)
            {
                bx = (coord.x - 0.5f) * genSettings.length + test.x;
                bz = (coord.z - 0.5f) * genSettings.length + test.z;
                biomes = GetBiomes(new Vector3(bx, 0f, bz));
                if (biomes.Count == 1)
                {
                    b = biomes[0].biome;
                    str = biomes[0].strength;
                }
                else
                {
                    b = null;
                    str = 0f;
                    for (int i = 0; i < biomes.Count; i++)
                    {
                        if (b == null)
                        {
                            b = biomes[i].biome;
                            str = biomes[i].strength;
                        }
                        else
                        {
                            if (biomes[i].strength > str)
                            {
                                b = biomes[i].biome;
                                str = biomes[i].strength;
                            }
                        }
                    }
                }
                if (str < minBiomeTreeStrength || b == null ||
                        b.treePrototypes == null ||
                        b.treePrototypes.Count == 0)
                {
                    canTree = false;
                }
            }
            if (canTree && b != null)
            {
                totalTreeFreq = 0;
                for (int i = 0; i < b.treePrototypes.Count; i++)
                {
                    totalTreeFreq += b.treePrototypes[i].relativeFrequency;
                }
                int index = (int)(rng.ValueUInt() % totalTreeFreq);

                for (int i = 0; i < b.treePrototypes.Count; i++)
                {
                    if (index >= 0 && index < b.treePrototypes[i].relativeFrequency)
                    {
                        selectedTree.Add(b.treePrototypes[i]);
                        selected.Add(test);
                        trees[ax, az] = selectedTree.Count;
                        break;
                    }
                    index -= b.treePrototypes[i].relativeFrequency;
                }
            }
            for (int i = 0; i < treeTestPoints; i++)
            {
                a = rng.Value();
                r = rng.Value();
                r = r * minTreeDistance + minTreeDistance;

                next = new Vector3(test.x + r * Mathf.Cos(a * 2 * Mathf.PI),
                        0f, test.z + r * Mathf.Sin(a * 2 * Mathf.PI));
                if (next.x < minBorderTreeDistance || 
                        next.z < minBorderTreeDistance ||
                        next.x > genSettings.length - minBorderTreeDistance ||
                        next.z > genSettings.length - minBorderTreeDistance)
                {
                    continue;
                }
                anx = Mathf.FloorToInt(next.x / cellSize);
                anz = Mathf.FloorToInt(next.z / cellSize);
                if (trees[anx, anz] != 0)
                    continue;
                range = Mathf.CeilToInt(minTreeDistance / cellSize);
                canTree = true;
                maxX = Mathf.Min(range, numCells - 1 - anx);
                maxZ = Mathf.Min(range, numCells - 1 - anz);
                for (int x = Mathf.Max(-range, -anx); canTree &&
                        x <= maxX; x++)
                    for (int z = Mathf.Max(-range, -anz); canTree &&
                            z <= maxZ; z++)
                    {
                        annx = anx + x;
                        annz = anz + z;

                        if (trees[annx, annx] != 0 && 
                                Vector3.Distance(next, points[annx, annz]) <
                                minTreeDistance)
                        {
                            canTree = false;
                        }
                    }
                if (canTree)
                {
                    toProcess.Add(next);
                    trees[anx, anz] = unprocessed;
                    points[anx, anz] = next;
                }
            }
            processed.Add(test);
            if (trees[ax, az] < 1)
                trees[ax, az] = noTree;
        }
        TreeInstance ti;
        TreePrototypeData tpd;
        for (int i = 0; i < selected.Count; i++)
        {
            tpd = selectedTree[i];
            if (!treePrototypes.Contains(tpd))
                treePrototypes.Add(tpd);
            ti = new TreeInstance();
            ti.color = tpd.color;
            ti.lightmapColor = tpd.lightmapColor;
            ti.prototypeIndex = treePrototypes.IndexOf(tpd);
            ti.position = selected[i] / genSettings.length;
            ti.heightScale = rng.Value() * (tpd.maxHeightScale - 
                    tpd.minHeightScale) + tpd.minHeightScale;
            ti.widthScale = rng.Value() * (tpd.maxWidthScale -
                    tpd.minWidthScale) + tpd.minWidthScale;
            ti.rotation = rng.Value() * 2 * Mathf.PI;

            treeInstances.Add(ti);
        }

        return treeInstances;
    }

    private List<SectionCoord> SectionsInRadius(SectionCoord coord, int radius)
    {
        int dist = radius * 2 + 1;
        List<SectionCoord> sections = new List<SectionCoord>(dist * dist);

        int x, z, dir;
        x = z = 0;
        dir = dist = 1;

        while (dist <= 2 * radius + 1)
        {
            while (2 * x * dir < dist)
            {
                if (x * x + z * z < radius * radius + 1)
                    sections.Add(new SectionCoord(coord.x + x, coord.z + z));
                x += dir;
            }
            while (2 * z * dir < dist)
            {
                if (x * x + z * z < radius * radius + 1)
                    sections.Add(new SectionCoord(coord.x + x, coord.z + z));
                z += dir;
            }
            dir *= -1;
            dist++;
        }

        return sections;
    }

    private SectionCoord SectionFor(Vector3 vLoc)
    {
        float lx, lz;
        lx = vLoc.x / genSettings.length;
        lz = vLoc.z / genSettings.length;
        return new SectionCoord(Mathf.RoundToInt(lx), Mathf.RoundToInt(lz));
        /*
        return new SectionCoord(Mathf.FloorToInt((vLoc.x +
                genSettings.length / 2) / genSettings.length), 
                Mathf.FloorToInt((vLoc.z + genSettings.length / 2) /
                genSettings.length));
                */
    }

    private bool SectionExists(SectionCoord coord)
    {
        return terrains.ContainsKey(coord) && terrains[coord].terrain != null &&
                !toRemove.Contains(coord);
    }

    public bool TerrainExistsAt(Vector3 vLoc)
    {
        return SectionExists(SectionFor(vLoc));
    }

    public float TerrainHeightAt(Vector3 vLoc)
    {
        SectionCoord coord = SectionFor(vLoc);
        if (!SectionExists(coord))
            return Mathf.NegativeInfinity;
        TerrainSection sec = terrains[coord];
        if (sec == null || sec.terrain == null)
            return Mathf.NegativeInfinity;
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
