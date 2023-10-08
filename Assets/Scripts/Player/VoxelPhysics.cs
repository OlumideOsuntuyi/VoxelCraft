using UnityEngine;

public class VoxelPhysics : MonoBehaviour
{
    public bool isGrounded;
    public bool isSprinting;

    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.8f;

    public float playerWidth = 0.15f;
    public float boundsTolerance = 0.1f;

    public int orientation;

    public float horizontal;
    public float vertical;
    public float mouseHorizontal;
    public float mouseVertical;

    public Transform head;
    public Vector3 velocity;
    public float verticalMomentum = 0;
    public bool jumpRequest;
    public bool sleep;
    public Vector3Int size;

    internal float xIncrease = 0;

    public BlockState feetBlock, legBlock, headBlock;
    public EntityState entityState;
    public AudioSource feetAudio, legAudio, headAudio;
    private Player player;
    private SurvivalSystem survival;

    private void Awake()
    {
        _ = TryGetComponent(out player);
        _ = TryGetComponent(out survival);
    }
    private void Update()
    {
        if (!Gameplay.instance.isPlayerPaused)
        {
            Vector3 XZDirection = transform.forward;
            XZDirection.y = 0;
            if (Vector3.Angle(XZDirection, Vector3.forward) <= 45)
                orientation = 0; // Player is facing forwards.
            else if (Vector3.Angle(XZDirection, Vector3.right) <= 45)
                orientation = 5;
            else if (Vector3.Angle(XZDirection, Vector3.back) <= 45)
                orientation = 1;
            else
                orientation = 4;

            if (!sleep && !Gameplay.instance.isPlayerPaused)
            {
                feetBlock = WorldData.Block(new BlockPosition(Vector3Int.FloorToInt(transform.position) + Vector3Int.down));
                legBlock = WorldData.Block(new BlockPosition(Vector3Int.FloorToInt(transform.position)));
                headBlock = WorldData.Block(new BlockPosition(Vector3Int.FloorToInt(transform.position) + (Vector3Int.up * size.y)));
                CalculateVelocity();
                CheckFall();
                CheckWalk();
                if (jumpRequest)
                    Jump();

                bool isWalking = vertical != 0 || horizontal != 0;
                transform.Rotate(Vector3.up * mouseHorizontal);
                //head.Rotate(Vector3.right * -mouseVertical);
                xIncrease = Mathf.Clamp(xIncrease + mouseVertical, -86, 86);
                head.transform.localEulerAngles = Vector3.left * xIncrease;
                if (isWalking)
                {
                }
                transform.Translate(velocity, Space.World);
                var newPos = new Vector3()
                {
                    x = transform.position.x,
                    y = Mathf.Clamp(transform.position.y, -40, 1000),
                    z = transform.position.z
                };
                transform.position = newPos;
            }
        }
    }

    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;

    }

    private void CalculateVelocity()
    {
        // Affect vertical momentum with gravity.
        if (verticalMomentum > gravity)
            verticalMomentum += Time.deltaTime * gravity * legBlock.block.gravityMultiplier;

        // if we're sprinting, use the sprint multiplier.
        float speed = (isSprinting ? sprintSpeed : walkSpeed) * feetBlock.block.walkMultiplier * legBlock.block.walkObstructionMultiplier;
        Vector3 direction = ((transform.forward * vertical) + (transform.right * horizontal * .65f));
        velocity = direction * Time.deltaTime * speed;

        // Apply vertical momentum (falling/jumping).
        velocity += Vector3.up * verticalMomentum * Time.deltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;

        if (velocity.y < 0)
            velocity.y = checkDownSpeed(velocity.y);
        else if (velocity.y > 0)
            velocity.y = checkUpSpeed(velocity.y);
    }

    private float checkDownSpeed(float downSpeed)
    {

        if (
            WorldData.IsBlockSolid(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            WorldData.IsBlockSolid(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            WorldData.IsBlockSolid(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) ||
            WorldData.IsBlockSolid(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth))
           )
        {

            isGrounded = true;
            return 0;

        }
        else
        {

            isGrounded = false;
            return downSpeed;

        }

    }

    private float checkUpSpeed(float upSpeed)
    {

        if (
            WorldData.IsBlockSolid(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) ||
            WorldData.IsBlockSolid(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) ||
            WorldData.IsBlockSolid(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)) ||
            WorldData.IsBlockSolid(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth))
           )
        {

            return 0;

        }
        else
        {

            return upSpeed;

        }

    }

    public bool front
    {

        get
        {
            if (
                WorldData.IsBlockSolid(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                WorldData.IsBlockSolid(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth))
                )
                return true;
            else
                return false;
        }

    }
    public bool back
    {

        get
        {
            if (
                WorldData.IsBlockSolid(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                WorldData.IsBlockSolid(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth))
                )
                return true;
            else
                return false;
        }

    }
    public bool left
    {

        get
        {
            if (
                WorldData.IsBlockSolid(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
                WorldData.IsBlockSolid(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z))
                )
                return true;
            else
                return false;
        }

    }
    public bool right
    {

        get
        {
            if (
                WorldData.IsBlockSolid(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
                WorldData.IsBlockSolid(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z))
                )
                return true;
            else
                return false;
        }

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Vector3 center = transform.position + (Vector3.one * size.y * 0.5f);

        Vector3 worldSize = Vector3.Scale(size, transform.lossyScale);

        Gizmos.DrawWireCube(center, worldSize);
    }
    void CheckWalk()
    {
        if(!entityState.walking && (velocity.x != 0 && velocity.z != 0))
        {

        }else if(entityState.walking && (velocity.x == 0 && velocity.z == 0))
        {
            entityState.walk_obstacle_id = -1;
            entityState.walk_id = -1;
            feetAudio.Stop();
            legAudio.Stop();
        }
        if (entityState.walking)
        {
            if(entityState.walk_id != feetBlock.id)
            {
                var feetClip = feetBlock.block.walkAudio;
                if(feetClip != null)
                {
                    feetAudio.clip = feetClip;
                    feetAudio.Play();
                }
                else
                {

                }
            }
            if (entityState.walk_obstacle_id != legBlock.id)
            {
                var legClip = legBlock.block.walkAudio;
                if (legClip != null)
                {
                    legAudio.clip = legClip;
                    legAudio.Play();
                    feetAudio.volume = .25f;
                    legAudio.volume = .45f;
                }
                else
                {
                    feetAudio.volume = .35f;
                    legAudio.volume = 0f;
                }
            }
            entityState.walk_id = feetBlock.id;
            entityState.walk_obstacle_id = legBlock.id;
        }
    }
    void CheckFall()
    {
        if(!entityState.falling && !feetBlock.block.isSolid && !feetBlock.block.isWater)
        {
            StartFall();
        }
        if(entityState.falling && feetBlock.block.isSolid)
        {
            EndFall();
        }
    }
    void StartFall()
    {
        entityState.falling = true;
        entityState.startFall = transform.position.y;
    }
    void EndFall()
    {
        entityState.fallHeight = Mathf.Max(0, entityState.startFall - transform.position.y);
        survival.OnFall((1 - feetBlock.block.fallDamageNegation) * entityState.fallHeight);
        entityState.fallHeight = 0;
        entityState.startFall = 0;
        entityState.falling = false;
    }
    [System.Serializable]
    public struct EntityState
    {
        public bool walking;
        public bool running;
        public bool falling;
        public bool swiming;

        public float fallHeight;
        public float startFall;

        public int walk_id;
        public int walk_obstacle_id;
    }
}
