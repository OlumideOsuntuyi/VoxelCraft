using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

[ExecuteAlways]
public class ChunkLoadingManager : MonoBehaviour
{
    internal static ChunkLoadingManager instance;
    internal List<ChunkPosition> activeChunks = new();
    internal List<ChunkPosition> activeChunkPass = new();
    internal Queue<ChunkPosition> chunksToInitialize= new();
    internal Queue<Queue<VoxelMod>> chunkModifications = new();


    internal Thread chunkUpdateThread;


    public bool RunChunkUpdates => runChunkUpdates;
    public bool runChunkUpdates = true;
    public bool useThread;
    bool isLighting;
    float gameTime;
    public ChunkLoadingInfo debugInfo = new();


    Vector3[] frustumVerts = new Vector3[8];
    private void OnDisable()
    {
        if (chunkUpdateThread != null)
        {
            chunkUpdateThread.Abort();
        }
    }
    private void Awake()
    {
        instance = this;
    }
    public void StartPlaying()
    {
        runChunkUpdates = true;
        debugInfo = new();
        World.world = new World("Test World");
        WorldGeneration.instance.StartGame();
        StartChunkUpdates();
    }
    private void StartChunkUpdates()
    {
        InitialSort();
        WorldData.instance.Initiate();
        isLighting = false;
        if (useThread)
        {
            chunkUpdateThread = new Thread(new ThreadStart(ChunkUpdates));
            chunkUpdateThread.Start();
        }
        _ = StartCoroutine(MainThreadChunkUpdates());
    }
    public void EndGame()
    {
        chunkUpdateThread.Abort();
        runChunkUpdates = false;
        activeChunkPass.Clear();
        activeChunks.Clear();
        chunksToInitialize.Clear();
        World.world.Unload();
        World.world = null;
        WorldData.instance.EndGame();
    }
    List<Vector2Int> sortedChunkPositions = new List<Vector2Int>();
    void InitialSort()
    {
        var r = (Settings.instance.renderDistance);
        for (int i = -r; i < r; i++)
        {
            for (int j = -r ; j < r; j++)
            {
                Vector2 chunkCenter = GetChunkCenter(new(i, j));
                sortedChunkPositions.Add(new Vector2Int(i, j));
            }
        }
        sortedChunkPositions.Sort((a, b) =>
        {
            Vector2 chunkCenterA = GetChunkCenter(a);
            Vector2 chunkCenterB = GetChunkCenter(b);
            float distanceA = Vector2.Distance(new Vector2(), chunkCenterA);
            float distanceB = Vector2.Distance(new Vector2(), chunkCenterB);
            return distanceA.CompareTo(distanceB);
        });
    }
    void SetBoundingBox()
    {
        int r = Settings.instance.renderDistance * 16;
        float r2 = r * Mathf.Sin(Mathf.Deg2Rad * Settings.instance.fov) * 1f;
        float r3 = r2 * 1f;
        float r4 = r + 8f;

        frustumVerts = new Vector3[8];
        Vector3 playerPos = new(Player.position.x, 0, Player.position.z);
        Quaternion rot = Quaternion.Euler(Player.rotation);
        //near plane
        frustumVerts[0] = playerPos + (Vector3.left * r2) + (Vector3.back * 32) + (Vector3.up * 0);
        frustumVerts[1] = playerPos + (Vector3.right * r2) + (Vector3.back * 32) + (Vector3.up * 0);
        frustumVerts[2] = playerPos + (Vector3.left * r2) + (Vector3.back * 32) + (Vector3.up * 256);
        frustumVerts[3] = playerPos + (Vector3.right * r2) + (Vector3.back * 32) + (Vector3.up * 256);

        //far plane
        frustumVerts[4] = playerPos + (Vector3.left * r3) + (Vector3.forward * r4) + (Vector3.up * 0);
        frustumVerts[5] = playerPos + (Vector3.right * r3) + (Vector3.forward * r4) + (Vector3.up * 0);
        frustumVerts[6] = playerPos + (Vector3.left * r3) + (Vector3.forward * r4) + (Vector3.up * 256);
        frustumVerts[7] = playerPos + (Vector3.right * r3) + (Vector3.forward * r4) + (Vector3.up * 256);

        for (int i = 0; i < frustumVerts.Length; i++)
        {
            frustumVerts[i] = rot * frustumVerts[i];
        }

    }
    bool IsPointInFrustum(Vector3 point)
    {
        return true;
        var p = point;
        point = Quaternion.Euler(Player.rotation) * point;
        if(
            point.x >= frustumVerts[4].x &&
            point.x <= frustumVerts[5].x &&
            point.y >= frustumVerts[0].y &&
            point.y <= frustumVerts[2].y &&
            point.z >= frustumVerts[0].z &&
            point.z <= frustumVerts[7].z

            )
        {
            return true;
        }
        return false;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(frustumVerts[0], frustumVerts[1]);
        Gizmos.DrawLine(frustumVerts[1], frustumVerts[3]);
        Gizmos.DrawLine(frustumVerts[3], frustumVerts[2]);
        Gizmos.DrawLine(frustumVerts[2], frustumVerts[0]);

        Gizmos.DrawLine(frustumVerts[4], frustumVerts[5]);
        Gizmos.DrawLine(frustumVerts[5], frustumVerts[7]);
        Gizmos.DrawLine(frustumVerts[7], frustumVerts[6]);
        Gizmos.DrawLine(frustumVerts[6], frustumVerts[4]);

        Gizmos.DrawLine(frustumVerts[0], frustumVerts[4]);
        Gizmos.DrawLine(frustumVerts[1], frustumVerts[5]);
        Gizmos.DrawLine(frustumVerts[2], frustumVerts[6]);
        Gizmos.DrawLine(frustumVerts[3], frustumVerts[7]);
    }
    private void Update()
    {
        if (Application.isPlaying)
        {
            gameTime = Time.time;
            if (Gameplay.Instance.isPlaying)
            {
                debugInfo.chunkCount = World.world.chunks.Count;
            }
        }
        else
        {
            if (chunkUpdateThread != null)
            {
                chunkUpdateThread.Abort();
            }
        }
    }

