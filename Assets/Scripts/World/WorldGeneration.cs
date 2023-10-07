using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;

using static UnityEditor.PlayerSettings;

public class WorldGeneration : MonoBehaviour
{
    public static WorldGeneration instance;
    public List<Biome> biomes = new List<Biome>();
    public Material[] materials = new Material[3];
    internal FastNoise noise = new();
    private void Awake()
    {
        instance = this;
    }
    public void StartGame()
    {
        noise = new(Settings.instance.seed);
        Random.InitState(Settings.instance.seed);
    }
    public byte GetBiome(ChunkPosition pos)
    {
        if(biomes.Count < 2)
        {
            return 0;
        }
        int x = pos.x * Settings.instance.chunkSize.x;
        int y = pos.z * Settings.instance.chunkSize.z;
        return (byte)Mathf.Clamp(Mathf.RoundToInt((biomes.Count - 1) * noise.GetWhiteNoise(x, y)), 0, biomes.Count - 1);
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
                var scO = scP + new Vector3(biome.plantsGroupOffset, biome.plantsGroupOffset, biome.plantsGroupOffset);
                var scPL = scO * biome.plantsScale * 16f;
                if (noise.GetWhiteNoise(scPL.x, scPL.z) > biome.plantsGroupThreshold)
                {
                    var scPT = scO * biome.plantsScale * 16f;
                    var p = pos;
                    p.y += 1;
                    if (noise.GetWhiteNoise(scPT.x, scPT.y, scPT.z) > biome.plantsThreshold)
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
                value += ClampNoise(noise.GetPerlin(x * scale * i, y * scale * i));
            }
            value /= octaves;
        }
        return value;
    }
    float ClampNoise(float noise)
    {
        return (noise + 1) * .5f;
    }
    int _GetBlockType(int x, int y, int z, Biome biome, Vector2Int chunkPos)
    {
        if(y == 0)
        {
            return 1;
        }
        var size = Settings.instance.chunkSize;
        float elevation = biome.elevation;
        float terrainScale = biome.terrainScale;
        int moveX = (x - 8) > 4 ? 1 : (x - 8) < -4 ? -1 : 0;
        int moveY = (x - 8) > 4 ? 1 : (x - 8) < -4 ? -1 : 0;
        int div = 1;
        if (moveX != 0)
        {
            int b = GetBiome(new(chunkPos + (Vector2Int.right * moveX)));
            elevation += biomes[b].elevation;
            terrainScale += biomes[b].terrainScale;
            div++;
        }
        if (moveY != 0)
        {
            int b = GetBiome(new(chunkPos + (Vector2Int.right * moveY)));
            elevation += biomes[b].elevation;
            terrainScale += biomes[b].terrainScale;
            div++;
        }
        elevation /= div;
        terrainScale /= div;
        float simplex1 = noise.GetSimplex(x * .8f * terrainScale, z * .8f * terrainScale) * 10;
        float simplex2 = noise.GetSimplex(x * 3f * terrainScale, z * terrainScale * 3f) * 10 * (noise.GetSimplex(x * terrainScale * .3f, z * terrainScale * .3f) + .5f);

        float heightMap = simplex1 + simplex2;

        //add the 2d noise to the middle of the terrain chunk
        float baseLandHeight = (elevation * size.y) * .5f + heightMap;

        //3d noise for caves and overhangs and such
        float caveNoise1 = noise.GetPerlinFractal(x * 5f * biome.caveScale, y * 10f, z * biome.caveScale * 5f);
        float caveMask = noise.GetSimplex(x * biome.caveScale * .3f, z * biome.caveScale * .3f) + .3f;

        //stone layer heightmap
        float simplexStone1 = noise.GetSimplex(x * 1f, z * 1f) * 10;
        float simplexStone2 = (noise.GetSimplex(x * 5f, z * 5f) + .5f) * 20 * (noise.GetSimplex(x * .3f, z * .3f) + .5f);

        float stoneHeightMap = simplexStone1 + simplexStone2;
        float baseStoneHeight = size.y * elevation * .25f + stoneHeightMap;

        float baseCaveHeight = size.y * elevation * biome.caveSize * .25f + stoneHeightMap;

        int blockType = 0;

        //under the surface, dirt block
        if (y <= baseLandHeight)
        {
            blockType = biome.GetBlock(x, y, z, biome.sediments); //dirt

            //just on the surface, use a grass type
            if (y > baseLandHeight - 1 && y > 0 - 2)
                blockType = biome.terrainBlock; //grass

            if (y <= baseStoneHeight)
                blockType = biome.GetBlock(x, y, z, biome.rocks); //stone
            if(y <= baseCaveHeight)
                blockType = biome.GetBlock(x, y, z, biome.caveBlocks);
        }
        if (caveNoise1 > Mathf.Max(caveMask, .2f))
            blockType = 0; // cave air
        return blockType;
    }

}