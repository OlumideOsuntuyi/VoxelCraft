using System;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class PropertyBarSystem : MonoBehaviour
{
    [System.Serializable]
    public class ColorOverlayBar
    {
        public float range { get; private set; }
        public Image bubble;
        public Color minColor, maxColor;
        public float min, max;
        [Range(1, 10)] public float colorMultiplier = 2;
        private float currentValue;
        public TMP_Text label;
        public bool useLabel;
        public void Update(float value)
        {
            this.currentValue = value;
            range = (value - min) / (max - min); range = Mathf.Clamp01(range);
            if (bubble)
            {
                bubble.fillAmount = range;
                bubble.color = Color.Lerp(minColor, maxColor, Mathf.Pow(range, colorMultiplier / 5)) * 1;
            }
            if(useLabel && label)
            {
                label.text = $"{Mathf.Round(range * 100)}%";
            }
        }
    }
    [System.Serializable]
    public class MenuTabs
    {
        [System.Serializable]
        public class Tab
        {
            [SerializeField] public string name { get; private set; }
            public GameObject[] tabBlocks;
            public Color TabColor;
            public List<Vector3> deactivatedPos, activatedPos;
            public Button button;
            [Range(0, 2)] public int tabIndex;
            [HideInInspector] public int prevTabIndex;
            [SerializeField] public List<Image> TabIconImages = new List<Image>();
            public void Enable()
            {
                foreach (Image image in TabIconImages)
                {
                    image.color = TabColor * 0.6f;
                }
                TabIconImages[tabIndex].color = TabColor * 1;
            }
        }
        [Range(0, 2)] public int tabIndex;
        private int prev_tabIndex;
        public List<Tab> tabs = new List<Tab>();
        public void EnableTab(int index)
        {
            DisableTab();
            tabs[tabIndex].tabBlocks[tabs[tabIndex].tabIndex].SetActive(true);
            tabs[tabIndex].TabIconImages[tabs[tabIndex].tabIndex].gameObject.transform.localPosition = tabs[tabIndex].activatedPos[tabs[tabIndex].tabIndex];
            tabs[tabIndex].Enable();
        }
        public void DisableTab()
        {
            tabs[tabIndex].tabBlocks[tabs[tabIndex].prevTabIndex].SetActive(false);
            tabs[tabIndex].TabIconImages[tabs[tabIndex].prevTabIndex].gameObject.transform.localPosition = tabs[tabIndex].deactivatedPos[tabs[tabIndex].prevTabIndex];
            tabs[tabIndex].prevTabIndex = tabs[tabIndex].tabIndex;
        }
        public void Update()
        {
            if (tabs[tabIndex].tabIndex != tabs[tabIndex].prevTabIndex)
            {
                EnableTab(tabs[tabIndex].tabIndex);
            }
        }
    }
    [System.Serializable]
    public class GravityBar
    {
        [System.Serializable]
        public struct DropBar
        {
            public Image image;
            public int index;
        }
        public DropBar[] drops;
        public Color[] colors = new Color[2];
        public float min, max, currentValue;
        public bool useColor, twoColor;
        public float range { get; private set; }
        public float ratio = 0.1f;

        public void Update(float currentValue)
        {
            this.currentValue = currentValue;
            range = (currentValue - min) / (max - min);
            foreach (var bar in drops)
            {
                UpdateEachBar(bar);
            }
        }
        void UpdateEachBar(DropBar bar)
        {
            float indexPosition = bar.index;
            float indexBound = ratio * indexPosition;
            float imageFill = 0;
            if (range <= (ratio * (indexPosition - 1)))
            {
                imageFill = 0f;
            }
            else if (range >= (ratio * indexPosition))
            {
                imageFill = 1;
            }
            else
            {
                imageFill = 1 - Mathf.Clamp01((indexBound - range) / ratio);
            }
            bar.image.fillAmount = imageFill;
            if (useColor)
            {
                bar.image.color = Color.Lerp(colors[(int)Mathf.Max(0, indexPosition - 1)], colors[(int)Mathf.Min(colors.Length - 1, indexPosition)], indexPosition / drops.Length);
            }
        }
    }

    [System.Serializable]
    public class MultipleDropScreen
    {
        public Image[] images;
        public Vector3[] start, finish;
        public float min, max;
        public float currentValue;
        public void Update(float currentValue)
        {
            float range = (currentValue - min) / (max - min);
            for (int i = 0; i < images.Length; i++)
            {
                images[i].gameObject.transform.localPosition = Vector3.Lerp(start[i], finish[i], range);
            }
        }
    }
    [SerializeField] private ColorOverlayBar simpleBar;
    [SerializeField] private MenuTabs tabs;
    [SerializeField] private GravityBar gravityBar;
    [SerializeField] private MultipleDropScreen multipleDropScreen;

    public bool doProgressBar;
    public bool doTab;
    public bool doGravityBar;
    public bool doMultipleDropScreen;
    public float currentValue;
    private float previousValue;
    void Update()
    {
        if(previousValue != currentValue)
        {
            if (doProgressBar)
            {
                simpleBar.Update(currentValue);
            }
            if (doTab)
            {
                tabs.Update();
            }
            if (doGravityBar)
            {
                gravityBar.Update(currentValue);
            }
            if (doMultipleDropScreen)
            {
                multipleDropScreen.Update(currentValue);
            }
            previousValue = currentValue;
        }
    }
    public void SetMinMax(float min, float max)
    {
        if (doProgressBar)
        {
            simpleBar.min = min; simpleBar.max = max;
        }
        else if (doGravityBar)
        {
            gravityBar.min = min; gravityBar.max = max;
        }
        else if (doMultipleDropScreen)
        {
            multipleDropScreen.min = min; multipleDropScreen.max = max;
        }
    }
    public void MoveToTab(int index)
    {
        tabs.tabs[tabs.tabIndex].tabIndex = index;
    }
}
public enum BarType { Health, Nitro, Time }