    Vector2 GetChunkCenter(Vector2Int chunkPosition)
    {
        // Replace with your logic to calculate the center position of a chunk
        // Example: If each chunk has a size of 1 unit, you can use (chunkPosition.x + 0.5f, chunkPosition.y + 0.5f) as the center.
        return new Vector2(chunkPosition.x, chunkPosition.y );
    }
    public void DrawChunk(ChunkPosition chunk)
    {
        lock (chunksToInitialize)
        {
            if (World.world.chunks.ContainsKey(chunk.position))
            {
                if (!World.world.chunks[chunk.position].chunkWaitingForDraw)
                {
                    chunksToInitialize.Enqueue(chunk);
                    World.world.chunks[chunk.position].chunkWaitingForDraw = true;
                }
            }
        }
    }
    public void CreateStructure(Queue<VoxelMod> queue)
    {
        chunkModifications.Enqueue(queue);
    }
    bool isUpdatingChunks = false, chunkPassFilled, drawingChunks;
    void ChunkUpdates()
    {
        prevChunk = new Vector2Int(int.MaxValue, int.MaxValue);
        while (RunChunkUpdates)
        {
            //send this to its separate sequence later on
            debugInfo.chunkModCount = chunkModifications.Count;
            lock (chunkModifications)
            {
                List<Vector2Int> chunksToMod = new();
                Queue<VoxelMod> queueRepass = new();
                while (chunkModifications.Count > 0)
                {
                    Queue<VoxelMod> queue;
                    queue = chunkModifications.Dequeue();
                    while (queue.Count > 0)
                    {
                        var b = queue.Dequeue();
                        BlockState newMod = new BlockState(b.id, new BlockPosition(b.position));
                        var cPos = newMod.position.chunkPosition.position;
                        if (World.world.chunks.ContainsKey(cPos))
                        {
                            if (!chunksToMod.Contains(cPos))
                            {
                                chunksToMod.Add(cPos);
                            }
                            WorldData.SetBlock(newMod, newMod.position);
                        }
                        else
                        {
                            queueRepass.Enqueue(b);
                        }
                    }
                }
                chunkModifications.Enqueue(queueRepass);
                foreach(var cPos in chunksToMod)
                {
                    if (World.world.chunks.ContainsKey(cPos))
                    {
                        World.world.chunks[cPos].statechange = true;
                        World.world.chunks[cPos].GenerateMeshData();
                    }
                }
            }
            var playerChunkPosition = WorldFunctions.WorldPositionToChunkPosition(Player.position);
            if (chunkPassFilled && !Gameplay.instance.isPlayerPaused)
            {
                isUpdatingChunks = true;
                float prevTime = gameTime;
                debugInfo.chunksUpdatePass = activeChunkPass.Count;
                foreach (var chunk in activeChunkPass)
                {
                    World.world.chunks[chunk.position].Update();
                }
                prevTime = 1000 * (gameTime - prevTime);
                debugInfo.prevChunkUpdateTime = prevTime;
                if (prevChunk != playerChunkPosition.position)
                {
                    activeChunkPass.Clear();
                    chunkPassFilled = false;
                }
                isUpdatingChunks = false;
            }
        }
    }
    Vector2Int prevChunk;
    IEnumerator MainThreadChunkUpdates()
    {
        while (RunChunkUpdates)
        {
            var playerChunkPosition = WorldFunctions.WorldPositionToChunkPosition(Player.position);
            if (!chunkPassFilled && playerChunkPosition.position != prevChunk)
            {
                prevChunk = playerChunkPosition.position;
                foreach (var chunk in activeChunks)
                {
                    ChunkPosition pos = new(chunk.position);
                    Vector3 worldPos = WorldFunctions.ChunkToWorldPosition(pos);
                    Vector3 center = worldPos + (Vector3.up * 128);
                    Vector3 size = new Vector3(16, 256, 16);
                    bool viewed = !CubeInFieldOfView.IsCubeNotInFieldOfView(center, size);
                    if (!World.world.chunks[chunk.position].isWithinRenderDistance)
                    {
                        World.world.chunks[chunk.position].Unload();
                    }
                }
                activeChunks.Clear();
                foreach (Vector2Int chunkPosition in sortedChunkPositions)
                {
                    ChunkPosition pos = new(chunkPosition + playerChunkPosition.position);
                    Vector3 worldPos = WorldFunctions.ChunkToWorldPosition(pos);
                    Vector3 center = worldPos + new Vector3(8, 128, 8);
                    Vector3 size = new(16, 256, 16);
                    bool viewed = !CubeInFieldOfView.IsCubeNotInFieldOfView(center, size) || true;
                    if (viewed)
                    {
                        Debug.Log("in frustum");
                        if (!World.world.chunks.ContainsKey(pos.position))
                        {
                            Chunk c = new(pos);
                            World.world.chunks.Add(pos.position, c);
                            World.world.chunks[pos.position].Init();
                        }
                        if (World.world.chunks.ContainsKey(pos.position))
                        {
                            if (!activeChunkPass.Contains(pos))
                            {
                                activeChunkPass.Add(pos);
                                activeChunks.Add(pos);
                            }
                            World.world.chunks[pos.position].Load();
                        }
                    }
                }
                lock (activeChunkPass)
                {
                    debugInfo.chunksLoaded = activeChunks.Count;
                    debugInfo.chunksUpdatePass = activeChunkPass.Count;
                    chunkPassFilled = activeChunkPass.Count > 0;
                }
            }
            yield return null;
        }
        yield break;
    }
    public bool isDrawingChunk;

