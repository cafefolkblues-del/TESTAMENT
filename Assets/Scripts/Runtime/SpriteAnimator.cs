using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    [SerializeField] Sprite[] frames;
    [SerializeField] float fps = 8f;

    SpriteRenderer _sr;
    float _timer;
    int _index;
    int _direction = 1;

    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;

        _timer += Time.deltaTime;
        if (_timer >= 1f / fps)
        {
            _timer = 0f;
            _index += _direction;
            if (_index >= frames.Length - 1 || _index <= 0)
                _direction = -_direction;
            _sr.sprite = frames[_index];
        }
    }
}
