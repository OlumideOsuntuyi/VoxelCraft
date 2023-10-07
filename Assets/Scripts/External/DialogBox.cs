using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DefaultExecutionOrder(1), ExecuteAlways]
public class DialogBox : MonoBehaviour
{
    public int active_box = 0;
    [HideInInspector] public int previous_active_box = -1;
    public bool remove_all_boxes = false;
    public List<DialogBoxObject> boxes = new();
    public List<ScreenOverlays> overlays;
    public bool sendBox, changeParentIndex, checkOverlays;
    [InspectorLabel("sendBox")] public DialogBox parent = null;
    [InspectorLabel("sendBox")] public int parentIndex = 0;

    public bool swipe;
    [InspectorLabel("swipe")] public bool horizontal;
    [InspectorLabel("swipe")] public float swipeDistance = 100, swipeCooldown = 1f;
    public RectTransform swipeRect;
    [SerializeField] private float swipeCooldownTime = 0;
    private bool playing = false;
    [SerializeField] private bool runInEdit = false;
    [SerializeField] private bool invokeInEditMode = false;
    private void Start()
    {
        if(Application.isPlaying || runInEdit)
        {
            remove_all_boxes = false;
            CheckBoxState();
            if (checkOverlays)
            {
                CheckOverlays();
            }
        }
    }
    private void LateUpdate()
    {
        if(Application.isPlaying || runInEdit)
        {
            if (active_box != previous_active_box)
            {
                if (checkOverlays)
                {
                    CheckOverlays();
                }
                Activate(active_box);
                previous_active_box = active_box;
                if (sendBox && (Application.isPlaying || invokeInEditMode))
                {
                    ScreenManager.Instance.activeBox = this;
                }
            }
            CheckBoxState();
        }
    }
    public string GetBoxID()
    {
        return boxes[active_box].id;
    }
    public void CheckOverlays()
    {
        if (checkOverlays && overlays != null && overlays.Count > 0)
        {
            foreach(var overlay in overlays)
            {
                for (int i = 0; i < overlay.content.Count; i++)
                {
                    overlay.content[i].SetActive(overlay.exclude ? !overlay.onActive.Contains(active_box) : overlay.onActive.Contains(active_box));
                }
            }
        }
    }
    public void SetActiveBox(int index)
    {
        active_box = index;
    }
    public void Prev()
    {
        Move(-1);
    }
    public void Next()
    {
        Move(1);
    }
    public void Move(int index)
    {
        active_box += index;
        if(active_box == boxes.Count)
        {
            active_box = 0;
        }else if(active_box < 0)
        {
            active_box = boxes.Count - 1;
        }
    }
    private void CheckBoxState()
    {
        for (int i = 0; i < boxes.Count; i++)
        {
            CheckBox(i);
            if (i != active_box && !boxes[i].closed && !boxes[i].is_playing)
            {
                Activate(i);
            }
            else if (i == active_box && boxes[i].closed && !boxes[i].is_playing)
            {
                Activate(i);
            }
        }
    }

    private void CheckBox(int i)
    {
        DialogBoxObject box = boxes[i];
        if (box.mode is DeactivateMode.Deactivate or DeactivateMode.Zoom)
        {
            box.closed = box.rect.localScale == Vector3.zero;
            box.is_playing = box.rect.localScale != Vector3.zero && box.rect.localScale != Vector3.one;
        }
        else if (box.mode == DeactivateMode.Swipe)
        {
            box.closed = box.rect.localPosition == box.finalPosition;
            box.is_playing = box.rect.localPosition != Vector3.zero && box.rect.localPosition != box.finalPosition;
        }
        else if (box.mode == DeactivateMode.Disable)
        {
            box.closed = !box.rect.gameObject.activeInHierarchy;
        }
        boxes[i] = box;
    }

