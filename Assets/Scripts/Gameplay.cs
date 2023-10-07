using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gameplay : Singleton<Gameplay>
{
    public static Gameplay instance;
    [SerializeField] private Player player;
    [SerializeField] private Camera mainCam;
    [SerializeField] private Material skyboxMaterial;
    [SerializeField] bool _isPlaying;
    [SerializeField] bool _isPaused;
    [SerializeField] bool _isInventoryOpen;

    public bool isPlayerPaused => _isPaused || !_isPlaying || isInventoryOpen;
    public bool isPlaying => _isPlaying;
    public bool isInventoryOpen => _isPlaying && _isInventoryOpen;
    private void Awake()
    {
        instance = this;
        mainCam.clearFlags = CameraClearFlags.Skybox;
        RenderSettings.skybox = skyboxMaterial;
    }
    public void StartGame()
    {
        _isPlaying = true;
        _isInventoryOpen = false;
        _isPaused = false;
        ChunkLoadingManager.instance.StartPlaying();
        player.StartGame();
        RenderSettings.skybox = null;
        mainCam.clearFlags = CameraClearFlags.SolidColor;
    }
    public void EndGame()
    {
        _isPlaying = false;
        _isPaused = false;
        _isInventoryOpen = false;
        player.EndGame();
        ChunkLoadingManager.instance.EndGame();
        RenderSettings.skybox = skyboxMaterial;
        mainCam.clearFlags = CameraClearFlags.Skybox;
    }
    private void Update()
    {
        if(!isPlayerPaused && !isInventoryOpen && !Command.Instance.screenOpened)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (isPlaying)
        {
            if(_isInventoryOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                _isInventoryOpen = false;
            }else if(!_isInventoryOpen && Input.GetKeyDown(KeyCode.E))
            {
                _isInventoryOpen = !_isInventoryOpen;
            }
            if (!isPlayerPaused && Input.GetKeyDown(KeyCode.P))
            {
                _isPaused = !_isPaused;
            }
        }
    }

    public void Unpause()
    {
        _isPaused = false;
    }
}
