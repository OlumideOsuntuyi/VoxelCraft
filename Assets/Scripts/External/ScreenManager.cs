using System.Collections;
using System.Collections.Generic;
using TMPro;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class ScreenManager : Singleton<ScreenManager>
{
    public string activeScreenID = null;
    private string prevActiveID = null;
    public DialogBox mainScreenControl, gameplayControl;
    [HideInInspector] public DialogBox activeBox;
    public ScreenBools screenBool;
    private void Update()
    {
        if(prevActiveID != activeScreenID)
        {
            prevActiveID = activeScreenID;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Return();
        }
    }
    public void SetWorldCam(bool activate)
    {
        screenBool.canMoveWorldCamera = activate;
    }
    public void SwitchMainScreen(bool activate)
    {
        SetWorldCam(activate);
    }
    public void SwitchIfBarIsZero(DialogBox box)
    {
        SwitchMainScreen(box.active_box == 0 && mainScreenControl.active_box == 0);
    }
    public void Return()
    {
        if(activeBox != null)
        {
            activeBox.SetActiveBox(0);
            if(activeBox == activeBox.parent)
            {
                activeBox.SetActiveBox(0);
            }
            else
            {
                if (activeBox.changeParentIndex)
                {
                    activeBox.parent.active_box = activeBox.parentIndex;
                }
                activeBox = activeBox.parent;
            }
            //SwitchMainScreen(mainScreenControl.active_box is 0 && overlayControl.active_box is 0);
        }
    }
    [System.Serializable]
    public class ErrorUI
    {
        public TMP_Text errorMessage;
        public GameObject gameObject;
        public int index;
    }
    [System.Serializable]
    public struct ScreenBools
    {
        public bool canMoveWorldCamera;
    }
}
public enum ScreenState { }
