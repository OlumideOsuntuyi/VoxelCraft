using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public int _currentSlot = 0;
    public List<InventorySlot> hotbarSlots = new();
    public Image currentSlotStatus;
    public Transform inventoryParent;
    internal int selected;
    [HideInInspector] public List<InventorySlot> invlots = new();
    [SerializeField] private List<Slot> slots = new();
    public InventorySlot currentSlot => hotbarSlots[_currentSlot];
    private void Awake()
    {
        slots = new();
        for (int i = 0; i < 36; i++)
        {
            slots.Add(new Slot());
        }
        if (inventoryParent)
        {
            invlots = inventoryParent.GetComponentsInChildren<InventorySlot>().ToList();
            invlots.Sort((a, b) =>
            {
                return a.index.CompareTo(b.index);
            });
        }
    }
    public void StartGame(List<Slot> slots, int current)
    {
        _currentSlot = current;
        if(slots.Count == this.slots.Count)
        {
            this.slots = slots;
        }
    }
    public List<Slot> EndGame()
    {
        return slots;
    }
    private void Update()
    {
        currentSlotStatus.transform.position = currentSlot.iconTransform.position;
        foreach (var slot in slots)
        {
            slot.Update();
        }
        foreach (var slot in hotbarSlots)
        {
            slot.invSrc = this;
            slot.id = slots[slot.index].id;
            slot.amount = slots[slot.index].amount;
        }
        foreach (var slot in invlots)
        {
            slot.id = slots[slot.index].id;
            slot.invSrc = this;
            slot.amount = slots[slot.index].amount;
        }
    }
    public void MoveSlot(int amount)
    {
        int pre = _currentSlot;
        int post = _currentSlot + amount;
        if(post < 0)
        {
            post = 9 + post;
        }
        if(post >= 0 && post < 9)
        {
            _currentSlot = post;
        }
        else
        {
            int mod = (post) % 9;
            _currentSlot = mod;
        }
    }
    public int AddItems(int id, int amount)
    {
        if(amount > 0)
        {
            int i = 0;
            while (i < slots.Count)
            {
                amount = AddItem(i, id, amount);
                if (amount == 0)
                {
                    i = slots.Count;
                }
                i++;
            }
            return amount;
        }
        return amount;
    }
    int AddItem(int slot, int id, int amount)
    {
        if (amount <= 0 || (id == 0 || (slots[slot].id != 0 && id != slots[slot].id)))
        {
            return amount;
        }
        int diff = 64 - slots[slot].amount;
        if (amount > diff)
        {
            slots[slot].amount += diff;
            return amount - diff;
        }
        slots[slot].id = id;
        slots[slot].amount += amount;
        return 0;
    }
    public int TakeItem(int slot)
    {
        if (slots[slot].amount > 0)
        {
            slots[slot].amount -= 1;
            return slots[slot].id;
        }
        return 0;
    }
    public void Swap(int a, int b)
    {
        Slot _a = slots[a];
        Slot _b = slots[b];

        if(_a.id == _b.id)
        {
            int max = _a.max;
            int sum = _a.amount + _b.amount;
            int a_am = sum > max ? sum - max : sum;
            _a.amount = a_am;
            _b.amount = sum - a_am;
        }

        slots[a] = _b;
        slots[b] = _a;

        invlots[a].id = _b.id;
        invlots[a].amount = _b.amount;

        invlots[b].id = _a.id;
        invlots[b].amount = _a.amount;
    }

    [System.Serializable]
    public class Slot
    {
        public int id = 0;
        public int amount = 0;
        public int max => WorldData.instance.GetBlockBaseInfo(id).stackMax;
        public void Update()
        {
            amount = Mathf.Clamp(amount, 0, max);
            if(amount == 0)
            {
                id = 0;
            }
        }
    }
}
