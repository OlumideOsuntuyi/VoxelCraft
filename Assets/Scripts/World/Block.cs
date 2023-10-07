
using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu(fileName = "New Block Data", menuName = "Voxelcraft/Block Data")]
public class Block : ScriptableObject
{
    public string blockName;
    public int id;
    public Sprite icon;
    public VoxelMeshData meshData;

    [Range(0, 1)] public float transparency;
    [Range(0, 15)] public int emission;
    public int stackMax = 64;
    public float walkMultiplier = 1;
    public float walkObstructionMultiplier = 1;
    public float gravityMultiplier = 1;
    public float occlusionStrength = 1;
    public float fallDamageNegation = 0;
    public bool renderNeighbourFaces;
    public bool isSolid;
    public bool isWater;
    public bool isPlant;
    public bool isActiveBlock;
    public bool canRotate;
    public bool canSpread;
    public bool canPower;
    public bool isPowered;
    public bool canBreak;
    public bool isGravity;

    public List<FaceTexture> faceTextures = new();
    public AudioClip walkAudio;
    public int GetTextureID(int faceIndex)
    {
        if(faceIndex >= faceTextures.Count)
        {
            return 0;
        }
        return VectorUVToInt(faceTextures[faceIndex].uv);
    }
    public int VectorUVToInt(Vector2 vect)
    {
        return (int)(vect.y * Settings.instance.atlasSize.y) + (int)vect.x;
    }
    [System.Serializable]
    public class FaceTexture
    {
        public Sprite texture;
        public Vector2 uv;
        public TintType tintType;
        public enum TintType { none, normal, grass, water};
    }
}


public class VoxelMod
{

    public Vector3 position;
    public int id;

    public VoxelMod()
    {

        position = new Vector3();
        id = 0;

    }

    public VoxelMod(Vector3 _position, int _id)
    {

        position = _position;
        id = _id;

    }

}
