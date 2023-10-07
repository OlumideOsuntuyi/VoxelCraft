using System.Collections;
using System.Collections.Generic;

using UnityEngine;
public class Structures
{
    public static Queue<VoxelMod> MakeTree(Vector3 startWorldPosition, Biome biome)
    {
        FastNoise noise = WorldGeneration.instance.noise;
        float perl = noise.GetPerlin(startWorldPosition.x * biome.treeScale, startWorldPosition.y * biome.treeScale, startWorldPosition.z * biome.treeScale);
        TreeShape tree = biome.trees[Mathf.RoundToInt((biome.trees.Count - 1) * perl)];
        float perlMask = noise.GetPerlin(startWorldPosition.x * biome.treeScale, startWorldPosition.z * biome.treeScale);
        float perl2 = Mathf.Max(perl, perlMask);
        int height = Mathf.RoundToInt(Mathf.Lerp(tree.minLog, tree.maxLog, perl2));
        Queue<VoxelMod> queue = new();
        Vector3 topLog = new();
        for (int i = 0; i < height; i++)
        {
            VoxelMod log = new VoxelMod()
            {
                id = biome.treeLog,
                position = startWorldPosition + Vector3.up * i
            };
            if(i == height - 1)
            {
                topLog = log.position;
            }
            queue.Enqueue(log);
        }
        foreach(var layer in tree.leaves)
        {
            foreach (var leaves in layer.layer)
            {
                queue.Enqueue(new VoxelMod()
                {
                    id = biome.treeLeaves,
                    position = topLog + leaves
                });

            }
        }

        return queue;
    }
}

[System.Serializable]
public class TreeShape
{
    public List<Layer> leaves;
    public int minLog, maxLog;

    [System.Serializable]
    public class Layer
    {
        public string name;
        public List<Vector3> layer;
    }
}