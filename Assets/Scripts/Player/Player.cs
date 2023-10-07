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
    private void Update()
    {
        if (!Gameplay.instance.isPlayerPaused)
        {
            orientation = physics.orientation;
            HandleMultiThreadInfo();

            GetPlayerInputs();
            placeCursorBlocks();
            HandleInventory();
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

    private void placeCursorBlocks()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {

            Vector3 pos = (aim.position) + (aim.forward * step);

            if (WorldData.IsBlockSolid(pos))
            {
                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;

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
                BlockState block = WorldData.Block(new(highlightBlock.position));
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
                BlockState block = WorldData.Block(new(placeBlock.position));
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
}
