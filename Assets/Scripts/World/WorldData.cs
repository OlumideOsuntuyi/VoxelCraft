using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

[ExecuteInEditMode]
public class WorldData : MonoBehaviour
{
    public static WorldData instance;
    public string searchFolderPath = "Assets";
    public string scriptableObjectName = "Block";

    internal List<Block> blocks = new List<Block>();
    private Dictionary<int, Block> blockDictionary;

    private void Awake()
    {
        instance = this;
        blocks = FindScriptableObjectsInFolder<Block>(searchFolderPath, scriptableObjectName);
        blockDictionary = new Dictionary<int, Block>();
        foreach (Block block in blocks)
        {
            if (!blockDictionary.ContainsKey(block.id))
            {
                blockDictionary.Add(block.id, block);
            }
        }
        blocks.Sort((a, b) =>
        {
            return a.id.CompareTo(b.id);
        });
    }

    List<T> FindScriptableObjectsInFolder<T>(string folderPath, string scriptableObjectName) where T : ScriptableObject
    {
        List<T> scriptableObjects = new List<T>();

        string[] guids = AssetDatabase.FindAssets("t:" + scriptableObjectName, new string[] { folderPath });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            T scriptableObject = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            if (scriptableObject != null)
            {
                scriptableObjects.Add(scriptableObject);
            }
        }

        return scriptableObjects;
    }
    public void Initiate()
    {

    }
    public void EndGame()
    {
        StaticShortcuts.ClearChildren(transform);
    }
    public Block GetBlockBaseInfo(int id)
    {
        if (blockDictionary != null && blockDictionary.ContainsKey(id))
        {
            return blockDictionary[id];
        }
        return blockDictionary[0];
    }
    public static void SetBlock(BlockState data, BlockPosition pos)
    {
        data.position = pos;
        World.world[pos.worldPosition.x, pos.worldPosition.y, pos.worldPosition.z] = data;
    }
    public static BlockState Block(BlockPosition pos)
    {
        if(World.world != null && World.world.chunks.ContainsKey(pos.chunkPosition.position))
        {
            var res = World.world[pos.worldPosition.x, pos.worldPosition.y, pos.worldPosition.z];
            if (res != null)
            {
                return res;
            }
        }
        return new BlockState(pos);
    }
    public static bool IsBlock(BlockPosition pos)
    {
        return Block(pos).generated;
    }
    public static bool IsBlock(Vector3 worldPosition)
    {
        return IsBlock(WorldFunctions.WorldToBlockPosition(worldPosition));
    }
    public static bool IsBlockSolid(Vector3 worldPosition)
    {
        return Block(WorldFunctions.WorldToBlockPosition(worldPosition)).block.isSolid;
    }
}
