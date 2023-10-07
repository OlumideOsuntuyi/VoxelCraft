using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AdaptiveScreen : MonoBehaviour
{
    [SerializeField] private RectTransform canvas;
    [SerializeField] private Vector3 editorPosition;
    [SerializeField] private Vector3 offset;
    [SerializeField] private ScreenLocation location = ScreenLocation.mid;
    [SerializeField] private ScreenAlignment alignment = ScreenAlignment.mid;
    [SerializeField] private bool changeOnlyInAwake;
    [SerializeField] private bool run;
    [SerializeField] private bool setEditorPosition;
    public float DeltaWidth => canvas.sizeDelta.x - 1280;
    public float DeltaHeight => canvas.sizeDelta.y - 720;
    public Vector2 DeltaSize => new Vector2(DeltaWidth, DeltaHeight);

    public enum ScreenLocation { left, mid, right }
    public enum ScreenAlignment { top, mid, bottom }

    private void Start()
    {
        if (changeOnlyInAwake && run)
        {
            SetPosition();
        }
    }
    private void Update()
    {
        if (setEditorPosition)
        {
            run = false;
            editorPosition = transform.localPosition;
        }
        if (run && !changeOnlyInAwake)
        {
            SetPosition();
        }
    }
    private void SetPosition()
    {
        switch (location)
        {
            case ScreenLocation.left:
                {
                    transform.localPosition = editorPosition - (Vector3.right * (DeltaWidth / 2)) + (Vector3.right * (Screen.width - Screen.safeArea.size.x) * 0.5f);
                }break;
            case ScreenLocation.right:
                {
                    transform.localPosition = editorPosition + (Vector3.right * (DeltaWidth / 2));
                }break;
            case ScreenLocation.mid:
                {
                    transform.localPosition = editorPosition;
                }
                break;
            default: break;
        }
        switch (alignment)
        {
            case ScreenAlignment.top:
                {
                    transform.localPosition += (Vector3.up * (DeltaHeight / 2));
                }
                break;
            case ScreenAlignment.bottom:
                {
                    transform.localPosition -= (Vector3.up * (DeltaHeight / 2));
                }break;
            case ScreenAlignment.mid:
                {
                    transform.localPosition += new Vector3();
                }
                break;
            default: break;
        }
        transform.localPosition += offset;
    }
}
