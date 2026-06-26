using System;

// 무전 메시지 단일 진입점 — 발신측(EnemySpawner 거대형 경고 등)과 수신 위젯(RadioWidget)을 분리.
// 향후 확장: 캐릭터별 상호 대사 / 동료 사망 통보는 Post 오버로드(speaker 추가)로 이 채널 재사용.
public static class RadioFeed
{
    public static event Action<string> OnMessage;

    public static void Post(string message) => OnMessage?.Invoke(message);
}
