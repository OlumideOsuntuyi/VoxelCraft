using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World
{
    internal static World world;
    internal Dictionary<Vector2Int, Chunk> chunks = new();

    /// <summary>
    /// Get block state using world position
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns> returns block state</returns>
    internal BlockState this[int x, int y, int z]
    {
        get
        {
            ChunkPosition chunk = new(new Vector3(x, y, z));
            Vector3Int local = new Vector3Int(x, y, z) - WorldFunctions.ChunkToWorldPosition(chunk);
            if (chunks.ContainsKey(chunk.position))
            {
                return chunks[chunk.position][local.x, y, local.z];
            }
            return new BlockState(new BlockPosition(new Vector3(x, y, z)));
        }
        set
        {
            int _x = Mathf.Abs(x % 16);
            if (_x == 0 && !value.generated)
            {
                Debug.Log($"missing x 0 at {new Vector3(x, y, z)}");
            }else if(_x == 15 && !value.generated)
            {
                Debug.Log($"missing x 15 at {new Vector3(x, y, z)}");
            }
            ChunkPosition chunk = new(new Vector3(x, y, z));
            if (chunks.ContainsKey(chunk.position))
            {
                Vector3Int local = new Vector3Int(x, y, z) - WorldFunctions.ChunkToWorldPosition(chunk);
                chunks[chunk.position][local.x, y, local.z] = value;
            }
        }
    }
    public World()
    {
        chunks = new Dictionary<Vector2Int, Chunk>();
        world = this;
    }
}
