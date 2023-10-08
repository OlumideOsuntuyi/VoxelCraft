using System;

using UnityEngine;


[System.Serializable]
public class BlockState
{
    public short _id = 0;
    public int id
    {
        get
        {
            return _id;
        }
        set
        {
            _id = IntConverter.ConvertIntToShort(value);
        }
    }

    public byte _orientation;
    public byte _state = 0;
    public byte _powered = 0;
    public bool generated;
    public int orientation
    {
        get
        {
            return _orientation;
        }
        set
        {
            _orientation = IntConverter.ConvertIntToByte(value);
        }
    }
    public BlockPosition position;
    public Block block => WorldData.instance.GetBlockBaseInfo(id);
    [System.NonSerialized] public byte _power = 0;
    [System.NonSerialized] public byte _skylight = 0;
    [System.NonSerialized] public byte _blockLight = 0;

    public BlockState(BlockPosition pos)
    {
        id = 0;
        position = pos;
        generated = false;
        _skylight = (byte)Mathf.RoundToInt(15 * Worldtime.instance.globalLightLevel);
        _blockLight = 0;
    }
    public BlockState(int _id, BlockPosition position)
    {
        id = _id;
        orientation = 0;
        _skylight = (byte)Mathf.RoundToInt(15 * Worldtime.instance.globalLightLevel);
        _state = 0;
        _powered = 0;
        generated = true;
        this.position = position;
        if (block.isWater)
        {
            _state = 7;
        }
    }
    public byte skylight
    {
        get { return _skylight; }
        set
        {
            if (value != _skylight)
            {
                var block = this.block; 
                byte prevLight = _skylight;
                byte maxNeighbourLight = value;
                for (int p = 0; p < block.meshData.faces.Length; p++)
                {
                    var neiPos = new BlockPosition(position.worldPosition + block.meshData.faces[p].normal);

                    if (WorldData.IsBlock(neiPos) && neiPos.position != position.position)
                    {
                        var nei = WorldData.Block(neiPos);
                        maxNeighbourLight = (byte)Mathf.Max(maxNeighbourLight, nei.skylight - 1);
                        if (prevLight > value)
                        {
                            if (prevLight - 1 == nei.skylight && nei.skylight > 0)
                            {
                                nei.skylight -= 1;
                                WorldData.SetBlock(nei, nei.position);
                            }
                        }
                        else
                        {
                            if (maxNeighbourLight - 1 > nei.skylight && nei.skylight < 14)
                            {
                                nei.skylight += 1;
                            }
                        }
                    }
                }
                _skylight = (byte)Mathf.Clamp(maxNeighbourLight, 0, 15);
            }
        }
    }
    public byte blockLight
    {
        get { return _blockLight; }
        set
        {
            if (value != _blockLight)
            {
                var block = this.block;
                byte prevLight = _blockLight;
                byte maxNeighbourLight = value;
                for (int p = 0; p < block.meshData.faces.Length; p++)
                {
                    var neiPos = new BlockPosition(position.worldPosition + block.meshData.faces[p].normal);

                    if (WorldData.IsBlock(neiPos) && neiPos.position != position.position)
                    {
                        var nei = WorldData.Block(neiPos);
                        maxNeighbourLight = (byte)Mathf.Max(maxNeighbourLight, nei.blockLight - 1);
                        if (prevLight > value)
                        {
                            if (prevLight - 1 == nei.blockLight && nei.blockLight > 0)
                            {
                                nei.blockLight -= 1;
                                WorldData.SetBlock(nei, nei.position);
                            }
                        }
                        else
                        {
                            if (maxNeighbourLight - 1 > nei.blockLight && nei.blockLight < 14)
                            {
                                nei.blockLight += 1;
                            }
                        }
                    }
                }
                _blockLight = (byte)Mathf.Clamp(maxNeighbourLight, 0, 15);
            }
        }
    }
}

[System.Serializable]
public class BlockPosition
{
    public int x;
    public int y;
    public int z;
    public ChunkPosition chunkPosition => new(new Vector3(x, y, z));
    public Vector3Int position
    {
        get
        {
            return new(x, y, z);
        }
        set
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }
    }
    public Vector3Int localPosition
    {
        get
        {
            return WorldFunctions.WorldToLocalPosition(position);
        }
    }
    public Vector3Int worldPosition
    {
        get
        {
            return position;
        }
    }

    public BlockPosition()
    {
        position = new();
    }
    public BlockPosition(Vector3 position)
    {
        this.position = Vector3Int.FloorToInt(position);
    }
    public BlockPosition(Vector3 position, ChunkPosition chunk)
    {
        this.position = Vector3Int.FloorToInt(position) + WorldFunctions.ChunkToWorldPosition(chunk);
    }
}