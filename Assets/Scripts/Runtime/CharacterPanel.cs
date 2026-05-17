using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] CharacterData _data;
    [SerializeField] Image _portrait;
    [SerializeField] Image _border;
    [SerializeField] Image _flashOverlay;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] RectTransform _nameRect;
    [SerializeField] Material _grayscaleMaterialTemplate;
    [SerializeField] float hoverScale = 1.05f;
    [SerializeField] float animDuration = 0.15f;
    [SerializeField] float nameSlideOffset = 12f;

    CharacterSelectManager _manager;
    Material _mat;
    Vector3 _baseScale;
    float _nameBaseY;
    bool _locked;
    Coroutine _hoverAnim;

    public CharacterData Data => _data;

    void Awake()
    {
        _mat = new Material(_grayscaleMaterialTemplate);
        _portrait.material = _mat;
        _baseScale = transform.localScale;
        _nameBaseY = _nameRect.anchoredPosition.y;

        _mat.SetFloat("_GrayscaleAmount", 1f);
        SetBorderAlpha(0f);
        SetNameAlpha(0f);
        SetNameY(_nameBaseY - nameSlideOffset);
    }

    void OnDestroy() => Destroy(_mat);

    public void Init(CharacterSelectManager manager)
    {
        _manager = manager;
        _portrait.sprite = _data != null ? _data.portrait : null;
        _nameText.text   = _data != null ? _data.characterName : string.Empty;
    }

    public void Lock() => _locked = true;

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

    public IEnumerator BlinkWhite(int count, float interval)
    {
        for (int i = 0; i < count * 2; i++)
        {
            SetFlashAlpha(i % 2 == 0 ? 1f : 0f);
            yield return new WaitForSeconds(interval);
        }
        SetFlashAlpha(0f);
    }

    void StopHoverAnim()
    {
        if (_hoverAnim != null) StopCoroutine(_hoverAnim);
    }

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

    void SetFlashAlpha(float a) { Color c = _flashOverlay.color; c.a = a; _flashOverlay.color = c; }

    void SetNameY(float y) { Vector2 p = _nameRect.anchoredPosition; p.y = y; _nameRect.anchoredPosition = p; }
}
