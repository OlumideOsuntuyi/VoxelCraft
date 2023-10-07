
using System.Collections.Generic;

using UnityEngine;

[System.Serializable]
public class SubChunk
{
    public byte layer;
    [System.NonSerialized] public ChunkPosition position;
    public Chunk chunk => World.world.chunks[position.position];
    [System.NonSerialized] public GameObject subChunkObject;
    [System.NonSerialized] public MeshFilter meshFilter;
    [System.NonSerialized] public MeshRenderer meshRenderer;

    [System.NonSerialized] public bool layerStateChange;
    [System.NonSerialized] public bool _isEditable;
    public bool isCreated => subChunkObject != null && meshFilter != null && meshRenderer != null;

    [System.NonSerialized] int vertexIndex = 0;
    [System.NonSerialized] List<Vector3> vertices = new List<Vector3>();
    [System.NonSerialized] List<int> triangles = new List<int>();
    [System.NonSerialized] List<int> transparentTriangles = new List<int>();
    [System.NonSerialized] List<int> waterTriangles = new List<int>();
    [System.NonSerialized] List<Vector2> uvs = new List<Vector2>();
    [System.NonSerialized] List<Vector2> uv2 = new List<Vector2>();
    [System.NonSerialized] List<Color> colors = new List<Color>();
    [System.NonSerialized] List<Vector3> normals = new List<Vector3>();

