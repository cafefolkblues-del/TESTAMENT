using UnityEngine;

public class ParallaxScroller : MonoBehaviour
{
    [SerializeField] float scrollSpeed = 1f;

    float _spriteWidth;

    void Start()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
            _spriteWidth = sr.sprite.bounds.size.x;
    }

    void Update()
    {
        transform.position += Vector3.left * scrollSpeed * Time.deltaTime;

        if (_spriteWidth > 0 && transform.position.x <= -_spriteWidth)
            transform.position += new Vector3(_spriteWidth * 2f, 0f, 0f);
    }
}
