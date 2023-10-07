using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Biome Data", menuName = "Voxelcraft/Biome Data")]
public class Biome : ScriptableObject
{
    public string biomeName;
    public TerrainType type;
    public List<BiomeBlock> sediments, rocks, caveBlocks;
    public float terrainScale;
    public float caveScale;

    [Header("Tree Properties")]
    public float treeScale, treeLumpScale;
    public float treeThreshold;
    public float treeLumpThreshold;
    public List<TreeShape> trees = new();

    [Header("Plant Properties")]
    public float plantsScale, plantsGroupOffset, plantsGroupThreshold, plantsThreshold;
    public List<BiomeBlock> plant;

    [Range(0.5f, 1.5f)] public float elevation;
    [Range(0.0f, 0.5f)] public float caveSize;
    public int waterHeight = 55;
    public int terrainBlock;
    public int treeLog, treeLeaves;

    public Color grassColor, waterColor, biomeTint;
    public int GetBlock(int x, int y, int z, List<BiomeBlock> blocks)
    {
        int id = blocks[Mathf.RoundToInt((blocks.Count - 1) * Mathf.Clamp01(WorldGeneration.instance.noise.GetPerlin(x, y, z)))].id;
        foreach(var block in blocks)
        {
            if(y >= (Settings.instance.chunkSize.y * block.minHeight * elevation) && y <= (Settings.instance.chunkSize.y * block.maxHeight * elevation))
            {
                if (Noise.Get3DPerlin(new Vector3(x, y, z), block.offset, block.scale, block.threshold))
                {
                    id = block.id;
                }
            }
        }
        return id;
    }


    [System.Serializable]
    public class BiomeBlock
    {
        public int id;
        public float scale;
        public float offset;
        public float threshold;
        public float minHeight;
        public float maxHeight;
    }
}
