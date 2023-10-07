using System;

using UnityEngine;

public static class WorldFunctions
{
    public static ChunkPosition WorldPositionToChunkPosition(Vector3 position)
    {
        return new ChunkPosition(position);
    }
    public static Vector3Int ChunkToWorldPosition(ChunkPosition position)
    {
        return new()
        {
            x = position.x * Settings.instance.chunkSize.x,
            y = 0,
            z = position.z * Settings.instance.chunkSize.z
        };
    }
    public static Vector3Int BlockToWorldPosition(BlockPosition position)
    {
        return position.worldPosition;
    }
    public static BlockPosition WorldToBlockPosition(Vector3 position)
    {
        var p = new BlockPosition(position);
        return p;
    }
    public static Vector3Int WorldToLocalPosition(Vector3 position)
    {
        Vector3Int pos = new Vector3Int
        {
            x = Mathf.Abs((int)position.x % 16),
            y = (int)position.y,
            z = Mathf.Abs((int)position.z % 16)
        };
        if(position.x < 0)
        {
            pos.x = 15 - pos.x;
        }
        if(position.z < 0)
        {
            pos.z = 15 - pos.z;
        }
        return pos;
    }
}
public class IntConverter
{
    public static short ConvertIntToShort(int intValue)
    {
        if (intValue >= short.MinValue && intValue <= short.MaxValue)
        {
            return (short)intValue;
        }
        else
        {
            throw new ArgumentOutOfRangeException("intValue", "Value is out of the short range.");
        }
    }

    public static byte ConvertIntToByte(int intValue)
    {
        if (intValue >= byte.MinValue && intValue <= byte.MaxValue)
        {
            return (byte)intValue;
        }
        else
        {
            throw new ArgumentOutOfRangeException("intValue", "Value is out of the byte range.");
        }
    }
}