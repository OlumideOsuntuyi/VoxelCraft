
using UnityEngine;

[System.Serializable]
public class ChunkPosition
{
    public int x;
    public int z;
    public byte BiomeIndex;
    public Vector2Int position
    {
        get
        {
            return new(x, z);
        }
        set
        {
            x = value.x;
            z = value.y;
        }
    }
    public ChunkPosition()
    {
        position = new Vector2Int();
    }
    public ChunkPosition(Vector2Int vector2Int)
    {
        position = vector2Int;
    }
    public ChunkPosition(int x, int z)
    {
        this.x = x;
        this.z = z;
    }
    public ChunkPosition(Vector3 worldPos)
    {
        x = (int)(worldPos.x / 16);
        if (worldPos.x < 0)
        {
            x = Mathf.CeilToInt(Mathf.Abs(worldPos.x / 16)) * -1;
        }
        z = (int)(worldPos.z / 16);
        if (worldPos.z < 0)
        {
            z = Mathf.CeilToInt(Mathf.Abs(worldPos.z / 16)) * -1;
        }
        /*
        position = new Vector2Int((int)(worldPos.x / Settings.instance.chunkSize.x), (int)(worldPos.z / Settings.instance.chunkSize.x));
        if(worldPos.x < 0)
        {
            x -= 1;
        }
        if(worldPos.z < 0)
        {
            z -= 1;
        }
        */
    }
    public bool Equals(ChunkPosition other)
    {

        if (other == null)
            return false;
        else if (other.x == x && other.z == z)
            return true;
        else
            return false;

    }
}