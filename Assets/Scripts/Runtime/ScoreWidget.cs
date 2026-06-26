using TMPro;
using UnityEngine;

// 거리 스코어 위젯 — GameState.distanceScore 표시 + 적 처치 시 바운스.
// UI 기능 분리: 자기 데이터(distanceScore)·이벤트(OnAnyKilled)만 구독, 다른 위젯과 결합 없음.
public class ScoreWidget : MonoBehaviour
{
    [SerializeField] GameState _gameState;
    [SerializeField] TMP_Text  _text;
    [SerializeField] float     _bounceScale  = 1.3f;   // 처치 순간 확대 배율
    [SerializeField] float     _recoverSpeed = 8f;      // 원래 크기 복귀 속도

    void OnEnable()  => EnemyHealth.OnAnyKilled += Bounce;
    void OnDisable() => EnemyHealth.OnAnyKilled -= Bounce;   // 구독 해제 — 씬 전환 누수 방지

    void Update()
    {
        // distanceScore는 매프레임 증가(시간 기반) → polling 적합. ScoreFormat이 floor→정수 m.
        _text.text = ScoreFormat.Display(_gameState.distanceScore) + " m";
        // 바운스 복귀 — localScale을 1로 수렴
        _text.transform.localScale = Vector3.Lerp(_text.transform.localScale, Vector3.one, _recoverSpeed * Time.deltaTime);
    }

    // 적 처치 순간 확대 → Update에서 서서히 복귀 (튕기는 피드백)
    void Bounce() => _text.transform.localScale = Vector3.one * _bounceScale;
}
