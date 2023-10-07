using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

public static class Lighting
{
    public static void RecalculateNaturaLight(ChunkPosition chunkData)
    {
        for (int x = 0; x < Settings.instance.chunkSize.x; x++)
        {
            for (int z = 0; z < Settings.instance.chunkSize.x; z++)
            {
                int lightray = Mathf.RoundToInt(15 * Worldtime.instance.globalLightLevel);
                // Loop from top to bottom of chunk.
                for (int y = Settings.instance.chunkSize.y; y > -1; y--)
                {
                    // Cache current voxel
                    BlockState voxel = WorldData.Block(new BlockPosition(new(x, y, z), chunkData));
                    lightray = Mathf.Clamp(lightray + voxel.block.emission, 0, 15);
                    voxel.skylight = (byte)lightray;

                    WorldData.SetBlock(voxel, voxel.position);
                    lightray = (int)(lightray * voxel.block.transparency); //apply transparency after light passes block
                }
            }
        }
    }

}