    [System.Serializable]
    public struct ChunkLoadingInfo
    {
        public int chunksLoaded;
        public int chunksUpdatePass;
        public int renderDistance;
        public int chunkCount;
        public int chunkModCount;
        public float prevChunkUpdateTime;

        public float prevLightCalculationTime, prevLightVolume;
    }
    public struct DrawSubMesh
    {
        public Vector2Int chunk;
        public int subChunk;
    }
}
public class CameraFrustumCheck
{
    public static Camera mainCamera => Gameplay.instance.cam; // Assign your camera reference in the Inspector.

    // Check if a cube defined by its center and size is within the camera's frustum.
    public static bool IsCubeInFrustum(Vector3 center, Vector3 size)
    {
        // Get the camera's frustum corners.
        Vector3[] frustumCorners = new Vector3[8];
        mainCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), mainCamera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

        // Calculate the half extents of the cube.
        Vector3 halfExtents = size * 0.5f;

        // Calculate the cube's corners.
        Vector3[] cubeCorners = new Vector3[8];
        cubeCorners[0] = center + new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z);
        cubeCorners[1] = center + new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z);
        cubeCorners[2] = center + new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z);
        cubeCorners[3] = center + new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z);
        cubeCorners[4] = center + new Vector3(-halfExtents.x, -halfExtents.y, halfExtents.z);
        cubeCorners[5] = center + new Vector3(halfExtents.x, -halfExtents.y, halfExtents.z);
        cubeCorners[6] = center + new Vector3(-halfExtents.x, halfExtents.y, halfExtents.z);
        cubeCorners[7] = center + new Vector3(halfExtents.x, halfExtents.y, halfExtents.z);

        // Check if all cube corners are within the camera's frustum.
        for (int i = 0; i < 8; i++)
        {
            if (!IsPointInFrustum(frustumCorners, cubeCorners[i]))
            {
                return false; // At least one corner is outside the frustum.
            }
        }

        return true; // All cube corners are within the frustum.
    }

    // Check if a point is within the camera's frustum.
    private static bool IsPointInFrustum(Vector3[] frustumCorners, Vector3 point)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        foreach (var plane in planes)
        {
            if (plane.GetSide(point) == false)
            {
                return false; // The point is outside this frustum plane.
            }
        }
        return true; // The point is inside all frustum planes.
    }
}
public static class CubeInFieldOfView
{
    public static Camera mainCamera => Gameplay.instance.cam;

