using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonV2 : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public Image graphic;
    public float holdSizeMultiplier = 1.2f;
    public float holdTime = 0.1f;
    private bool isDragging = false;
    private bool isOnInputImage = false;

    private bool after_cooldown;
    private float coolDown = 0.5f;

    private float holdTime_ = 0;

    bool isHeld;
    float time = 0;

    public UnityEvent onHold, onClick, onRelease;
    public void Start()
    {
        graphic = GetComponent<Image>();
    }
    private void Update()
    {
        if (after_cooldown)
        {
            time += Time.deltaTime;
            if (time > coolDown)
            {
                time = 0;
                after_cooldown = false;
                isDragging = false;
            }
        }

        if (isHeld)
        {
            onHold.Invoke();
            holdTime_ += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(holdSizeMultiplier, holdSizeMultiplier, holdSizeMultiplier), holdTime_ / holdTime);
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
        isHeld = true;
        onClick.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        holdTime_ += Time.deltaTime;
        isHeld = true;
        if (isDragging)
        {
            if (Input.touchCount == 1)
            {

            }
            else if (Input.mousePresent)
            {

            }
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (isDragging)
        {
            ResetImages();
        }
        holdTime_ = 0;
        transform.localScale = Vector3.one;
        isHeld = false;
        onRelease.Invoke();
    }
    public void ResetImages()
    {
        isDragging = false;
        isOnInputImage = false;
        after_cooldown = true;
        time = 0;
    }
}
