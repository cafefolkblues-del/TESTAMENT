using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(AbilityHandler))]
public class PlayerController : MonoBehaviour
{
    // Layer 이름으로 박는 이유 — 인스펙터 LayerMask 드롭다운이 깨져 0 되는 사고 반복됨.
    // 코드에서 Awake 1회 GetMask로 박으면 사람 손 닿는 면적 제거. Layer 정의 바뀔 때만 수정.
    [SerializeField] string    _groundLayerName   = "Ground";
    LayerMask                  _groundLayer;
    [SerializeField] Vector2   _groundCheckSize   = new Vector2(0.7f, 0.1f);
    [SerializeField] float     _airMoveMultiplier = 0.7f;

    [Header("Jump Polish")]
    [SerializeField] float _coyoteTime             = 0.2f;
    [SerializeField] float _jumpBufferTime         = 0.3f;
    [SerializeField] float _jumpCutMultiplier      = 0.5f;
    [SerializeField] float _fallGravityMultiplier  = 2f;
    [SerializeField] float _maxFallSpeed           = 15f;

    Rigidbody2D       _rb;
    CapsuleCollider2D _col;
    SpriteRenderer    _sr;
    AbilityHandler    _ability;
    // 디버그 모니터링용 노출 — Init이 매번 CharacterData 값으로 덮어씀 (SO 단일 진실 유지)
    [SerializeField] float _moveSpeed;
    [SerializeField] float _jumpForce;
    bool              _isGrounded;
    bool              _inputEnabled = true;
    float             _defaultGravityScale;
    bool              _canDoubleJump;
    float             _coyoteTimer;
    float             _jumpBufferTimer;
    // 점프 hold 플래그 제거 (2026-05-24) — 런앤건 속도감 우선, 정밀 점프 살림 (원샷 사망 보호)
    // 향후 검토 후보: (c) Air Dash hold — hold 동안 X속 가속. 캐릭터별 차별화 여지 (단, 릴 무적돌진과 겹침 주의)

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

        // LayerMask: 인스펙터 노출 X → 이름으로 박음. Layer 정의 사라지면 0 반환 → fail-loud
        _groundLayer = LayerMask.GetMask(_groundLayerName);
        if (_groundLayer.value == 0)
            Debug.LogError($"[PlayerController] Layer '{_groundLayerName}' not defined in TagManager — GroundCheck 영구 실패");

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
        // 상태 디버그 — 어떤 캐릭터/능력/스탯으로 초기화됐는지 (좌클자동/우클수동 검증용)
        Debug.Log($"[Player] 캐릭터 로드: {data.characterName} | 좌={data.leftAbility} 우={data.rightAbility} | moveSpeed={_moveSpeed} jumpForce={_jumpForce}");
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

    // Jump performed: 버퍼 타이머 시작 (실제 점프는 HandleJump에서 grounded/coyote/double 판정 후 실행)
    void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        _jumpBufferTimer = _jumpBufferTime;
    }

    // Jump canceled: 점프컷 (짧게 누르면 낮게 점프) — 상승 중일 때만 velocity.y 감쇄
    void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        if (_rb.velocity.y > 0f)
            _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * _jumpCutMultiplier);
    }

    void OnFirePerformed(InputAction.CallbackContext ctx)
    {
        _fireHeld = true;
        _ability.OnLeftDown();
    }

    void OnFireCanceled(InputAction.CallbackContext ctx)
    {
        _fireHeld = false;
        _ability.OnLeftUp();
    }

    void OnAltFirePerformed(InputAction.CallbackContext ctx)
    {
        _altFireHeld = true;
        _ability.OnRightDown();
    }

    void OnAltFireCanceled(InputAction.CallbackContext ctx)
    {
        _altFireHeld = false;
        _ability.OnRightUp();
    }

    #endregion

    void HandleMovement()
    {
        float h = _moveInput;
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

        // canJump: 지상(coyote 포함) 또는 더블점프 보유
        bool canJump = _coyoteTimer > 0f || _canDoubleJump;
        if (_jumpBufferTimer > 0f && canJump)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, _jumpForce);
            _jumpBufferTimer = 0f;

            // 코요테 점프(지상 점프 포함)는 coyote 소진, 더블점프는 더블 소진. 둘 다 소진하면 다음은 착지 전까지 불가
            if (_coyoteTimer > 0f) _coyoteTimer = 0f;
            else                   _canDoubleJump = false;
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
        // OverlapBox: 박스 형태로 바닥 감지, 캡슐 끝점 raycast보다 안정 (경사/모서리 노이즈 적음)
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
