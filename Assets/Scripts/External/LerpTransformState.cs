using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LerpTransformState : MonoBehaviour
{
    public float lerpTime;
    private float _lerpTime;
    private int direction = 1;
    public bool pos, rot, sca;
    public Vector3[] position = new Vector3[2];
    public Vector3[] rotation = new Vector3[2];
    public Vector3[] scale = new Vector3[2];

    private void Update()
    {
        if (_lerpTime <= 0)
        {
            direction = 1;
        }
        else if (_lerpTime >= lerpTime)
        {
            direction = -1;
        }
        _lerpTime += Time.deltaTime * direction;
        float range = _lerpTime / lerpTime;

        if (pos)
        {
            transform.localPosition = Vector3.Lerp(position[0], position[1], range);
        }
        if (rot)
        {
            transform.localEulerAngles = Vector3.Lerp(rotation[0], rotation[1], range);
        }
        if (sca)
        {
            transform.localScale = Vector3.Lerp(scale[0], scale[1], range);
        }
    }
}
