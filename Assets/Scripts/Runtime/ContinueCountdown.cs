using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ContinueCountdown : MonoBehaviour
{
    // TMP_Text 사용 중 — 아트 에셋 준비 시 Image 기반 UI로 교체 가능
    [SerializeField] TMP_Text _countText;
    [SerializeField] float    _duration = 10f;

    Coroutine _coroutine;

    public event Action OnComplete;

    public void StartCountdown()
    {
        StopCountdown();
        _coroutine = StartCoroutine(Tick());
    }

    public void StopCountdown()
    {
        if (_coroutine == null) return;
        StopCoroutine(_coroutine);
        _coroutine = null;
    }

    public void ResetCountdown()
    {
        StopCountdown();
        _countText.text = Mathf.CeilToInt(_duration).ToString();
    }

    IEnumerator Tick()
    {
        float remaining = _duration;
        while (remaining > 0f)
        {
            _countText.text = Mathf.CeilToInt(remaining).ToString();
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }
        _countText.text = "0";
        OnComplete?.Invoke();
    }
}
