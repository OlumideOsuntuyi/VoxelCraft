using UnityEngine;

[ExecuteAlways]
public class RotateCubemapBackground : MonoBehaviour
{
    public float rotationSpeed = 10.0f;

    public Material skyboxMaterial;

    void Update()
    {
        skyboxMaterial.SetFloat("_Rotation", Time.time * rotationSpeed);
    }
}
