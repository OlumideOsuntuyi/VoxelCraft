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
        int c_x = (int)(position.x / 16);
        if(position.x < 0)
        {
            c_x = Mathf.CeilToInt(Mathf.Abs(position.x / 16)) * -1;
        }
        int c_z = (int)(position.z / 16);
        if (position.z < 0)
        {
            c_z = Mathf.CeilToInt(Mathf.Abs(position.z / 16)) * -1;
        }
        Vector3Int pos = new Vector3Int
        {
            x = (int)position.x - (c_x * 16),
            y = (int)position.y,
            z = (int)position.z - (c_z * 16)
        };
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