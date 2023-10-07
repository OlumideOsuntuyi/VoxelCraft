using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Inventory;

[DefaultExecutionOrder(-1)]
public class InventorySlot : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public int index;
    public int id;
    public int amount;
    public bool hotbar;
    private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text count;
    internal Inventory invSrc;

    private int prev_;
    public bool HasItem => amount > 0;
    public Transform iconTransform => icon.transform;
    public Block block => WorldData.instance.GetBlockBaseInfo(id);
    int max => block.stackMax;
    bool holding;
    bool dragging, click;
    Vector3 pos;
    private void Awake()
    {
        if (!hotbar)
        {
            pos = transform.position;
            icon = GetComponentInChildren<Image>();
            count = GetComponentInChildren<TMP_Text>();
            icon.raycastTarget = false;
            count.raycastTarget = false;
            background = gameObject.AddComponent<Image>();
            background.color = Color.clear;
        }
    }
    public void Update()
    {
        amount = Mathf.Clamp(amount, 0, max);
        if (id > 0)
        {
            if (prev_ != id)
            {
                prev_ = id;
                icon.sprite = block.icon;
            }
            count.text = amount.ToString();
        }
        icon.gameObject.SetActive(id > 0);
        count.gameObject.SetActive(id > 0);
    }
    private void OnMouseDown()
    {

    }
    private void OnMouseDrag()
    {

    }
    private void OnMouseUp()
    {

    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!hotbar)
        {
            if (amount > 0)
            {
                click = true;
                background.color = Color.blue * 0.5f;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (click && !hotbar)
        {
            dragging = true;
            background.color = Color.clear;
            transform.position = Input.mousePosition;
            icon.transform.localScale = Vector3.one * 1.2f;
            count.transform.localScale = Vector3.one * 1.2f;
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!hotbar)
        {
            if (!dragging)
            {

            }
            else
            {
                dragging = false;
                click = false;
                background.color = Color.clear;
                OnSlot();
                transform.position = pos;
                icon.transform.localScale = Vector3.one * 1f;
                count.transform.localScale = Vector3.one * 1f;
            }
        }
    }
    void OnSlot()
    {
        float min = 9999;
        int id = 0;
        for (int i = 0; i < invSrc.invlots.Count; i++)
        {
            if (invSrc.invlots[i].index != index)
            {
                float diff = Vector3.Distance(Input.mousePosition, invSrc.invlots[i].transform.position);
                if (diff < min)
                {
                    min = diff;
                    id = i;
                }
            }
        }
        if (id != index)
        {
            invSrc.Swap(index, id);
            Update();
            invSrc.invlots[id].Update();
        }
    }
    public int TakeItem()
    {
        if (!HasItem)
        {
            return 0;
        }
        amount--;
        return id;
    }
}
