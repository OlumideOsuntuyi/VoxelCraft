using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public static Settings instance;
    public Vector2 atlasSize = new(32, 32);
    public Vector3Int chunkSize = new(16, 256, 16);
    public int renderDistance;
    public int shadowDistance;
    public int seed = 4672138;
    public float unitOfLight = 0.06f;

    public float lightFalloff = 0.08f;
    public float mouseSensitivity = 1f;
    public bool doLighting = true;

    public int fov = 70;
    private void Awake()
    {
        instance = this;
    }

    [System.Serializable]
    public class UI
    {
        public Slider renderDistance, simulationDistance, mouseSensitity;
        public TMP_Text renderText, simulationText, mouseText;
    }
}
