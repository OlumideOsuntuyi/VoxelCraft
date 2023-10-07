using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public static class VoxelData
{
    public static readonly int TextureAtlasSizeInBlocks = 32;
    public static float NormalizedBlockTextureSize
    {

        get { return 1f / (float)TextureAtlasSizeInBlocks; }

    }

    public static readonly Vector3[] voxelVerts = new Vector3[8] {

        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f),

    };

    public static readonly Vector3Int[] faceChecks = new Vector3Int[6] {

        new Vector3Int(0, 0, -1), // Back
		new Vector3Int(0, 0, 1), // Front
		new Vector3Int(0, 1, 0), //top
        new Vector3Int(0, -1, 0), //bottom
        new Vector3Int(-1, 0, 0), //left
        new Vector3Int(1, 0, 0) //right

    };

    public static Vector3Int[] GetVertexNeighbour(Vector3Int vertexPos)
    {
        // Define the three neighboring positions based on the faceChecks array.
        Vector3Int sharedSide1 = new Vector3Int(0, 0, 0);
        Vector3Int sharedSide2 = new Vector3Int(0, 0, 0);
        Vector3Int adjacent = new Vector3Int(0, 0, 0);

        for (int i = 0; i < 6; i++)
        {
            Vector3Int faceCheck = faceChecks[i];
            Vector3Int neighborPos = vertexPos + faceCheck;

            // Depending on the face direction, assign the positions to sharedSide1, sharedSide2, and adjacent.
            if (faceCheck.x == 1)
                sharedSide1 = neighborPos;
            else if (faceCheck.x == -1)
                sharedSide2 = neighborPos;
            else if (faceCheck.y == 1)
                sharedSide1 = neighborPos;
            else if (faceCheck.y == -1)
                sharedSide2 = neighborPos;
            else if (faceCheck.z == 1)
                sharedSide1 = neighborPos;
            else if (faceCheck.z == -1)
                sharedSide2 = neighborPos;
        }

        // Calculate the adjacent position based on sharedSide1 and sharedSide2.
        adjacent = sharedSide1 + sharedSide2 - vertexPos;

        // Return the array of neighboring positions.
        return new Vector3Int[] { sharedSide1, sharedSide2, adjacent };
    }
    public static readonly int[] revFaceCheckIndex = new int[6] { 1, 0, 3, 2, 5, 4 };

    public static readonly int[,] voxelTris = new int[6, 4] {

        // Back, Front, Top, Bottom, Left, Right

		// 0 1 2 2 1 3
		{0, 3, 1, 2}, // Back Face
		{5, 6, 4, 7}, // Front Face
		{3, 7, 2, 6}, // Top Face
		{1, 5, 0, 4}, // Bottom Face
		{4, 7, 0, 3}, // Left Face
		{1, 2, 5, 6} // Right Face

	};

    public static readonly Vector2[] voxelUvs = new Vector2[4] {

        new Vector2 (0.0f, 0.0f),
        new Vector2 (0.0f, 1.0f),
        new Vector2 (1.0f, 0.0f),
        new Vector2 (1.0f, 1.0f)

    };


}