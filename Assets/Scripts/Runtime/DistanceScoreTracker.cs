using UnityEngine;

// 거리 스코어 = 생존시간 × 열차속도. 트레드밀(고정 스폰) 구조라 player.x 대신 시간 누적으로 거리 환산.
// 가드 불필요: 단조 증가(+= 양수)라 음수 없음, float 오버플로는 수년치 연속 플레이라 비현실적.
public class DistanceScoreTracker : MonoBehaviour
{
    [SerializeField] GameState _gameState;
    [SerializeField] float     _metersPerSecond = 10f;  // 열차 진행 속도 (거리=시간×속도, placeholder 튜닝)

    bool _running = true;

    void Update()
    {
        if (!_running) return;
        _gameState.distanceScore += _metersPerSecond * Time.deltaTime;  // ScoreFormat이 floor→정수 m 표시
    }

    // 사망/카운트다운 중 스코어 부당 증가 방지 — GameSceneManager가 spawner와 함께 제어
    public void Pause()  => _running = false;
    public void Resume() => _running = true;
}
