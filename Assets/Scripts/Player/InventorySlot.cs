using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;
[DefaultExecutionOrder(-1)]
public class InventorySlot : MonoBehaviour
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
        if (!hotbar)
        {
            if (amount > 0)
            {
                click = true;
                background.color = Color.blue * 0.5f;
            }
            if (invSrc.selected != index)
            {
                invSrc.Swap(index, invSrc.selected);
            }
        }
    }
    private void OnMouseDrag()
    {
        if (click && !hotbar)
        {
            dragging = true;
            transform.position = Input.mousePosition;
            icon.transform.localScale = Vector3.one * 1.2f;
            count.transform.localScale = Vector3.one * 1.2f;
        }
    }
    private void OnMouseUp()
    {
        if (!hotbar)
        {
            if (!click)
            {

            }
            else
            {
                dragging = false;
                click = false;
                background.color = Color.clear;
                transform.position = pos;
                icon.transform.localScale = Vector3.one * 1f;
                count.transform.localScale = Vector3.one * 1f;
                invSrc.selected = index;
            }
        }
    }
    void OnSlot()
    {

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