    public BlockState[,,] blocks = new BlockState[16, 16, 16];
    internal BlockState this[int x, int y, int z]
    {
        get
        {
            return blocks[x, y, z];
        }
        set
        {
            lock (blocks)
            {
                blocks[x, y, z] = value;
            }
        }
    }
    public SubChunk()
    {

    }
    public SubChunk(byte layer, ChunkPosition position)
    {
        this.layer = layer;
        this.position = position;
    }
    public void Init(ChunkPosition position)
    {
        this.position = position;
        if (!subChunkObject)
        {
            subChunkObject = new GameObject($"layer {layer}");
            subChunkObject.transform.parent = chunk.chunkObject.transform;
            subChunkObject.transform.localPosition = new();
            meshFilter = subChunkObject.AddComponent<MeshFilter>();
            meshRenderer = subChunkObject.AddComponent<MeshRenderer>();
            meshRenderer.materials = WorldGeneration.instance.materials;
        }
        layerStateChange = true;
    }
    internal void Load()
    {

    }
    internal void Unload()
    {

    }
    void ClearMeshData()
    {
        meshDrawn = false;
        _isEditable = false;
        vertexIndex = 0;
        vertices.Clear();
        normals.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        waterTriangles.Clear();
        uvs.Clear();
        uv2.Clear();
        colors.Clear();
    }
    [System.NonSerialized] internal bool meshDrawn;
    internal void GenerateMeshData()
    {
        if(!_isEditable && !meshDrawn)
        {
            ClearMeshData();
            var size = Settings.instance.chunkSize;
            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.x; z++)
                {
                    for (int y = (16 * (layer + 1)) - 1; y >= 16 * layer; y--)
                    {
                        UpdateMeshData(new(x, y, z));
                    }
                }
            }
            _isEditable = true;
            meshDrawn = false;
            /*
            lock (ChunkLoadingManager.instance.drawSubMeshes)
            {
                ChunkLoadingManager.instance.drawSubMeshes.Enqueue(new ChunkLoadingManager.DrawSubMesh
                {
                    chunk = chunk.position.position,
                    subChunk = layer
                });
            }
            */
        }
    }
    void UpdateMeshData(Vector3 pos)
    {
        bool doShadows = chunk.isWithinShadowDistance;

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        Biome biome = WorldGeneration.instance.biomes[position.BiomeIndex];
        BlockPosition blockPos = new BlockPosition(new(x, y, z), position);
        BlockState voxel = WorldData.Block(blockPos);
        var blockProperty = voxel.block;
        if (blockProperty.isActiveBlock)
        {
            chunk.activeBlocks.Add(voxel.position);
        }
        if (blockProperty.isGravity)
        {
            chunk.gravityBlocks.Add(voxel.position);
        }
        if (voxel.id == 0)
        {
            return;
        }
        var blockWorldPos = blockPos.worldPosition;
        float rot = 0f;
        switch (voxel.orientation)
        {
            case 0:
                rot = 180f;
                break;
            case 5:
                rot = 270f;
                break;
            case 1:
                rot = 0f;
                break;
            default:
                rot = 90f;
                break;
        }

        for (int p = 0; p < blockProperty.faceTextures.Count; p++)
        {
            int translatedP = p;

            if (voxel.orientation != 1 && blockProperty.canRotate)
            {
                if (voxel.orientation == 0)
                {
                    if (p == 0) translatedP = 1;
                    else if (p == 1) translatedP = 0;
                    else if (p == 4) translatedP = 5;
                    else if (p == 5) translatedP = 4;
                }
                else if (voxel.orientation == 5)
                {
                    if (p == 0) translatedP = 5;
                    else if (p == 1) translatedP = 4;
                    else if (p == 4) translatedP = 0;
                    else if (p == 5) translatedP = 1;
                }
                else if (voxel.orientation == 4)
                {
                    if (p == 0) translatedP = 4;
                    else if (p == 1) translatedP = 5;
                    else if (p == 4) translatedP = 1;
                    else if (p == 5) translatedP = 0;
                }
            }

            BlockState neighbour = WorldData.Block(new BlockPosition(blockWorldPos + VoxelData.faceChecks[translatedP]));
            BlockState upBlock = WorldData.Block(new BlockPosition(blockWorldPos + Vector3.one));
            if (neighbour != null && neighbour.block.renderNeighbourFaces && !(blockProperty.isWater && (neighbour.block.isWater || upBlock.block.isWater)))
            {
                float lightLevel = Settings.instance.doLighting || doShadows ? Mathf.Max(voxel.blockLight, voxel.skylight) : Worldtime.instance.globalLightLevel;
                if (lightLevel == 0)
                {
                    //Spawn Enemies
                }
                int faceVertCount = 0;

                for (int i = 0; i < blockProperty.meshData.faces[p].vertData.Length; i++)
                {
                    VertData vertData = blockProperty.meshData.faces[p].GetVertData(i);
                    var vertexPos = pos + (blockProperty.canRotate ? vertData.GetRotatedPosition(new Vector3(0, rot, 0)) : vertData.position);

                    vertices.Add(vertexPos);
                    normals.Add(blockProperty.meshData.faces[p].normal);


                    float ao = GetVertexAO(Vector3Int.FloorToInt(vertexPos)) * blockProperty.occlusionStrength;
                    if(blockProperty.occlusionStrength == 0)
                    {
                        ao = 1;
                    }
                    Color tint = Color.white;
                    switch (blockProperty.faceTextures[p].tintType)
                    {
                        case Block.FaceTexture.TintType.normal:
                            {
                                tint = biome.biomeTint;
                            }break;
                        case Block.FaceTexture.TintType.grass:
                            {
                                tint = biome.grassColor;
                            }break;
                        case Block.FaceTexture.TintType.water:
                            {
                                tint = biome.waterColor;
                            }break;
                        default:break;
                    }
                    colors.Add(new Color(tint.r, tint.g, tint.b, ao));
                    uv2.Add(new Vector2(voxel.skylight * 0.067f, voxel.blockLight * 0.067f));


                    if (blockProperty.isWater)
                        uvs.Add(blockProperty.meshData.faces[p].vertData[i].uv);
                    else
                        AddTexture(blockProperty.GetTextureID(p), vertData.uv);
                    faceVertCount++;
                }

                if (!blockProperty.renderNeighbourFaces)
                {
                    for (int i = 0; i < blockProperty.meshData.faces[p].triangles.Length; i++)
                        triangles.Add(vertexIndex + blockProperty.meshData.faces[p].triangles[i]);
                }
                else
                {
                    if (blockProperty.isWater)
                    {
                        for (int i = 0; i < blockProperty.meshData.faces[p].triangles.Length; i++)
                            waterTriangles.Add(vertexIndex + blockProperty.meshData.faces[p].triangles[i]);
                    }
                    else
                    {
                        for (int i = 0; i < blockProperty.meshData.faces[p].triangles.Length; i++)
                            transparentTriangles.Add(vertexIndex + blockProperty.meshData.faces[p].triangles[i]);
                    }
                }
                vertexIndex += faceVertCount;
            }
        }

    }
    public float GetVertexAO(Vector3Int vertexPos)
    {
        vertexPos += WorldFunctions.ChunkToWorldPosition(position);
        var occB = VoxelData.GetVertexNeighbour(vertexPos);
        int side = 0;
        bool side1, side2, corner;
        side1 = WorldData.IsBlockSolid(occB[0]);
        if (side1)
            side++;
        side2 = WorldData.IsBlockSolid(occB[1]);
        if (side2)
            side++;
        corner = WorldData.IsBlockSolid(occB[2]);
        if (corner)
            side++;
        if (side1 && side2)
        {
            side = 3;
        }
        if (side == 1)
        {
            return 0.8f;
        }
        else if (side == 2)
        {
            return 0.7f;
        }
        else if (side == 3)
        {
            return 0.5f;
        }
        return 1;
    }
    internal void CreateMesh()
    {
        if (isCreated && _isEditable && chunk.isActive)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();

            mesh.subMeshCount = 3;
            mesh.SetTriangles(triangles.ToArray(), 0);
            mesh.SetTriangles(transparentTriangles.ToArray(), 1);
            mesh.SetTriangles(waterTriangles.ToArray(), 2);
            mesh.uv = uvs.ToArray();
            mesh.uv2 = uv2.ToArray();
            mesh.colors = colors.ToArray();
            mesh.normals = normals.ToArray();

            meshFilter.mesh = mesh;
            layerStateChange = false;
            meshDrawn = true;
        }
    }

    void AddTexture(int textureID, Vector2 uv)
    {

        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        x += VoxelData.NormalizedBlockTextureSize * uv.x;
        y += VoxelData.NormalizedBlockTextureSize * uv.y;

        uvs.Add(new Vector2(x, y));
    }
}