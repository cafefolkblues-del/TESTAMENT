using System.Collections;
using TMPro;
using UnityEngine;

public class BlinkText : MonoBehaviour
{
    [SerializeField] float blinkInterval = 0.6f;

    TMP_Text _text;

    void Start()
    {
        _text = GetComponent<TMP_Text>();
        StartCoroutine(Blink());
    }

    public IEnumerator BlinkTimes(int count, float interval)
    {
        StopAllCoroutines();
        _text.enabled = true;
        for (int i = 0; i < count * 2; i++)
        {
            _text.enabled = !_text.enabled;
            yield return new WaitForSeconds(interval);
        }
        _text.enabled = true;
    }

    IEnumerator Blink()
    {
        while (true)
        {
            _text.enabled = !_text.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
