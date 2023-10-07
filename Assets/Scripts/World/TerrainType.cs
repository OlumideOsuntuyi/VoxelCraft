using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu(fileName = "New Terrain Data", menuName = "Voxelcraft/Terrain Data")]
public class TerrainType : ScriptableObject
{
    public float continentScale = 1;
    public int continentOctaves = 1;
    public float continentAmplitude = 1;
    public SplineLerp.FloatSplinePoint[] continentPoints;

    public float erosionScale = 1;
    public int erosionOctaves = 1;
    public float erosionAmplitude = 1;
    public SplineLerp.FloatSplinePoint[] erosionPoints;

    public float pvScale = 1;
    public int pvOctaves = 1;
    public float pvAmplitude = 1;
    public SplineLerp.FloatSplinePoint[] pvPoints;

    public float caveHeight = 45f;
    public float minCaveThreshold = 0.0f, maxCaveThreshold = 1.0f;
    public float minCaveSize = 0.02f, maxCaveSize = 0.1f;


    public float minLinkedCaveThreshold = 0.0f, maxLinkedCaveThreshold = 1.0f;
    public float minLinkedCaveSize = 0.02f, maxLinkedCaveSize = 0.05f;
}




public class SplineLerp
{
    [System.Serializable]
    public struct FloatSplinePoint
    {
        public float value;
        public float time;

        public FloatSplinePoint(float value, float time)
        {
            this.value = value;
            this.time = time;
        }
    }

    public static float Lerp(FloatSplinePoint[] splinePoints, float t)
    {
        if (splinePoints == null || splinePoints.Length == 0)
        {
            Debug.LogError("SplinePoints array is null or empty.");
            return 0.0f;
        }

        if (t <= splinePoints[0].time)
        {
            return splinePoints[0].value;
        }
        else if (t >= splinePoints[splinePoints.Length - 1].time)
        {
            return splinePoints[splinePoints.Length - 1].value;
        }

        for (int i = 1; i < splinePoints.Length; i++)
        {
            if (t <= splinePoints[i].time)
            {
                // Interpolate between the two points in the segment
                float t0 = splinePoints[i - 1].time;
                float t1 = splinePoints[i].time;

                float alpha = (t - t0) / (t1 - t0);
                return Mathf.Lerp(splinePoints[i - 1].value, splinePoints[i].value, alpha);
            }
        }

        Debug.LogError("Error finding spline segment.");
        return 0.0f;
    }
}