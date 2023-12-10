using UnityEngine;
using System.Collections;
using System;

public partial class PlayerController : MonoBehaviour
{
    private const float DASH_COOLDOWN = 2f;

    public PlayerType Player;
    [SerializeField] float speed = 4.0f;
    [SerializeField] float jumpForce = 7.5f;

    private Animator animator;
    public Animator Animator => animator;

    private Rigidbody2D body2d;
    public Rigidbody2D Body2d => body2d;
    [SerializeField] private GroundChecker groundChecker;
    public bool IsGrounded = false;
    private bool isDead = false;
    [SerializeField] private float airSpeed;
    public float AirSpeed => airSpeed;
    private float inputX = 0;

    #region Dash Group
    public bool IsDashing = false;
    [SerializeField] private float dashDistance = 5.0f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private bool isDashAble = true;
    public bool IsDashAble => isDashAble;
    public bool isDashAttack = false;
    #endregion

    #region Block Group
    public bool IsBlocking = false;
    [SerializeField] private float blockDuration = 0.2f;
    [SerializeField] private bool isBlockAble = true;
    public bool IsBlockAble => isBlockAble;
    [SerializeField] private GameObject blockVfx;
    #endregion

    [SerializeField] private DamageObject hitBox;
    [SerializeField] private Health health;
    public Health Health => health;
    [SerializeField] private SpriteRenderer IconPlayer;
    private Vector2 startPosition;
    public bool IsReplaying = false;

    public Action OnPlayerDead;


    private void Awake()
    {
        startPosition = this.transform.position;
        string iconName = Player == PlayerType.Player1 ? "P1" : "P2";
        var image = Resources.Load<Sprite>($"Icon/{iconName}");
        IconPlayer.sprite = image;
    }

    void Start()
    {
        health.OnDead = OnDead;
        health.OnHealthUpdate += OnTakeDamage;
        animator = GetComponentInChildren<Animator>();
        body2d = GetComponent<Rigidbody2D>();
        hitBox.Init(this);
        groundChecker = transform.Find("GroundSensor").GetComponentInChildren<GroundChecker>();

        ChangeState(PlayerStateType.Idle);

        //Check if character just started falling
        if (IsGrounded && !groundChecker.State())
        {
            IsGrounded = false;
            animator.SetBool("Grounded", IsGrounded);
        }
    }
    public void ResetToStartPosition()
    {
        this.transform.position = startPosition;
    }

    public void Dash()
    {
        if (!IsDashing && isDashAble)
            StartCoroutine(DashProcess());

        IEnumerator DashProcess()
        {
            float tempGravity = body2d.gravityScale;
            body2d.gravityScale = 0;
            IsDashing = true;
            isDashAble = false;
            health.gameObject.SetActive(false);
            animator.SetBool("Dashing", IsDashing);
            float dashDirection = transform.localScale.x;

            body2d.velocity = Vector2.zero;

            body2d.velocity = new Vector2(-dashDirection * speed * 2, body2d.velocity.y);

            yield return new WaitForSeconds(dashDuration);
            animator.SetBool("Dashing", false);
            health.gameObject.SetActive(true);
            if (isDashAttack)
                yield return StartCoroutine(DashAttackProcess());
            body2d.velocity = Vector2.zero;
            body2d.gravityScale = tempGravity;
            IsDashing = false;
            ChangeState(PlayerStateType.Idle);
            yield return new WaitForSeconds(DASH_COOLDOWN);
            isDashAble = true;

        }
        IEnumerator DashAttackProcess()
        {
            animator.SetTrigger("DashAttack");
            hitBox.ActiveDamage(12, 2);
            yield return new WaitForSeconds(0.2f);
            body2d.velocity = Vector2.zero;
            hitBox.gameObject.SetActive(true);
            hitBox.ShakeObject();
            yield return new WaitForSeconds(0.5f);
            hitBox.gameObject.SetActive(false);
            ChangeState(PlayerStateType.Idle);
        }
    }

    public void Block()
    {
        if (!IsBlocking)
        {
            StartCoroutine(BlockProcess());
        }
        IEnumerator BlockProcess()
        {
            IsBlocking = true;
            blockVfx.SetActive(IsBlocking);
            health.gameObject.SetActive(!IsBlocking);
            animator.SetInteger("AnimState", (int)AnimState.Block);
            yield return new WaitForSeconds(1f);
            IsBlocking = false;
            blockVfx.SetActive(IsBlocking);
            health.gameObject.SetActive(!IsBlocking);
            animator.SetInteger("AnimState", (int)AnimState.Idle);
            ChangeState(PlayerStateType.Idle);
        }
    }

    public void Attack()
    {
        // animator.SetInteger("AnimState", (int)AnimState.Idle);
        StartCoroutine(AttackProcess());
        IEnumerator AttackProcess()
        {
            animator.SetTrigger("Attack");
            hitBox.ActiveDamage(6, 1);
            yield return new WaitForSeconds(0.25f);
            hitBox.gameObject.SetActive(true);
            hitBox.ShakeObject();
            yield return new WaitForSeconds(0.25f);
            hitBox.gameObject.SetActive(false);
            ChangeState(PlayerStateType.Idle);
        }
    }

