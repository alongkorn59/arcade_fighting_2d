using UnityEngine;
using System.Collections;
using System;

public partial class PlayerControllerOld : MonoBehaviour
{
    private float gravity = -6f;
    private float gravityMultiplier = -6f;

    private const float DASH_COOLDOWN = 2f;

    public PlayerType Player;
    [SerializeField] float speed = 4.0f;
    [SerializeField] float jumpForce = 7.5f;
    private float _velocity;
    private Vector3 velocity;

    private Animator animator;
    public Animator Animator => animator;

    [SerializeField] private CharacterController characterController; // เปลี่ยนจาก Rigidbody2D เป็น CharacterController
    public CharacterController CharacterController => characterController;

    [SerializeField] private GroundChecker groundChecker;
    public bool IsGrounded = false;
    public bool IsDead = false;
    [SerializeField] private float airSpeed;
    public float AirSpeed => airSpeed;
    private float inputX = 0;
    [SerializeField] private Vector2 playerVelocity;

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

    public bool IsAttacking = false;

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
        // ChangeState(PlayerStateType.Idle);
    }

    void Start()
    {
        health.OnDead = OnDead;
        health.OnHealthUpdate += OnTakeDamage;
        animator = GetComponentInChildren<Animator>();
        // characterController = GetComponent<CharacterController>(); // เปลี่ยนจาก Rigidbody2D เป็น CharacterController
        groundChecker = transform.Find("GroundSensor").GetComponentInChildren<GroundChecker>();

        // Check if character just started falling
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
            // float tempGravity = characterController.gravity;
            // characterController.ConfigureCharacterController(isKinematic: true, useGravity: false);
            IsDashing = true;
            isDashAble = false;
            health.gameObject.SetActive(false);
            animator.SetBool("Dashing", IsDashing);
            float dashDirection = transform.localScale.x;

            characterController.Move(new Vector3(-dashDirection * speed * 2, 0, 0));

            yield return new WaitForSeconds(dashDuration);
            animator.SetBool("Dashing", false);
            health.gameObject.SetActive(true);
            // characterController.ConfigureCharacterController(isKinematic: false, useGravity: true);
            IsDashing = false;

            if (isDashAttack)
                yield return StartCoroutine(DashAttackProcess());

            yield return new WaitForSeconds(DASH_COOLDOWN);
            isDashAble = true;
        }

        IEnumerator DashAttackProcess()
        {
            Debug.Log("DashAttack");
            animator.SetTrigger("DashAttack");
            hitBox.ActiveDamage(12, 2);
            yield return new WaitForSeconds(0.5f);
            hitBox.gameObject.SetActive(false);
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
            // ChangeState(PlayerStateType.Idle);
        }
    }

    public void Attack()
    {
        // animator.SetInteger("AnimState", (int)AnimState.Idle);
        if (!IsAttacking)
            StartCoroutine(AttackProcess());

        IEnumerator AttackProcess()
        {
            IsAttacking = true;
            animator.SetTrigger("Attack");
            hitBox.ActiveDamage(6, 1);
            yield return new WaitForSeconds(0.25f);
            hitBox.gameObject.SetActive(true);
            hitBox.ShakeObject();
            yield return new WaitForSeconds(0.25f);
            hitBox.gameObject.SetActive(false);
            IsAttacking = false;
            // ChangeState(PlayerStateType.Idle);
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
            // ChangeState(PlayerStateType.Idle);
        }
    }

    public void BasicMovement() //? Move Jump
    {
        if (IsReplaying || IsDead || IsAttacking || IsDashing || IsBlocking)
            return;

        airSpeed = characterController.velocity.y; // เปลี่ยนจาก body2d.velocity.y เป็น characterController.velocity.y

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
            characterController.Move(new Vector3(inputX * speed, airSpeed, 0) * Time.deltaTime);

        playerVelocity = characterController.velocity;

        //Jump
        if (Input.GetKeyDown(InputFactory.GetKeyCode(Player, ActionKey.Jump)) && IsGrounded && (!IsReplaying && !IsDead))
        {
            animator.SetTrigger("Jump");
            IsGrounded = false;
            animator.SetBool("Grounded", IsGrounded);
            characterController.Move(new Vector3(0, jumpForce, 0)); // เปลี่ยนจาก body2d.velocity.x, jumpForce เป็น characterController.Move(new Vector3(0, jumpForce, 0))
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
        ApplyGravity();

        // Apply gravity
        if (characterController.isGrounded)
        {
            velocity.y = -0.5f; // reset vertical velocity if grounded
            IsGrounded = true;
        }
        else
        {
            velocity.y += Physics.gravity.y * Time.deltaTime;
            IsGrounded = false;
        }
        // playerState.UpdateState();
    }
    private void ApplyGravity()
    {
        _velocity += gravity * gravityMultiplier * Time.deltaTime;
        // characterController.Move(_velocity);
    }
    private void OnTakeDamage(int currentHealth)
    {
        Debug.Log($"{Player} TakeDamage");
        // ChangeState(PlayerStateType.Hurt);
    }
    private void OnDead()
    {
        Debug.Log($"{Player} Dead");
        IsDead = true;
        health.gameObject.SetActive(false);
        animator.SetTrigger("Death");
        animator.SetBool("Dead", true);
        OnPlayerDead?.Invoke();
    }
    public void Reset()
    {
        Debug.Log($"{Player} Dead");
        IsDead = false;
        health.gameObject.SetActive(true);
        health.Reset();
        animator.SetTrigger("Recover");
        animator.SetBool("Dead", false);
        ResetToStartPosition();
    }

    public void ApplyReplayInput(bool attackInput, bool blockInput, bool skillInput, bool dashInput, Vector2 playerPosition)
    {
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

        // Handle dashing
        if (dashInput)
        {
            Dash();
        }
    }
    public void ApplyReplayMovement(float inputX, bool jump)
    {
        if (IsAttacking || IsDashing || IsBlocking)
            return;

        // Set the player's position based on the recorded information
        // transform.position = playerPosition;

        airSpeed = characterController.velocity.y;

        // Set AirSpeed in animator
        animator.SetFloat("AirSpeed", airSpeed);

        // Check if the character just landed on the ground
        if (!IsGrounded && groundChecker.State())
        {
            IsGrounded = true;
            animator.SetBool("Grounded", IsGrounded);
        }

        // Swap direction of the sprite depending on walk direction
        if (inputX > 0)
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else if (inputX < 0)
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        FlipIcon();

        // Move
        if (IsGrounded || airSpeed > 0)
            characterController.Move(new Vector3(inputX * speed, airSpeed, 0) * Time.deltaTime);

        // Jump
        if (jump && IsGrounded)
        {
            animator.SetTrigger("Jump");
            IsGrounded = false;
            animator.SetBool("Grounded", IsGrounded);
            characterController.Move(new Vector3(0, jumpForce, 0));
            groundChecker.Disable(0.2f);
        }

        // Run
        if (Mathf.Abs(inputX) > Mathf.Epsilon)
            animator.SetInteger("AnimState", (int)AnimState.Run);
        // Idle
        else
            animator.SetInteger("AnimState", (int)AnimState.Idle);
    }
}
