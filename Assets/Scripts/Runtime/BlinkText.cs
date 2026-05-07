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

    IEnumerator Blink()
    {
        while (true)
        {
            _text.enabled = !_text.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
