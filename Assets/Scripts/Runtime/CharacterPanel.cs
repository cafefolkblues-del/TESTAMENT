using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// CanvasGroup RequireComponent 이유 — 본체 시차 페이드 인 위해 prefab에 강제 부착
[RequireComponent(typeof(CanvasGroup))]
public class CharacterPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] Image _portrait;
    [SerializeField] Image _border;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] RectTransform _nameRect;
    [SerializeField] Material _grayscaleMaterialTemplate;
    [SerializeField] float hoverScale = 1.05f;
    [SerializeField] float animDuration = 0.15f;
    [SerializeField] float nameSlideOffset = 12f;

    CharacterSelectManager _manager;
    CanvasGroup _selfCanvasGroup;
    Material _mat;
    Vector3 _baseScale;
    float _nameBaseY;
    bool _locked;
    Coroutine _hoverAnim;

    // 외부 read-only — Init에서 주입된 캐릭터 데이터
    public CharacterData Data { get; private set; }

    void Awake()
    {
        // RequireComponent로 보장된 CanvasGroup 캐시 — Manager의 시차 페이드인 대상
        _selfCanvasGroup = GetComponent<CanvasGroup>();
        _selfCanvasGroup.alpha = 0f;   // 등장 전 비표시 — Manager가 FadeIn으로 0→1
        _locked = true;                // 등장 완료 전까지 호버/클릭 차단

        // new Material 인스턴스 생성 이유 — 패널마다 _GrayscaleAmount를 독립 토글
        // (sharedMaterial이면 호버 시 모든 패널이 동시 변화)
        _mat = new Material(_grayscaleMaterialTemplate);
        _portrait.material = _mat;
        _baseScale = transform.localScale;
        _nameBaseY = _nameRect.anchoredPosition.y;

        _mat.SetFloat("_GrayscaleAmount", 1f);
        _mat.SetFloat("_FlashAmount", 0f);
        SetBorderAlpha(0f);
        SetNameAlpha(0f);
        SetNameY(_nameBaseY - nameSlideOffset);
    }

    void OnDestroy() => Destroy(_mat);

    // 동적 생성 시 Manager가 호출 — 매니저 참조 + 캐릭터 데이터 주입
    public void Init(CharacterSelectManager manager, CharacterData data)
    {
        _manager = manager;
        Data = data;
        // null 가드 제거 이유 — 호출자(Manager) 책임. 누락 시 NRE로 즉시 발견
        _portrait.sprite = data.portrait;
        _nameText.text   = data.characterName;
    }

    // 선택 후 다른 패널 클릭 차단 / 시차 등장 중 초기 입력 차단
    public void Lock()   => _locked = true;
    // 시차 등장 완료 후 Manager가 호출 — 호버/클릭 활성
    public void Unlock() => _locked = false;

    // 본체 페이드인 — Manager의 PanelsCascadeFadeIn에서 시차로 호출
    // CanvasGroupExtensions.Fade로 일원화 (StartScene/Tutorial/CharacterSelect 페이드와 동일 경로)
    public IEnumerator FadeIn(float duration) => _selfCanvasGroup.Fade(0f, 1f, duration);

    public void OnPointerEnter(PointerEventData e)
    {
        if (_locked) return;
        StopHoverAnim();
        _hoverAnim = StartCoroutine(AnimateHover(true));
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (_locked) return;
        StopHoverAnim();
        _hoverAnim = StartCoroutine(AnimateHover(false));
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (_locked) return;
        _manager.OnPanelSelected(this);
    }

    // 선택 시 흰색 플래시 N회 — Manager의 PlayTransition에서 yield return으로 chain.
    // FlashOverlay 박스 대신 shader _FlashAmount로 Portrait sprite 밝기 변화 (캐릭터 실루엣 그대로, 박스 튀어나옴 X)
    public IEnumerator BlinkWhite(int count, float interval)
    {
        for (int i = 0; i < count * 2; i++)
        {
            _mat.SetFloat("_FlashAmount", i % 2 == 0 ? 1f : 0f);
            yield return new WaitForSeconds(interval);
        }
        _mat.SetFloat("_FlashAmount", 0f);
    }

    void StopHoverAnim()
    {
        if (_hoverAnim != null) StopCoroutine(_hoverAnim);
    }

    // 호버 전체 시각 변환을 한 메서드에 묶음 — 그레이스케일/스케일/보더/이름알파/이름Y 동시 보간
    IEnumerator AnimateHover(bool entering)
    {
        float fromGray        = _mat.GetFloat("_GrayscaleAmount");
        float toGray          = entering ? 0f : 1f;
        float fromScale       = transform.localScale.x;
        float toScale         = entering ? _baseScale.x * hoverScale : _baseScale.x;
        float fromBorderAlpha = GetBorderAlpha();
        float toBorderAlpha   = entering ? 1f : 0f;
        float fromNameAlpha   = GetNameAlpha();
        float toNameAlpha     = entering ? 1f : 0f;
        float fromNameY       = _nameRect.anchoredPosition.y;
        float toNameY         = entering ? _nameBaseY : _nameBaseY - nameSlideOffset;

        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);

            _mat.SetFloat("_GrayscaleAmount", Mathf.Lerp(fromGray, toGray, t));
            transform.localScale = Vector3.one * Mathf.Lerp(fromScale, toScale, t);
            SetBorderAlpha(Mathf.Lerp(fromBorderAlpha, toBorderAlpha, t));
            SetNameAlpha(Mathf.Lerp(fromNameAlpha, toNameAlpha, t));
            SetNameY(Mathf.Lerp(fromNameY, toNameY, t));

            yield return null;
        }
    }

    void SetBorderAlpha(float a) { Color c = _border.color; c.a = a; _border.color = c; }
    float GetBorderAlpha() => _border.color.a;

    void SetNameAlpha(float a) { Color c = _nameText.color; c.a = a; _nameText.color = c; }
    float GetNameAlpha() => _nameText.color.a;

    void SetNameY(float y) { Vector2 p = _nameRect.anchoredPosition; p.y = y; _nameRect.anchoredPosition = p; }
}
