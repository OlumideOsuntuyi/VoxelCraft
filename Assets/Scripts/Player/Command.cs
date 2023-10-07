using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;

public class Command : Singleton<Command>
{
    [SerializeField] private TMP_Text debugText, debugLeft;
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private GameObject messageScreen;

    public bool screenOpened { get; private set; }
    private float fps;
    float time;
    void Start()
    {
        
    }

    void Update()
    {
        time += Time.deltaTime;
        screenOpened = messageScreen.activeSelf;
        if(time > 1)
        {
            time = 0;
            fps = (int)(1 / Time.deltaTime);
        }
        if (!Gameplay.Instance.isPlayerPaused)
        {
            HandleDebugScreen();
            HandleCommands();
            if (Input.GetKeyDown(KeyCode.T) && !screenOpened)
            {
                messageScreen.SetActive(!messageScreen.activeSelf);
            }else if(screenOpened && Input.GetKeyDown(KeyCode.Escape))
            {
                messageScreen.SetActive(!messageScreen.activeSelf);
            }
        }
    }
    void HandleDebugScreen()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugText.gameObject.SetActive(!debugText.gameObject.activeSelf);
        }
        string fps = $"{this.fps} fps\n";
        string position = $"XYZ {Vector3Int.FloorToInt(Player.position)}\n" +
                            $"In chunk ({WorldFunctions.WorldPositionToChunkPosition(Player.position).position})\t" +
                            $"In block ({WorldFunctions.WorldToBlockPosition(Player.position).localPosition})\n";
        string light = $"Global light {Mathf.RoundToInt(15 * Worldtime.instance.globalLightLevel)}\n";

        var chunkSrc = ChunkLoadingManager.instance.debugInfo;
        string chunkLoading = $"render distance - {chunkSrc.renderDistance}" +
            $"\nloaded:[{chunkSrc.chunksLoaded} - {chunkSrc.prevChunkUpdateTime}ms] of {chunkSrc.chunkCount}\n" +
            $"light volume : {StaticShortcuts.ReduceNumberStringLength(chunkSrc.prevLightVolume)} [{chunkSrc.prevLightCalculationTime}ms]\n";
        debugText.text = fps + position + light + chunkLoading;



        var srcBlk = Player.instance.lastLookAt;
        var srcProp = srcBlk.block;

        var srcDownBlk = Player.instance.physics.feetBlock;
        var srcDownProp = srcDownBlk.block;


        var srcUpBlk = Player.instance.physics.headBlock;
        var srcUpProp = srcUpBlk.block;

        string look_blockGeneratedState = srcBlk.generated ? "<color=green>G</color>" : "<color=red>NG</color>";
        string feet_blockGeneratedState = srcDownBlk.generated ? "<color=green>G</color>" : "<color=red>NG</color>";
        string blockInfo = $"Block: {srcProp.name}: light[{srcBlk.skylight}] \n" +
            $"xyz[{srcBlk.position.position}] [{look_blockGeneratedState}]\n";
        string blockUpInfo = $"Head block: {srcUpProp.name}: light[{srcUpBlk.skylight}] \n";
        string blockDownInfo = $"On: {srcDownProp.name}: light[{srcDownBlk.skylight}] \n" +
            $"xyz[{srcDownBlk.position.position}] [{feet_blockGeneratedState}]\n";
        debugLeft.text = blockInfo + blockUpInfo + blockDownInfo;
    }
    void HandleCommands()
    {
        if (messageScreen.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                bool success = false;
                string text = messageInput.text;
                if (text.Length > 2)
                {
                    if (text[0] == '/')
                    {
                        string[] texts = text.Remove(0).Split(' ');
                        if (texts.Length > 1)
                        {
                            string tx1 = texts[0];
                            string[] args = text.Remove(0, tx1.Length).Split(" ");
                            switch (tx1)
                            {
                                case "time":
                                    {
                                        if(args.Length == 1)
                                        {
                                            if (int.TryParse(args[0], out int arg))
                                            {
                                                if(arg > 0)
                                                {
                                                    if(arg > 1200)
                                                    {
                                                        arg %= 1200;
                                                    }
                                                    SetTime(arg);
                                                    success = true;
                                                }
                                            }
                                            else
                                            {
                                                success = SetTime(args[0]);
                                            }
                                        }
                                    }break;
                                default: break;
                            }
                        }
                    }
                }
                if (success)
                {
                    messageInput.text = "";
                }
            }
        }
    }
    bool SetTime(string time)
    {
        List<string> times = new(){ "day", "night", "noon", "midnight"};
        int[] timesInt = { 350, 50, 500, 1100};
        if (times.Contains(time))
        {
            SetTime(timesInt[times.IndexOf(time)]);
            return true;
        }
        return false;
    }
    void SetTime(int time)
    {
        Worldtime.instance.currentLength = time;
    }
}