    // Check if the cube is not in the player's field of view.
    public static bool IsCubeNotInFieldOfView(Vector3 cubeCenter, Vector3 cubeSize)
    {
        // Calculate the vector from the camera to the cube's center.
        Vector3 cameraToCube = cubeCenter - mainCamera.transform.position;

        // Calculate the dot product between camera forward vector and camera-to-cube vector.
        float dotProduct = Vector3.Dot(mainCamera.transform.forward, cameraToCube);

        // Calculate half extents of the cube.
        Vector3 halfExtents = cubeSize * 0.5f;

        // Check if the cube's center is behind the camera and if any part of the cube is in view.
        return dotProduct < 0f && IsCubePartiallyInView(cameraToCube, halfExtents);
    }

    // Check if any part of the cube is in view.
    private static bool IsCubePartiallyInView(Vector3 cameraToCube, Vector3 halfExtents)
    {
        // Calculate the projected size of the cube on the camera's near plane.
        float projectedSize = Vector3.Project(cameraToCube, mainCamera.transform.forward).magnitude;

        // Calculate the projected size of the cube on each axis.
        float halfSizeX = halfExtents.x * projectedSize;
        float halfSizeY = halfExtents.y * projectedSize;
        float halfSizeZ = halfExtents.z * projectedSize;

        // Get the camera's field of view angles.
        float halfFOVX = mainCamera.fieldOfView / 2f;
        float halfFOVY = mainCamera.fieldOfView * mainCamera.aspect / 2f;

        // Check if any part of the cube is within the camera's field of view.
        return Mathf.Abs(cameraToCube.x) < halfSizeX + Mathf.Tan(Mathf.Deg2Rad * halfFOVX) * cameraToCube.z &&
               Mathf.Abs(cameraToCube.y) < halfSizeY + Mathf.Tan(Mathf.Deg2Rad * halfFOVY) * cameraToCube.z;
    }
}
