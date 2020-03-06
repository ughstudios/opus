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
    public float updateInterval = 5f;
    public int loadedSectionRadius = 5;
    public int numHeightmapOctaves = 8;
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

        offsets = new float[numHeightmapOctaves, 2];
        NotRandom.RNG rng = new NotRandom.RNG(seedHash);

        for (int i = 0; i < numHeightmapOctaves; i++)
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
            foreach (SectionCoord loc in coords)
                needed.Add(loc);
        }

        List<SectionCoord> exists = terrains.Keys.ToList();
        List<SectionCoord> toGen = needed.Except(exists).Except(toCreate).
                Except(toRemove).Except(generating).ToList();
        List<SectionCoord> remove = exists.Except(needed).Except(toRemove).ToList();

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
            while (numGenThreads < maxSimultaneousGens && toCreate.Count > 0)
            {
                numGenThreads++;
                SectionCoord coord = toCreate.Dequeue();
                StartCoroutine(GenerateSection_CR(coord));
                generating.Add(coord);
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
        TerrainData data = new TerrainData();
        data.heightmapResolution = genSettings.heightMapRes;
        data.alphamapResolution = genSettings.alphaMapRes;
        data.SetDetailResolution(genSettings.detailMapRes,
                genSettings.detailMapResPerPatch);
        data.size = new Vector3(genSettings.length, genSettings.height,
                genSettings.length);

        List<Biome> containedBiomes = null;
        List<DetailPrototypeData> detailPrototypeDatas = null;
        List<int[,]> detailMaps = null;
        List<TreePrototypeData> treePrototypeDatas = null;
        List<TreeInstance> treeInstances = null;

        Thread heightThread = new Thread(() =>
                {
                    heightmap = GenerateHeightmap(coord);
                });
        Thread alphaThread = new Thread(() =>
                {
                    alphamaps = GenerateAlphamaps(coord, out containedBiomes);
                });
        Thread detailThread = new Thread(() =>
                {
                    detailMaps = GenerateDetailMaps(coord,
                            out detailPrototypeDatas);
                });
        Thread treeThread = new Thread(() =>
                {
                    treeInstances = GenerateTreeInstances(coord,
                            out treePrototypeDatas);
                });
        heightThread.Start();
        alphaThread.Start();
        detailThread.Start();
        treeThread.Start();
        yield return new WaitUntil(() => (heightmap != null &&
                alphamaps != null && detailMaps != null && 
                treeInstances != null));

        TerrainLayer[] terrainLayers = new TerrainLayer[containedBiomes.Count];

        for (int i = 0; i < containedBiomes.Count; i++)
            terrainLayers[i] = containedBiomes[i].terrainLayer;

        DetailPrototype[] detailPrototypes = null;
        if (detailPrototypeDatas.Count > 0)
            detailPrototypes = new DetailPrototype[detailPrototypeDatas.Count];
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
            dp.prototype = dpd.prototype;
            dp.prototypeTexture = dpd.prototypeTexture;
            dp.renderMode = dpd.renderMode;
        }

        TreePrototype[] treePrototypes = null;
        if (treePrototypeDatas.Count > 0)
                treePrototypes = new TreePrototype[treePrototypeDatas.Count];
        for (int i = 0; i < treePrototypeDatas.Count; i++)
        {
            TreePrototype tp = treePrototypes[i] = new TreePrototype();
            tp.bendFactor = treePrototypeDatas[i].bendFactor;
            tp.prefab = treePrototypeDatas[i].prefab;
        }

        data.SetHeights(0, 0, heightmap);
        data.terrainLayers = terrainLayers;
        data.SetAlphamaps(0, 0, alphamaps);
        if (detailPrototypes != null)
        {
            data.detailPrototypes = detailPrototypes;
            for (int i = 0; i < detailMaps.Count; i++)
            {
                data.SetDetailLayer(0, 0, i, detailMaps[i]);
            }
        }
        if (treePrototypes != null)
        {
            data.treePrototypes = treePrototypes;
            data.SetTreeInstances(treeInstances.ToArray(), true);
        }
        data.RefreshPrototypes();


        GameObject obj = Terrain.CreateTerrainGameObject(data);
        obj.transform.position = new Vector3(coord.x * genSettings.length -
                genSettings.length / 2, 0f, coord.z * genSettings.length -
                genSettings.length / 2);
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
        Dictionary<Biome, float> biomes = GetBiomes(loc);
        Biome max = null;
        foreach (KeyValuePair<Biome, float> kvp in biomes)
        {
            if (max == null)
                max = kvp.Key;
            else
            {
                if (kvp.Value > biomes[max])
                    max = kvp.Key;
            }
        }

        return max;
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

    private Dictionary<Biome, float> GetBiomes(Vector3 vLoc)
    {
        Dictionary<Biome, float> output = new Dictionary<Biome, float>();

        Vector3 loc = new Vector3(vLoc.x, 0f, vLoc.z);
        Vector3 biomeLoc = loc / biomeCenterSpacing;
        SectionCoord coord = new SectionCoord(Mathf.RoundToInt(biomeLoc.x),
                Mathf.RoundToInt(biomeLoc.z));

        SectionCoord[] seedLocs = new SectionCoord[9];
        BiomeCenter[] centers = new BiomeCenter[seedLocs.Length];

        float[] dists = new float[seedLocs.Length];
        int[] order = new int[seedLocs.Length];

        for (int i = 0; i < seedLocs.Length; i++)
        {
            seedLocs[i] = new SectionCoord(coord.x + i % 3 - 1,
                    coord.z + i / 3 - 1);
            if (!biomeCenters.TryGetValue(seedLocs[i], out centers[i]))
            {
                centers[i] = GenerateBiomeCenter(seedLocs[i]);
                BiomeCenter test;
                lock (biomeCenters)
                {
                    if (!biomeCenters.TryGetValue(seedLocs[i], out test))
                        biomeCenters.Add(seedLocs[i], centers[i]);
                    else
                        centers[i] = test;
                }
            }
            dists[i] = Vector3.Distance(loc, centers[i].center);
            order[i] = i;
        }

        for (int i = 1; i < order.Length; i++)
            for (int j = i; j > 0 && dists[order[j - 1]] > dists[order[j]]; j--)
            {
                int k = order[j];
                order[j] = order[j - 1];
                order[j - 1] = k;
            }

        int extra = order.Length - 1;

        for (; extra >= 0; extra--)
        {
            if (dists[order[extra]] < dists[order[0]] + biomeBlend)
                break;
        }

        if (extra == 0)
        {
            output.Add(centers[order[0]].biome, 1f);
            return output;
        }

        for (int i = 0; i <= extra; i++)
        {
            float val = 1f - ((dists[order[i]] - dists[order[0]]) / biomeBlend);

            if (output.ContainsKey(centers[order[i]].biome))
            {
                output[centers[order[i]].biome] += val;
            }
            else
                output.Add(centers[order[i]].biome, val);
        }
        Biome[] biomesOut = output.Keys.ToArray();
        for (int i = 0; i < biomesOut.Length; i++)
        {
            output[biomesOut[i]] =
                    Mathf.Pow(Mathf.Clamp01(output[biomesOut[i]]), 2f);
        }

        return output;
    }

    private float[,] GenerateHeightmap(SectionCoord coord)
    {
        float[,] heightmap = new float[genSettings.heightMapRes,
                genSettings.heightMapRes];
        Biome b;
        float height, nx, nz, bx, bz, totalHeight, totalWeight;
        Dictionary<Biome, float> biomes = null;
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
                foreach (KeyValuePair<Biome, float> kvp in biomes)
                {
                    totalWeight += kvp.Value;
                    b = kvp.Key;

                    height = 0f;
                    for (int i = 0; i < numHeightmapOctaves; i++)
                    {
                        height += Mathf.Pow(2f, -i) * Mathf.PerlinNoise(
                                nx * Mathf.Pow(2, i) + offsets[i, 0],
                                nz * Mathf.Pow(2, i) + offsets[i, 1]);
                    }
                    height = height / (2f - Mathf.Pow(2, -(numHeightmapOctaves - 1)));
                    height = Mathf.Pow(height, b.heightExponent);
                    height = height * (b.maxHeight - b.minHeight) + b.minHeight;

                    totalHeight += height * kvp.Value;
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
        Dictionary<Biome, float> locBiomes = null;
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

                foreach (KeyValuePair<Biome, float> kvp in locBiomes)
                {
                    if (!containedBiomes.Contains(kvp.Key))
                    {
                        containedBiomes.Add(kvp.Key);
                        alphamapList.Add(new float[genSettings.alphaMapRes,
                                genSettings.alphaMapRes]);
                    }
                    alphamapList[containedBiomes.IndexOf(kvp.Key)][z, x] =
                            kvp.Value;
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
        Dictionary<Biome, float> biomes;

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

                foreach (KeyValuePair<Biome, float> kvp in biomes)
                {
                    foreach (DetailPrototypeData dpd in kvp.Key.detailPrototypes)
                    {
                        if (!detailPrototypes.Contains(dpd))
                        {
                            detailPrototypes.Add(dpd);
                            detailmaps.Add(new int[genSettings.detailMapRes,
                                    genSettings.detailMapRes]);
                        }

                        density = 0f;
                        for (int i = 0; i < numDetailOctaves; i++)
                        {
                            density += Mathf.Pow(2f, -i) * Mathf.PerlinNoise(
                                    nx * Mathf.Pow(2, i) + detailOffsets[i, 0],
                                    nz * Mathf.Pow(2, i) + detailOffsets[i, 1]);
                        }
                        density = density / (2f - Mathf.Pow(2,
                                -(numDetailOctaves - 1)));

                        detailmaps[detailPrototypes.IndexOf(dpd)][z, x] =
                                Mathf.FloorToInt(((density * (dpd.maxDensity -
                                dpd.minDensity)) + dpd.minDensity) *
                                Mathf.Pow(kvp.Value, 2f));
                        /**/
                        /*
                       detailmaps[detailPrototypes.IndexOf(dpd)][z, x] =
                              Mathf.FloorToInt(dpd.minDensity * kvp.Value * kvp.Value);
                       /**/
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

        float leastDist = minTreeDistance, mostDist = minTreeDistance;

        NotRandom.RNG rng = new NotRandom.RNG(NotRandom.Hash2Int(seedHash, 
                coord.GetHashCode()));

        List<Vector3> toProcess = new List<Vector3>();
        List<Vector3> processed = new List<Vector3>();
        List<Vector3> selected = new List<Vector3>();
        List<TreePrototypeData> selectedTree = new List<TreePrototypeData>();
        Dictionary<Biome, float> biomes;
        Vector3 test, next;
        Biome b = null;
        int p, totalTreeFreq;
        float bx, bz, a, r;
        bool canTree;
        toProcess.Add(new Vector3(rng.Value() * (genSettings.length -
                minBorderTreeDistance * 2) + minBorderTreeDistance, 0f,
                rng.Value() * (genSettings.length -
                minBorderTreeDistance * 2) + minBorderTreeDistance));

        while (toProcess.Count > 0)
        {
            p = (int) (rng.Value() * toProcess.Count);
            test = toProcess[p];
            toProcess.RemoveAt(p);

            canTree = true;
            for (int i = 0; i < selected.Count; i++)
            {
                if (Vector3.Distance(test, selected[i]) < selectedTree[i].minDistance)
                {
                    canTree = false;
                    break;
                }
            }
            if (canTree)
            {
                bx = (coord.x - 0.5f) * genSettings.length + test.x;
                bz = (coord.z - 0.5f) * genSettings.length + test.z;
                biomes = GetBiomes(new Vector3(bx, 0f, bz));
                if (biomes.Keys.Count == 1)
                {
                    b = biomes.Keys.ToList()[0];
                }
                else
                {
                    b = null;
                    foreach (KeyValuePair<Biome, float> kvp in biomes)
                    {
                        if (b == null)
                            b = kvp.Key;
                        else
                        {
                            if (kvp.Value > biomes[b])
                                b = kvp.Key;
                        }
                    }
                }
                if (biomes[b] < minBiomeTreeStrength ||
                        b.treePrototypes == null ||
                        b.treePrototypes.Count == 0)
                {
                    canTree = false;
                }
            }
            if (canTree && b != null)
            {
                totalTreeFreq = 0;
                foreach (TreePrototypeData tp in b.treePrototypes)
                {
                    totalTreeFreq += tp.relativeFrequency;
                }
                int index = (int)(rng.ValueUInt() % totalTreeFreq);

                for (int i = 0; i < b.treePrototypes.Count; i++)
                {
                    if (index >= 0 && index < b.treePrototypes[i].relativeFrequency)
                    {
                        selectedTree.Add(b.treePrototypes[i]);
                        selected.Add(test);
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
                canTree = true;
                foreach (Vector3 vec in toProcess)
                {
                    if (Vector3.Distance(next, vec) < minTreeDistance)
                    {
                        canTree = false;
                        break;
                    }
                }
                if (!canTree)
                    continue;
                foreach (Vector3 vec in processed)
                {
                    if (Vector3.Distance(next, vec) < minTreeDistance)
                    {
                        canTree = false;
                        break;
                    }
                }
                if (canTree)
                {
                    toProcess.Add(next);
                }
            }
            processed.Add(test);
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
        return new SectionCoord(Mathf.FloorToInt((vLoc.x +
                genSettings.length / 2) / genSettings.length), 
                Mathf.FloorToInt((vLoc.z + genSettings.length / 2) /
                genSettings.length));
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