    private void Activate(int index)
    {
        _ = StartCoroutine(AnimateSpecificBox(boxes[index], index, boxes[index].mode, boxes[index].animationTime));
    }
    public void SetZeroIfSame(int index)
    {
        if (index == active_box)
        {
            SetActiveBox(0);
        }
        else
        {
            SetActiveBox(index);
        }
    }
    public void RemoveAll()
    {
        for (int i = 0; i < boxes.Count; i++)
        {
            if (!boxes[i].closed && !boxes[i].is_playing)
            {
                _ = StartCoroutine(AnimateSpecificBox(boxes[i], i, boxes[i].mode, boxes[i].closeTime));
            }
        }
        remove_all_boxes = false;
    }

    private IEnumerator DeactivateButton(Button button, float delay)
    {
        button.interactable = false;
        _ = new WaitForSeconds(delay);
        button.interactable = true;
        yield break;
    }

    private IEnumerator AnimateSpecificBox(DialogBoxObject box, int index, DeactivateMode mode, float time)
    {
        playing = true;
        box.is_completed = false;
        bool activate = active_box == index;
        float animationTime = 0;
        if (activate)
        {
            if (box.sendID && (Application.isPlaying || invokeInEditMode))
            {
                ScreenManager.Instance.activeScreenID = box.id;
                if(box.onStart is not null && (Application.isPlaying || invokeInEditMode))
                {
                    box.onStart.Invoke();
                }
            }
        }
        else
        {
            if (box.sendID)
            {
                if(box.onExit is not null && (Application.isPlaying || invokeInEditMode))
                {
                    box.onExit.Invoke();
                }
            }
        }
        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForEndOfFrame();
        }
        if (mode == DeactivateMode.Disable)
        {
            if(box.animationTime != 0)
            {
                for (int i = 0; i < 1; i++)
                {
                    yield return new WaitForSeconds(box.animationTime);
                }
            }
            box.rect.gameObject.SetActive(activate);
            box.is_completed = true;
        }
        else
        {
            if (activate && mode != DeactivateMode.Swipe)
            {
                box.rect.gameObject.SetActive(activate);
            }
        }
        while (!box.is_completed)
        {
            animationTime += Time.deltaTime;
            float range = Mathf.Clamp01(animationTime / time);
            if (!activate) { range = 1 - range; }
            if (mode == DeactivateMode.Zoom)
            {
                box.rect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, range);
            }
            else if (mode == DeactivateMode.Swipe)
            {
                box.rect.localPosition = Vector3.Lerp(box.finalPosition, Vector3.zero, range);
            }
            else if (mode == DeactivateMode.Deactivate)
            {
                box.rect.localScale = activate ? Vector3.one : Vector3.zero;
            }

            box.is_completed = false;
            //checks
            if (activate)
            {
                if (box.mode is DeactivateMode.Deactivate or DeactivateMode.Zoom)
                {
                    box.is_completed = box.rect.localScale == Vector3.one;
                }
                else if (box.mode == DeactivateMode.Swipe)
                {
                    box.is_completed = box.rect.localPosition == Vector3.zero;
                }
            }
            else
            {
                if (box.mode is DeactivateMode.Deactivate or DeactivateMode.Zoom)
                {
                    box.is_completed = box.rect.localScale == Vector3.zero;
                }
                else if (box.mode == DeactivateMode.Swipe)
                {
                    box.is_completed = box.rect.localPosition == box.finalPosition;
                }
            }
            yield return new WaitForEndOfFrame();
        }
        if (activate)
        {
            if (box.sendID && (Application.isPlaying || invokeInEditMode))
            {
                ScreenManager.Instance.activeScreenID = box.id;
            }
        }
        if (!activate && mode != DeactivateMode.Swipe)
        {
            box.rect.gameObject.SetActive(activate);
        }
        playing = false;
        yield break;
    }


    [System.Serializable]
    public class DialogBoxObject
    {
        public string id;
        public RectTransform rect;
        public float animationTime, closeTime;
        public DeactivateMode mode;
        public Vector3 finalPosition;
        public bool sendID;
        public UnityEvent onStart, onExit;
        public bool closed;
        public bool is_completed = true, is_playing = false;
    }
    [System.Serializable]
    public class ScreenOverlays
    {
        public string name;
        public List<GameObject> content;
        public List<int> onActive;
        public bool exclude;
    }
}
public enum DeactivateMode { Swipe, Zoom, Deactivate, Disable }
