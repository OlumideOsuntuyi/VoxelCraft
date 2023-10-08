using System.Collections.Generic;

using UnityEngine;

public class WorldGeneration : MonoBehaviour
{
    public static WorldGeneration instance;
    public static Dictionary<int, Biome> biomes = new();
    public Material[] materials = new Material[3];
    internal FastNoise noise = new();
    [SerializeField, Tooltip("Sorted by height")] private List<BiomeHeightTable> overworldBiomes = new();
    public string searchFolderPath = "Assets";
    public string scriptableObjectName = "Biome";
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        var biomes_ = WorldData.instance.FindScriptableObjectsInFolder<Biome>(searchFolderPath, scriptableObjectName);
        biomes = new Dictionary<int, Biome>();
        foreach (Biome biome in biomes_)
        {
            if (!biomes.ContainsKey(biome.id))
            {
                biomes.Add(biome.id, biome);
            }
        }
    }
    public void StartGame()
    {
        noise = new(Settings.instance.seed);
        Random.InitState(Settings.instance.seed);
    }
    public byte GetBiome(ChunkPosition pos)
    {
        float temperature = Mathf.PerlinNoise(pos.x * 2.5f, pos.z * 2.5f);
        float height = Mathf.PerlinNoise(pos.x + .05f, pos.z + .05f);

        int id = 0;
        for (int i = 0; i < overworldBiomes.Count; i++)
        {
            if(height >= overworldBiomes[i].height)
            {
                id = i;
            }
        }
        return (byte)overworldBiomes[id].GetBiome(temperature).id;
    }
    public void GenerateChunk(ChunkPosition position)
    {
        var size = Settings.instance.chunkSize;
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.x; z++)
                {
                    Vector3Int pos = new(x, y, z);
                    BlockPosition pos_ = new(pos, position);
                    if (!WorldData.IsBlock(pos_))
                    {
                        if (position.BiomeIndex > biomes.Count - 1)
                        {
                            Debug.Log($"{position.BiomeIndex}");
                        }
                        int index = Mathf.Min(Mathf.Max(0, position.BiomeIndex), biomes.Count - 1);
                        Biome biome = biomes[index];
                        var posWorld = new BlockPosition(new(x, y, z), position).worldPosition;
                        int id = GetBlockType(posWorld.x, posWorld.y, posWorld.z, biome, position.position);
                        BlockState state = new(id, pos_);
                        WorldData.SetBlock(state, pos_);
                    }
                }
            }
        }
    }



    int GetBlockType(int x, int y, int z, Biome biome, Vector2Int chunkPos)
    {
        int id = 0;
        float continent = GetOctaves(x, z, biome.type.continentScale, biome.type.continentOctaves);
        float erosion = GetOctaves(x, z, biome.type.erosionScale, biome.type.erosionOctaves);
        float pv = GetOctaves(x, z, biome.type.pvScale, biome.type.pvOctaves);

        int continentHeight = Mathf.RoundToInt(SplineLerp.Lerp(biome.type.continentPoints, Mathf.Lerp(-biome.type.continentAmplitude, biome.type.continentAmplitude, continent)));
        int erosionHeight = Mathf.RoundToInt(SplineLerp.Lerp(biome.type.erosionPoints, Mathf.Lerp(-biome.type.erosionAmplitude, biome.type.erosionAmplitude, erosion)));
        int pvHeight = Mathf.RoundToInt(SplineLerp.Lerp(biome.type.pvPoints, Mathf.Lerp(-biome.type.pvAmplitude, biome.type.pvAmplitude, pv)));

        int baseLandHeight = (continentHeight - erosionHeight + pvHeight);

        float stoneHeight = (ClampNoise(noise.GetSimplex(x * 5f, z * 5f) * .4f + .5f)) * continentHeight;


        if (y <= baseLandHeight)
        {
            id = biome.GetBlock(x, y, z, biome.sediments);
            if(y == baseLandHeight)
            {
                id = biome.terrainBlock;
            }
            if(y < stoneHeight)
            {
                id = biome.GetBlock(x, y, z, biome.rocks);
            }
            if (y <= biome.type.caveHeight)
            {
                float caveNoise = ClampNoise(noise.GetPerlinFractal(x * 2f, y * 3f, z * 2f));
                float caveNoise2 = ClampNoise(noise.GetPerlinFractal(x * 5f, z * 5f));

                float caveThreshold = Mathf.Lerp(biome.type.minCaveThreshold, biome.type.maxCaveThreshold, 1 - Mathf.Clamp01(y / biome.type.caveHeight));
                float caveSize = Mathf.Lerp(biome.type.minCaveSize, biome.type.maxCaveSize, 1 - Mathf.Clamp01(y / biome.type.caveHeight));

                //open cave
                if (caveNoise >= caveThreshold - caveSize && caveNoise <= caveThreshold + caveSize)
                {
                    id = 0;
                }

                //linked caves
                float caveLinkedThreshold = Mathf.Lerp(biome.type.minLinkedCaveThreshold, biome.type.maxLinkedCaveThreshold, 1 - Mathf.Clamp01(y / biome.type.caveHeight));
                float caveLinkedSize = Mathf.Lerp(biome.type.minLinkedCaveSize, biome.type.maxLinkedCaveSize, 1 - Mathf.Clamp01(y / biome.type.caveHeight));

                float linkDiff = Mathf.Abs(caveNoise - caveNoise2);
                float linkThresh = caveLinkedThreshold - caveLinkedSize;
                if (linkDiff >= linkThresh)
                {
                    if(linkDiff == linkThresh)
                    {
                        id = 3;
                    }
                    else
                    {
                        id = 0;
                    }
                }
            }
        }
        //under water surface blocks
        if (y == baseLandHeight && y < biome.waterHeight)
        {
            id = biome.GetBlock(x, y, z, biome.sediments);
        }
        if (y > baseLandHeight)
        {
            if (y <= biome.waterHeight)
            {
                if(y == biome.waterHeight)
                {
                    //spawn fish
                }
                id = 6;
            }
        }
        if(id == biome.terrainBlock)
        {
            GetPlants(x, y, z, id, biome, new(chunkPos), continent, erosion, pv);
        }
        return id;
    }
    void GetPlants(int x, int y, int z, int id, Biome biome, ChunkPosition chunk, float ct, float er, float pv)
    {
        var pos = new BlockPosition(new Vector3(x, y, z), chunk);
        if (id == biome.terrainBlock)
        {
            bool success = false;
            var scP = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
            //Tree pass
            if (biome.treeScale > 0 && !success)
            {
                var scPL = scP * biome.treeLumpScale;
                if (Mathf.Abs(pv - er) <= biome.treeLumpThreshold)
                {
                    var scPT = scP * biome.treeScale;
                    if (Mathf.Abs(noise.GetSimplex(scPT.x, scPT.z)) > biome.treeThreshold)
                    {
                        var treeQueue = Structures.MakeTree(pos.worldPosition, biome);
                        success = true;
                        ChunkLoadingManager.instance.CreateStructure(treeQueue);
                    }
                }
            }
            //grass and flowers pass
            if (biome.plantsScale > 0 && !success)
            {
                if (Mathf.Abs(pv - er) <= biome.plantsGroupThreshold)
                {
                    var p = pos;
                    p.y += 1;
                    if (pv > er)
                    {
                        BlockState plant = new BlockState(biome.GetBlock(x, y, z, biome.plant), p);
                        WorldData.SetBlock(plant, p);
                        success = true;
                    }
                }
            }
        }
    }
    float GetOctaves(float x, float y, float scale, int octaves)
    {
        float value = 0;
        if(octaves > 0)
        {
            for (int i = 1; i < octaves + 1; i++)
            {
                value += ClampNoise(noise.GetPerlinFractal(x * scale * i, y * scale * i));
            }
            value /= octaves;
        }
        return value;
    }
    public static float ClampNoise(float noise)
    {
        return (noise + 1) * .5f;
    }
    [System.Serializable]
    public class BiomeHeightTable
    {
        public string name;
        public float height;
        [Tooltip("Sorted by temperature")] public List<BiomeTemperatureTable> biomes = new();
        public Biome GetBiome(float temperature)
        {
            int id = 0;
            for (int i = 0; i < biomes.Count; i++)
            {
                if (height <= biomes[i].temperature)
                {
                    id = i;
                }
            }
            return biomes[id].biome;
        }
        [System.Serializable]
        public class BiomeTemperatureTable
        {
            public string name;
            public float temperature;
            public Biome biome;
        }
    }
}