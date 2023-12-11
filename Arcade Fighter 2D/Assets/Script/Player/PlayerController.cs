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

    public Action<PlayerType> OnPlayerDead;
    public bool isGameStart = false;
    private Coroutine idleReplay;
    [SerializeField] private SoundController soundController;
    public SoundController SoundController => soundController;

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
            soundController.PlaySound(SoundType.Dash);
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
            body2d.velocity = Vector2.zero;
            body2d.gravityScale = tempGravity;
            IsDashing = false;
            if (isDashAttack)
                yield return StartCoroutine(DashAttackProcess());
            ChangeState(PlayerStateType.Idle);
            yield return new WaitForSeconds(DASH_COOLDOWN);
            isDashAble = true;

        }
        IEnumerator DashAttackProcess()
        {
            soundController.PlaySound(SoundType.Attack);
            animator.SetTrigger("DashAttack");
            body2d.velocity = Vector2.zero;
            yield return new WaitForSeconds(0.2f);
            hitBox.ActiveDamage(20, 2);
            hitBox.gameObject.SetActive(true);
            hitBox.ShakeObject();
            yield return new WaitForSeconds(0.3f);
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
            soundController.PlaySound(SoundType.Block);
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
        if (!IsAttacking)
            StartCoroutine(AttackProcess());

        IEnumerator AttackProcess()
        {
            IsAttacking = true;
            animator.SetTrigger("Attack");
            hitBox.ActiveDamage(10, 1);
            yield return new WaitForSeconds(0.25f);
            soundController.PlaySound(SoundType.Attack);
            hitBox.gameObject.SetActive(true);
            hitBox.ShakeObject();
            yield return new WaitForSeconds(0.25f);
            hitBox.gameObject.SetActive(false);
            IsAttacking = false;
            ChangeState(PlayerStateType.Idle);
        }
    }

    public void Hurt()
    {
        StartCoroutine(HurtProcess());
        IEnumerator HurtProcess()
        {
            soundController.PlaySound(SoundType.Hurt);
            animator.SetTrigger("Hurt");
            yield return new WaitForSeconds(0.2f);
            ChangeState(PlayerStateType.Idle);
        }
    }

    public void BasicMovement()
    {
        if (IsReplaying || IsDead || IsAttacking || IsDashing || IsBlocking)
            return;

        airSpeed = body2d.velocity.y;
        animator.SetFloat("AirSpeed", airSpeed);

        if (!IsGrounded && groundChecker.State())
        {
            IsGrounded = true;
            animator.SetBool("Grounded", IsGrounded);
        }

        inputX = Input.GetAxisRaw(InputFactory.GetInputAxisMovement(Player));
        if (inputX != 0 && IsGrounded)
            soundController.PlayFootStep();
        else
            soundController.StopFootStep();
        // Swap direction of sprite depending on walk direction
        if (inputX > 0)
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else if (inputX < 0)
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        FlipIcon();

        // Move using Translate
        Vector3 movement = new Vector3(inputX * speed * Time.deltaTime, airSpeed * Time.deltaTime, 0f);
        body2d.transform.Translate(movement);

        playerVelocity = body2d.velocity;

        // Jump
        if (Input.GetKeyDown(InputFactory.GetKeyCode(Player, ActionKey.Jump)) && IsGrounded && (!IsReplaying && !IsDead))
        {
            soundController.PlaySound(SoundType.Jump);
            animator.SetTrigger("Jump");
            IsGrounded = false;
            animator.SetBool("Grounded", IsGrounded);

            body2d.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);

            groundChecker.Disable(0.2f);
        }

        // Run
        if (Mathf.Abs(inputX) > Mathf.Epsilon)
            animator.SetInteger("AnimState", (int)AnimState.Run);
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
        if (isGameStart)
            playerState.UpdateState();
    }
    private void OnTakeDamage(int currentHealth)
    {
        ChangeState(PlayerStateType.Hurt);
    }
    private void OnDead()
    {
        soundController.PlaySound(SoundType.Dead);
        IsDead = true;
        health.gameObject.SetActive(false);
        animator.SetTrigger("Death");
        animator.SetBool("Dead", true);
        OnPlayerDead?.Invoke(Player);
    }
    public void Reset()
    {
        IsDead = false;
        health.gameObject.SetActive(true);
        health.Reset();
        animator.SetTrigger("Recover");
        animator.SetBool("Dead", false);
        ResetToStartPosition();
    }
    public void ApplyReplayDead()
    {
        health.SetForceDead();
    }

    public void ApplyReplayInput(bool attackInput, bool blockInput, bool dashInput, Vector2 playerPosition)
    {
        this.transform.position = playerPosition;
        if (attackInput)
            Attack();
        if (blockInput)
            Block();
        if (dashInput)
            Dash();
    }

    public void ApplyReplayMovement(float inputX, bool jump) //? Move Jump
    {
        if (idleReplay != null)
            StopCoroutine(idleReplay);
        if (jump)
            if (IsAttacking || IsDashing || IsBlocking)
                return;
        airSpeed = body2d.velocity.y;
        animator.SetFloat("AirSpeed", airSpeed);

        if (!IsGrounded && groundChecker.State())
        {
            IsGrounded = true;
            animator.SetBool("Grounded", IsGrounded);
        }

        if (inputX != 0 && IsGrounded)
            soundController.PlayFootStep();
        else
            soundController.StopFootStep();
        if (inputX > 0)
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else if (inputX < 0)
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        FlipIcon();

        Vector3 movement = new Vector3(inputX * speed * Time.deltaTime, airSpeed * Time.deltaTime, 0f);
        body2d.transform.Translate(movement);

        playerVelocity = body2d.velocity;

        // Jump
        if (jump && IsGrounded && !IsDead)
        {
            soundController.StopFootStep();
            soundController.PlaySound(SoundType.Jump);
            animator.SetTrigger("Jump");
            IsGrounded = false;
            animator.SetBool("Grounded", IsGrounded);

            body2d.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);

            groundChecker.Disable(0.2f);
        }
        // Run
        if (Mathf.Abs(inputX) > Mathf.Epsilon)
            animator.SetInteger("AnimState", (int)AnimState.Run);
        else
            animator.SetInteger("AnimState", (int)AnimState.Idle);
        idleReplay = StartCoroutine(StopRunningAnimation());

        IEnumerator StopRunningAnimation()
        {
            yield return new WaitForSeconds(0.5f);
            animator.SetInteger("AnimState", (int)AnimState.Idle);
            soundController.StopFootStep();
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

