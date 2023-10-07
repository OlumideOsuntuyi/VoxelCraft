using UnityEngine;

public class Worldtime : MonoBehaviour
{
    public static Worldtime instance;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Gradient skyColor;
    [SerializeField] private float dayLength = 1200;
    [SerializeField] public float currentLength = 600;
    public float globalLightLevel { get; private set; }

    private void Awake()
    {
        instance = this;
    }
    private void Update()
    {
        mainCamera.backgroundColor = skyColor.Evaluate(globalLightLevel);
        WorldGeneration.instance.materials[0].SetFloat("_GlobalLightLevel", globalLightLevel);
        WorldGeneration.instance.materials[1].SetFloat("_GlobalLightLevel", globalLightLevel);
        WorldGeneration.instance.materials[2].SetFloat("_GlobalLightLevel", globalLightLevel);
        SetGlobalIllumination();
    }
    private void SetGlobalIllumination()
    {
        currentLength = Mathf.Clamp(currentLength + Time.deltaTime, 0, dayLength);
        if (currentLength >= dayLength)
        {
            currentLength = 0;
        }
        float len = dayLength * 0.5f;
        if (currentLength > len)
        {
            globalLightLevel = 1 - Mathf.Clamp01((currentLength - len) / len);
        }
        else
        {
            globalLightLevel = Mathf.Clamp01(currentLength / len);
        }
    }

}