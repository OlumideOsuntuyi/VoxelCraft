using System.Collections.Generic;
using System.IO;

using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;
    public static Vector3 position;
    public static Vector3 rotation;
    public VoxelPhysics physics;
    public SurvivalSystem survival;
    public Inventory inventory;
    public int orientation = 0;


    public Transform aim;
    public Transform highlightBlock;
    public Transform placeBlock;

    public float checkIncrement = 0.1f;
    public float reach = 8f;

    public Animator animator;
    public AudioSource walkAudio;

    public BlockState lastLookAt;
    public PlayerWorldData worldData = null;
    public PlayerAccountInfo accountInfo = new();
    public string playerWorldData => Path.Combine(World.world.worldFolder, Path.Combine("players", $"{accountInfo.name}"));
    private void Awake()
    {
        instance = this;
        physics = gameObject.GetComponent<VoxelPhysics>();
    }
    private void Start()
    {
        lastLookAt = WorldData.Block(new BlockPosition(new()));
        physics.feetBlock = WorldData.Block(new BlockPosition(new()));
        physics.headBlock = WorldData.Block(new BlockPosition(new()));
    }
    public void StartGame()
    {
        worldData = FileHandler.LoadObject<PlayerWorldData>(playerWorldData);
        inventory.StartGame(worldData.slots, worldData._currentSlot);
        survival.StartGame();
        transform.position = new Vector3(worldData.lastPosition.p_x, worldData.lastPosition.p_y, worldData.lastPosition.p_z);
        transform.eulerAngles += Vector3.up * worldData.lastPosition.r_y;
        physics.xIncrease = worldData.lastPosition.r_x;
    }
    public void EndGame()
    {
        worldData.slots = inventory.EndGame();
        worldData._currentSlot = inventory._currentSlot;
        FileHandler.SaveObject(worldData, playerWorldData);
        survival.EndGame();
    }
    private void Update()
    {
        if (!Gameplay.instance.isPlayerPaused)
        {
            orientation = physics.orientation;
            HandleMultiThreadInfo();

            GetPlayerInputs();
            placeCursorBlocks();
            HandleInventory();
            HandleData();
            animator.SetBool("isWalking", physics.vertical != 0);
            animator.SetFloat("speed", Mathf.Abs(physics.vertical));

        }
        aim.eulerAngles = new Vector3(-physics.head.eulerAngles.x, transform.eulerAngles.y, 0);
        aim.transform.position = physics.head.position;
    }
    void HandleMultiThreadInfo()
    {
        position = transform.position;
        rotation = aim.eulerAngles;
    }

    readonly Vector3 offset = new(0.5f, 0.5f, 0.5f);
    private void placeCursorBlocks()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();
        while (step < reach)
        {

            Vector3 pos = (aim.position) + (aim.forward * step);

            if (WorldData.IsBlockSolid(pos))
            {
                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z)) + offset;
                placeBlock.position = lastPos + offset;

                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);
                lastLookAt = WorldData.Block(new BlockPosition(pos));
                return;

            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;

        }

        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);

    }

    private void GetPlayerInputs()
    {
        physics.horizontal = Input.GetAxis("Horizontal");
        physics.vertical = Input.GetAxis("Vertical");
        physics.mouseHorizontal = Input.GetAxis("Mouse X") * Settings.instance.mouseSensitivity * 1.5f;
        physics.mouseVertical = Input.GetAxis("Mouse Y") * Settings.instance.mouseSensitivity * 2;

        if (Input.GetButtonDown("Sprint"))
            physics.isSprinting = true;
        if (Input.GetButtonUp("Sprint"))
            physics.isSprinting = false;

        if (physics.isGrounded && Input.GetButtonDown("Jump"))
            physics.jumpRequest = true;

        if (highlightBlock.gameObject.activeSelf)
        {
            // Destroy block.
            if (Input.GetMouseButtonDown(1))
            {
                animator.SetTrigger("placing");
                animator.SetBool("place", false);
                BlockState block = WorldData.Block(new(highlightBlock.position - offset));
                if (World.world.chunks.ContainsKey(block.position.chunkPosition.position))
                {
                    inventory.AddItems(block.id, 1);
                    World.world.chunks[block.position.chunkPosition.position].EditVoxel(block.position, 0);
                }
            }

            // Place block.
            if (Input.GetMouseButtonDown(0))
            {
                animator.SetTrigger("placing");
                animator.SetBool("place", true);
                BlockState block = WorldData.Block(new(placeBlock.position - offset));
                if (World.world.chunks.ContainsKey(block.position.chunkPosition.position))
                {
                    int placeID = inventory.TakeItem(inventory._currentSlot);
                    if (placeID > 0)
                    {
                        World.world.chunks[block.position.chunkPosition.position].EditVoxel(block.position, placeID, physics.orientation);
                    }
                }
            }
        }

    }
    private void HandleInventory()
    {
        int key = GetPressedNumber() - 1;
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        int dir = scroll > 0 ? 1 : scroll < 0 ? -1 : 0;
        if(dir != 0)
        {
            inventory.MoveSlot(dir);
        }
        if(key is >= 0 and < 9)
        {
            inventory._currentSlot = key;
        }
    }
    int GetPressedNumber()
    {
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                return i;
            }
        }
        return -1; // No number key was pressed
    }

    private void HandleData()
    {
        if(worldData == null)
        {
            worldData.lastPosition = new PlayerPosition(position, rotation);
        }
    }
    [System.Serializable]
    public class PlayerAccountInfo
    {
        public string name = "player";
    }
    [System.Serializable]
    public class PlayerWorldData
    {
        public PlayerPosition lastPosition = new();
        public int _currentSlot = 0;
        public List<Inventory.Slot> slots = new();
    }
    [System.Serializable]
    public class PlayerPosition
    {
        public float p_x;
        public float p_y;
        public float p_z;


        public float r_x;
        public float r_y;
        public float r_z;

        public PlayerPosition()
        {

        }
        public PlayerPosition(Vector3 position, Vector3 rotation)
        {
            p_x = position.x;
            p_y = position.y;
            p_z = position.z;


            r_x = rotation.x;
            r_y = rotation.y;
            r_z = rotation.z;
        }
    }
}
