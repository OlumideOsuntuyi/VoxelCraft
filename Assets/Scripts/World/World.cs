using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

public class World
{
    internal static World world;
    internal Dictionary<Vector2Int, Chunk> chunks = new();
    internal WorldSettings settings;
    internal WorldData data;
    internal WorldSettings.Gamerules gamerules => settings.gamerules;
    internal string worldFolder => Path.Combine(Application.dataPath, Path.Combine("saves", worldName));
    internal string worldChunksFolder => Path.Combine(worldFolder, "regions");
    internal string worldDataPath => Path.Combine(worldFolder, $"data/data.dat");
    internal string worldSettingsPath => Path.Combine(worldFolder, $"data/settings.dat");
    private string worldName;

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
            Vector3Int local = new BlockPosition(new Vector3(x, y, z)).localPosition;
            if (chunks.ContainsKey(chunk.position))
            {
                return chunks[chunk.position][local.x, y, local.z];
            }
            return new BlockState(new BlockPosition(new Vector3(x, y, z)));
        }
        set
        {
            int _x = Mathf.Abs(x % 16);
            ChunkPosition chunk = new(new Vector3(x, y, z));
            if (chunks.ContainsKey(chunk.position))
            {
                Vector3Int local = new BlockPosition(new Vector3(x, y, z)).localPosition;
                chunks[chunk.position][local.x, y, local.z] = value;
            }
        }
    }
    public World(string name)
    {
        chunks = new Dictionary<Vector2Int, Chunk>();
        world = this;
        worldName = name;
        LoadWorld();
        data.worldName = name;
    }
    [System.Serializable]
    public class WorldData
    {
        public string worldName;
    }
    [System.Serializable]
    public class WorldSettings
    {
        public Gamerules gamerules;
        [System.Serializable]
        public class Gamerules
        {
            public bool doFallDamage;
            public bool doFireDamage;
            public bool doDaylightCycle;

        }
    }
    public void SaveGame()
    {
        foreach(var kvp in chunks)
        {
            kvp.Value.Unload();
        }
        FileHandler.SaveObject(data, worldDataPath);
        FileHandler.SaveObject(settings, worldSettingsPath);
    }
    public void LoadWorld()
    {
        data = FileHandler.LoadObject<WorldData>(worldDataPath);
        settings = FileHandler.LoadObject<WorldSettings>(worldSettingsPath);
    }
    public void Unload()
    {
        SaveGame();
    }
}
