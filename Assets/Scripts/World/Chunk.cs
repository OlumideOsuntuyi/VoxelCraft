using System.Collections.Generic;
using System.IO;

using UnityEngine;

public class Chunk
{
    internal ChunkPosition position;
    internal GameObject chunkObject;
    internal ChunkComponent chunkComponent;
    internal List<SubChunk> subChunks = new();

    internal List<BlockPosition> activeBlocks = new();
    internal List<BlockPosition> gravityBlocks = new();
    public bool seedGenerated => WorldData.IsBlock(WorldFunctions.ChunkToWorldPosition(position));
    [SerializeField] private bool _editable;
    public bool isEditable
    {
        get
        {
            return _isActive && seedGenerated && _editable && chunkObject != null;
        }
    }
    public bool statechange = true;
    private bool _isActive = false;
    public bool chunkDrawn;
    public bool chunkWaitingForDraw;
    public bool isActive => _isActive;
    public bool isWithinRenderDistance
    {
        get
        {
            if (position.position == new Vector2Int())
            {
                return true;
            }
            else
            {
                return Vector2.Distance(new ChunkPosition(Player.position).position, position.position) <= Settings.instance.renderDistance;
            }
        }
    }
    public bool isWithinShadowDistance => Vector2.Distance(new ChunkPosition(Player.position).position, position.position) <= Settings.instance.shadowDistance;
    internal BlockState this[int x, int y, int z]
    {
        get
        {
            if (!_isActive && subChunks.Count < 16 || y < 0 || y > 255 || x > 15 || z > 15 || x < 0 || z < 0)
            { 
                return new BlockState(new(new Vector3(x, y, z), position));
            }
            int layer = y / 16;
            return subChunks[layer][x, y - (layer * 16), z];
        }
        set
        {
            if (!_isActive && subChunks.Count < 16 || y < 0 || y > 255 || x > 15 || z > 15 || x < 0 || z < 0)
            {
                return;
            }
            int layer = y / 16;
            subChunks[layer][x, y - (layer * 16), z] = value;
        }
    }
    internal string SavePath => Path.Combine(World.world.worldChunksFolder, $"r.{position.position}.VCR");
    public Chunk(ChunkPosition position)
    {
        this.position = position;
        position.BiomeIndex = WorldGeneration.instance.GetBiome(position);
        chunkObject = null;
        subChunks = new();
        for (byte i = 0; i < 16; i++)
        {
            subChunks.Add(new SubChunk(i, position));
        }
    }
    public void Init()
    {
        if (!chunkObject)
        {
            chunkObject = new GameObject(position.position.ToString());
            chunkObject.transform.parent = WorldData.instance.transform;
            chunkObject.transform.localPosition = WorldFunctions.ChunkToWorldPosition(position);
            statechange = true;
            /*
            var data = FileHandler.LoadObject<ChunkData>(SavePath);
            if(data.subchunks.Length != subChunks.Count)
            {
                subChunks = data.subchunks.ToList();
            }
            */
            foreach(var sc in subChunks)
            {
                sc.Init(position);
            }
            chunkComponent = chunkObject.AddComponent<ChunkComponent>();
            chunkComponent.Set(position.position);
        }
        _isActive = false;
        Load();
    }
    public void Load()
    {
        if (!_isActive && chunkObject && isWithinRenderDistance)
        {
            _isActive = true;
            chunkObject.SetActive(true);
            for (int i = 0; i < subChunks.Count; i++)
            {
                subChunks[i].Load();
            }
        }
    }
    public void Unload()
    {
        if (_isActive && chunkObject)
        {
            _isActive = false;
            FileHandler.SaveObject(new ChunkData(subChunks, position), SavePath);
            ClearMeshData();
            for (int i = 0; i < subChunks.Count; i++)
            {
                subChunks[i].Unload();
            }
            chunkObject.SetActive(false);
            chunkComponent.DestoySelf();
        }
    }
    public void EditVoxel(BlockPosition position, int id, int orientation = 1)
    {
        var pre = WorldData.Block(position);
        var post = new BlockState(id, position);


        post.blockLight = pre.blockLight;
        post.skylight = pre.skylight;
        post.orientation = orientation;
        WorldData.SetBlock(post, post.position);
        Debug.Log($"replaced block {pre.block.name} with {post.block.blockName}");
        int layer = (position.y / 16);
        subChunks[layer].layerStateChange = true;
        subChunks[layer]._isEditable = false;
        subChunks[layer].meshDrawn = false;
        subChunks[layer].GenerateMeshData();
        if (chunkComponent != null && !chunkComponent.drawMesh)
        {
            chunkComponent.layerToDraw = layer;
            chunkComponent.drawMesh = true;
        }
    }
    public void Update()
    {
        UpdateGravityBlocks();
        UpdateActiveBlocks();
        UpdateChunkData();
        chunkComponent.updating++;
    }
    void UpdateChunkData()
    {
        GenerateMeshData(true, -1);
    }
    void ClearMeshData()
    {
        _editable = false;
        chunkDrawn = false;
        activeBlocks.Clear();
        gravityBlocks.Clear();
    }
    public void GenerateMeshData(bool queued = true, int layer = -1)
    {
        if (!seedGenerated)
        {
            WorldGeneration.instance.GenerateChunk(position);
            statechange = true;
        }
        if (statechange)
        {
            ClearMeshData();
            if (isWithinShadowDistance)
            {
                CastLight();
            }
            if (subChunks.Count > 0)
            {
                _editable = true;
                if (layer < 0)
                {
                    for (int i = 0; i < subChunks.Count; i++)
                    {
                        subChunks[i].GenerateMeshData();
                    }
                }
                else if (layer > 0 && layer < subChunks.Count)
                {
                    subChunks[layer].GenerateMeshData();
                }
                if (chunkComponent != null && !chunkComponent.drawMesh)
                {
                    chunkComponent.layerToDraw = layer;
                    chunkComponent.drawMesh = true;
                }
                statechange = false;
            }

        }
    }
    void CastLight()
    {
        var size = Settings.instance.chunkSize;
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.z; z++)
            {
                int ray = 15;
                for (int y = size.y - 1; y >= 0; y--)
                {
                    var block = WorldData.Block(new BlockPosition(new(x, y, z), position));
                    if(this[x, y, z] != null)
                    {
                        Block prop = block.block;
                        this[x, y, z].skylight = (byte)ray;
                        this[x, y, z].blockLight = (byte)prop.emission;
                        ray = Mathf.RoundToInt(ray * prop.transparency);
                    }
                }
            }
        }
    }
    void UpdateActiveBlocks()
    {
        List<int> dirs = new() { 0, 1, 4, 5 };
        foreach (var blockPos in activeBlocks)
        {
            var block = WorldData.Block(blockPos);
            var blockProp = block.block;
            if (block.id > 1)
            {
                BlockState[] blocks = new BlockState[6];
                for (int f = 0; f < 6; f++)
                {
                    blocks[f] = WorldData.Block(new BlockPosition(blockPos.worldPosition + VoxelData.faceChecks[f]));
                    var blockFProp = blocks[f].block;
                    if (blockProp.isSolid)
                    {
                        if (blockProp.id > 1)
                        {

                        }
                        else if (blockProp.id == 0)
                        {

                        }
                    }
                    if (dirs.Contains(f))
                    {
                        if (blockProp.isWater && !blockFProp.isSolid && block._state > 2)
                        {
                            blocks[f] = new(block.id, blocks[f].position);
                            blocks[f]._state = (byte)(block._state - 1);
                            WorldData.SetBlock(blocks[f], blocks[f].position);
                        }
                    }
                }
            }
            statechange = true;
        }
    }
    void UpdateGravityBlocks()
    {
        foreach (var blockPos in gravityBlocks)
        {
            var block = WorldData.Block(blockPos);
            var blockProp = block.block;
            if (block.block.isGravity)
            {
                var down = WorldData.Block(new BlockPosition(blockPos.worldPosition + Vector3Int.down));
                var blockFProp = down.block;

                if (!blockFProp.isSolid)
                {
                    WorldData.SetBlock(down, block.position);
                    WorldData.SetBlock(block, down.position);
                }
            }
            statechange = true;
        }
    }
    void SwapBlocks(BlockState one, BlockState two)
    {
        var onePos = one.position;
        var twoPos = two.position;
        WorldData.SetBlock(two, onePos);
        WorldData.SetBlock(one, twoPos);
    }
    [System.Serializable]
    public class ChunkData
    {
        public SubChunk[] subchunks = new SubChunk[16];
        public ChunkPosition position = new();
        public ChunkData()
        {
            subchunks = new SubChunk[16];
            for (byte i = 0; i < subchunks.Length; i++)
            {
                subchunks[i] = new SubChunk(i, position);
            }
        }
        public ChunkData(List<SubChunk> subchunks, ChunkPosition position)
        {
            if (subchunks.Count > 16)
            {
                subchunks.RemoveRange(16, subchunks.Count - 16);
            }
            this.subchunks = subchunks.ToArray();
            this.position = position;
        }
    }
}