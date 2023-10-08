using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
public class Structures
{
    public static Queue<VoxelMod> MakeTree(Vector3 startWorldPosition, Biome biome)
    {
        FastNoise noise = WorldGeneration.instance.noise;
        float perl = WorldGeneration.ClampNoise(noise.GetWhiteNoise(startWorldPosition.x, startWorldPosition.z));
        TreeShape tree = biome.trees[Mathf.RoundToInt((biome.trees.Count - 1) * perl)];
        float perl2 = WorldGeneration.ClampNoise(noise.GetWhiteNoise(startWorldPosition.x * 1.5f, startWorldPosition.z * 1.5f));
        int height = Mathf.RoundToInt(Mathf.Lerp(tree.minLog, tree.maxLog, perl2));
        Queue<VoxelMod> queue = new();
        Vector3 topLog = new();
        for (int i = 0; i < height; i++)
        {
            VoxelMod log = new()
            {
                id = biome.treeLog,
                position = startWorldPosition + Vector3.up * i
            };
            topLog = log.position;
            queue.Enqueue(log);
        }
        foreach(var layer in tree.leaves)
        {
            List<Vector3> leaves = new();
            if(layer.type is TreeShape.Layer.Type.lineX or TreeShape.Layer.Type.lineZ)
            {
                int index = layer.noMiddleLog ? 0 : 1;
                while(index <= layer.size)
                {
                    bool x = layer.type == TreeShape.Layer.Type.lineX;
                    leaves.Add(new Vector3(x ? 1 * index : 0, 0, x ? 0 : 1 * index));
                    index++;
                }
            }else if(layer.type is TreeShape.Layer.Type.crissCross)
            {
                int index = layer.noMiddleLog ? 0 : 1;
                while (index <= layer.size)
                {
                    leaves.Add(new Vector3(index, 0, 0));
                    leaves.Add(new Vector3(0, 0, index));
                    index++;
                }
            }
            else if(layer.type is TreeShape.Layer.Type.full)
            {
                for (int i = -layer.size; i <= layer.size; i++)
                {
                    for (int j = -layer.size; j <= layer.size; i++)
                    {
                        if(!(i == 0 && j == 0 && !layer.noMiddleLog))
                        {
                            leaves.Add(new Vector3(i, 0, j));
                        }
                    }
                }
            }
            foreach(var leaf in leaves)
            {
                queue.Enqueue(new VoxelMod()
                {
                    id = biome.treeLeaves,
                    position = topLog + leaf + (Vector3.down * layer.depth)
                }); ;
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
        public bool noMiddleLog;
        public int depth;
        public int size;
        public Type type;
        public enum Type { lineX, lineZ, crissCross, full}
    }
}