    public void Hurt()
    {
        // animator.SetInteger("AnimState", (int)AnimState.Idle);
        StartCoroutine(HurtProcess());
        IEnumerator HurtProcess()
        {
            animator.SetTrigger("Hurt");
            yield return new WaitForSeconds(0.2f);
            ChangeState(PlayerStateType.Idle);
        }
    }

    public void BasicMovement() //? Move Jump
    {
        if (IsReplaying)
            return;
        airSpeed = body2d.velocity.y;

        //Set AirSpeed in animator
        animator.SetFloat("AirSpeed", airSpeed);

        //Check if character just landed on the ground
        if (!IsGrounded && groundChecker.State())
        {
            IsGrounded = true;
            animator.SetBool("Grounded", IsGrounded);
        }

        inputX = Input.GetAxis(InputFactory.GetInputAxisMovement(Player));
        // Swap direction of sprite depending on walk direction
        if (inputX > 0)
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else if (inputX < 0)
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        FlipIcon();
        // Move
        if (IsGrounded || airSpeed > 0)
            body2d.velocity = new Vector2(inputX * speed, airSpeed);


        //Jump
        if (Input.GetKeyDown(InputFactory.GetKeyCode(Player, ActionKey.Jump)) && IsGrounded && !IsReplaying)
        {
            animator.SetTrigger("Jump");
            IsGrounded = false;
            animator.SetBool("Grounded", IsGrounded);
            body2d.velocity = new Vector2(body2d.velocity.x, jumpForce);
            groundChecker.Disable(0.2f);
        }

        //Run
        if (Mathf.Abs(inputX) > Mathf.Epsilon)
            animator.SetInteger("AnimState", (int)AnimState.Run);

        //Idle
        else
            animator.SetInteger("AnimState", (int)AnimState.Idle);

    }
    public void ApplyReplayMovement(float inputX, bool jump) //? Move Jump
    {
        airSpeed = body2d.velocity.y;

        //Set AirSpeed in animator
        animator.SetFloat("AirSpeed", airSpeed);

        //Check if character just landed on the ground
        if (!IsGrounded && groundChecker.State())
        {
            IsGrounded = true;
            animator.SetBool("Grounded", IsGrounded);
        }

        // inputX = Input.GetAxis(InputFactory.GetInputAxisMovement(Player));
        // Swap direction of sprite depending on walk direction
        if (inputX > 0)
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else if (inputX < 0)
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        FlipIcon();
        // Move
        if (IsGrounded || airSpeed > 0)
            body2d.velocity = new Vector2(inputX * speed, airSpeed);


        //Jump
        if (jump && IsGrounded)
        {
            animator.SetTrigger("Jump");
            IsGrounded = false;
            animator.SetBool("Grounded", IsGrounded);
            body2d.velocity = new Vector2(body2d.velocity.x, jumpForce);
            groundChecker.Disable(0.2f);
        }

        //Run
        if (Mathf.Abs(inputX) > Mathf.Epsilon)
            animator.SetInteger("AnimState", (int)AnimState.Run);

        //Idle
        else
            animator.SetInteger("AnimState", (int)AnimState.Idle);

    }
    private void FlipIcon()
    {
        var x = this.transform.localScale.x;
        IconPlayer.transform.localScale = new Vector2(x * 0.2f, 0.2f);
    }

    void Update()
    {
        playerState.UpdateState();
    }
    private void OnTakeDamage(int currentHealth)
    {
        Debug.Log($"{Player} TakeDamage");
        ChangeState(PlayerStateType.Hurt);
    }
    private void OnDead()
    {
        Debug.Log($"{Player} Dead");
        isDead = true;
        health.gameObject.SetActive(false);
        animator.SetTrigger("Death");
        animator.SetBool("Dead", true);
        OnPlayerDead?.Invoke();
    }
    public void Reset()
    {
        Debug.Log($"{Player} Dead");
        isDead = false;
        health.gameObject.SetActive(true);
        health.Reset();
        animator.SetTrigger("Recover");
        animator.SetBool("Dead", false);
        ResetToStartPosition();
    }
    public void ApplyInput(float horizontalInput, bool jumpInput, bool attackInput, bool blockInput, bool skillInput, bool dashInput)
    {
        // Handle horizontal movement
        ApplyReplayMovement(horizontalInput, jumpInput);

        // Handle attacking
        if (attackInput)
        {
            Attack();
        }

        // Handle blocking
        if (blockInput)
        {
            Block();
        }

        // // Handle using a skill (customize this based on your game)
        // if (skillInput)
        // {
        //     UseSkill();
        // }

        // Handle dashing
        if (dashInput)
        {
            Dash();
        }
    }
}
public enum PlayerType
{
    Player1,
    Player2,
}

public enum AnimState : int
{
    Idle = 0,
    Run = 1,
    Block = 2,
}

