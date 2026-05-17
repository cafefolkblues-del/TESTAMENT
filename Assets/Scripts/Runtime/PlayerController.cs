using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(AbilityHandler))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] LayerMask _groundLayer;
    [SerializeField] Vector2   _groundCheckSize   = new Vector2(0.7f, 0.1f);
    [SerializeField] float     _airMoveMultiplier = 0.7f;

    [Header("Jump Polish")]
    [SerializeField] float _coyoteTime             = 0.2f;
    [SerializeField] float _jumpBufferTime         = 0.3f;
    [SerializeField] float _jumpCutMultiplier      = 0.5f;
    [SerializeField] float _fallGravityMultiplier  = 2f;
    [SerializeField] float _maxFallSpeed           = 15f;
    [SerializeField] float _maxFallSpeedReachTime  = 1f;

    Rigidbody2D       _rb;
    CapsuleCollider2D _col;
    SpriteRenderer    _sr;
    AbilityHandler    _ability;
    float             _moveSpeed;
    float             _jumpForce;
    bool              _isGrounded;
    bool              _inputEnabled = true;
    float             _defaultGravityScale;
    bool              _canDoubleJump;
    float             _coyoteTimer;
    float             _jumpBufferTimer;
    bool              _isJumpHeld;

    // New Input System: 콜백 기반으로 입력 분리, 하드코딩 제거
    PlayerInputActions _input;
    float _moveInput;
    bool  _fireHeld;
    bool  _altFireHeld;

    void Awake()
    {
        _rb      = GetComponent<Rigidbody2D>();
        _col     = GetComponent<CapsuleCollider2D>();
        _sr      = GetComponent<SpriteRenderer>();
        _ability = GetComponent<AbilityHandler>();
        _defaultGravityScale = _rb.gravityScale;

        // New Input System 인스턴스 생성
        _input = new PlayerInputActions();
    }

    void OnEnable()
    {
        _input.Player.Enable();

        // Move: ReadValue로 폴링 (held 상태 자연스러움)
        // Jump: 콜백으로 버퍼링/릴리즈 감지
        _input.Player.Jump.performed += OnJumpPerformed;
        _input.Player.Jump.canceled  += OnJumpCanceled;

        // Fire/AltFire: 콜백으로 down/up 감지 + held 플래그
        _input.Player.Fire.performed    += OnFirePerformed;
        _input.Player.Fire.canceled     += OnFireCanceled;
        _input.Player.AltFire.performed += OnAltFirePerformed;
        _input.Player.AltFire.canceled  += OnAltFireCanceled;
    }

    void OnDisable()
    {
        _input.Player.Jump.performed -= OnJumpPerformed;
        _input.Player.Jump.canceled  -= OnJumpCanceled;
        _input.Player.Fire.performed    -= OnFirePerformed;
        _input.Player.Fire.canceled     -= OnFireCanceled;
        _input.Player.AltFire.performed -= OnAltFirePerformed;
        _input.Player.AltFire.canceled  -= OnAltFireCanceled;

        _input.Player.Disable();
    }

    public void SetInputEnabled(bool enabled) => _inputEnabled = enabled;

    public void Init(CharacterData data)
    {
        _moveSpeed = data.moveSpeed;
        _jumpForce = data.jumpForce;
        _ability.Init(data.leftAbility, data.rightAbility);
    }

    void Update()
    {
        if (!_inputEnabled || _moveSpeed <= 0f) return;

        // Move는 매 프레임 ReadValue로 폴링 (부드러운 입력)
        _moveInput = _input.Player.Move.ReadValue<float>();

        HandleMovement();
        HandleJump();
        HandleGravity();
        HandleAbilityHeld();
    }

    void FixedUpdate() => GroundCheck();

    #region Input Callbacks

    // Jump performed: 버퍼 타이머 시작
    void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log("[Input] Jump performed");
        _jumpBufferTimer = _jumpBufferTime;
        _isJumpHeld = true;
    }

    // Jump canceled: 점프컷 (짧게 누르면 낮게 점프)
    void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        Debug.Log("[Input] Jump canceled");
        _isJumpHeld = false;
        if (_rb.velocity.y > 0f)
            _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * _jumpCutMultiplier);
    }

    void OnFirePerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log("[Input] Fire performed");
        _fireHeld = true;
        _ability.OnLeftDown();
    }

    void OnFireCanceled(InputAction.CallbackContext ctx)
    {
        Debug.Log("[Input] Fire canceled");
        _fireHeld = false;
        _ability.OnLeftUp();
    }

    void OnAltFirePerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log("[Input] AltFire performed");
        _altFireHeld = true;
        _ability.OnRightDown();
    }

    void OnAltFireCanceled(InputAction.CallbackContext ctx)
    {
        Debug.Log("[Input] AltFire canceled");
        _altFireHeld = false;
        _ability.OnRightUp();
    }

    #endregion

    void HandleMovement()
    {
        float h = _moveInput;
        if (h != 0f) Debug.Log($"[Input] Move: {h}");

        float speed = _moveSpeed * (_isGrounded ? 1f : _airMoveMultiplier);
        _rb.velocity = new Vector2(h * speed, _rb.velocity.y);

        // 스프라이트 방향 전환
        if      (h > 0f) _sr.flipX = false;
        else if (h < 0f) _sr.flipX = true;
    }

    void HandleJump()
    {
        _coyoteTimer     -= Time.deltaTime;
        _jumpBufferTimer -= Time.deltaTime;

        bool canJump = _coyoteTimer > 0f || _canDoubleJump;
        if (_jumpBufferTimer > 0f && canJump)
        {
            string jumpType = _coyoteTimer > 0f ? "Ground/Coyote" : "Double";
            Debug.Log($"[Jump] {jumpType} jump executed");
            _rb.velocity = new Vector2(_rb.velocity.x, _jumpForce);
            _jumpBufferTimer = 0f;

            if (_coyoteTimer > 0f)
            {
                _coyoteTimer = 0f;
            }
            else
            {
                _canDoubleJump = false;
            }
        }
    }

    // 상승/하강 중력 분리: 하강 시 빠르게 떨어져 게임 템포 유지
    void HandleGravity()
    {
        if (_rb.velocity.y <= 0f)
            _rb.gravityScale = _defaultGravityScale * _fallGravityMultiplier;
        else
            _rb.gravityScale = _defaultGravityScale;

        if (_rb.velocity.y < -_maxFallSpeed)
            _rb.velocity = new Vector2(_rb.velocity.x, -_maxFallSpeed);
    }

    // held 상태 매 프레임 전달 (기관총 등 연사 무기용)
    void HandleAbilityHeld()
    {
        if (_fireHeld)    _ability.OnLeftHeld();
        if (_altFireHeld) _ability.OnRightHeld();
    }

    void GroundCheck()
    {
        bool wasGrounded = _isGrounded;
        Vector2 origin = (Vector2)transform.position
                       + _col.offset
                       + Vector2.down * (_col.size.y * 0.5f);
        // OverlapBox: 박스 형태로 바닥 감지, 캡슐보다 넓은 범위 체크 가능
        _isGrounded = Physics2D.OverlapBox(origin, _groundCheckSize, 0f, _groundLayer);

        if (_isGrounded)
        {
            _canDoubleJump = true;
            _coyoteTimer   = _coyoteTime;
        }
        else if (wasGrounded)
        {
            // 막 떨어진 순간: 코요테 타임 시작
            _coyoteTimer = _coyoteTime;
        }
    }
}
