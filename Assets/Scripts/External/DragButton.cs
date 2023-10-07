using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragButton : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public Camera main;
    public Image currentImage;  // Currently selected image
    private bool isDragging = false;  // Flag to indicate if the image is being dragged
    private bool isOnInputImage = false;  // Flag to indicate if the currently held image is on top of another input image

    public bool after_cooldown;
    public int index;
    public float coolDown = 0.5f;

    public bool useRadius;
    public float radius;

    public Vector3 D_Position => transform.localPosition - new Vector3();
    float time = 0;
    public void Start()
    {
        currentImage = GetComponent<Image>();
    }
    private void FixedUpdate()
    {
        if (after_cooldown)
        {
            time += Time.deltaTime;
            if(time > coolDown)
            {
                time = 0;
                after_cooldown = false;
                isDragging = false;
            }
        }
        if (!isDragging && transform.localPosition != new Vector3())
        {
            transform.localPosition = new();
        }
    }
    private void OnEnable()
    {

    }
    private void OnDisable()
    {

    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!after_cooldown)
        {
            isDragging = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            if(Input.touchCount >= 1)
            {
                transform.position = Input.GetTouch(0).position;
            }
            else if(Input.mousePresent)
            {
                transform.position = Input.mousePosition;
            }
        }
        if (useRadius)
        {
            ClampRadius(radius);
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        ResetImages();
    }
    public void ClampRadius(float radius)
    {
        StaticShortcuts.ClampTransformToRadius(currentImage.transform, new Vector3(), radius);
    }
    public void ResetImages()
    {
        currentImage.transform.localPosition = new Vector3();
        isDragging = false;
        isOnInputImage = false;
        after_cooldown = true;
        time = 0;
    }

    public Vector2 GetCurrentButtonPosition()
    {
        if (isDragging)
        {
            return currentImage.rectTransform.position;
        }
        else
        {
            return Vector2.zero;
        }
    }

    public bool IsOnInputImage()
    {
        return isOnInputImage;
    }
}
