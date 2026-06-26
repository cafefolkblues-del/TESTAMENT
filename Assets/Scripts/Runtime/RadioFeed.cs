using System;

// 무전 메시지 단일 진입점 — 발신측(EnemySpawner 경고, DialogueManager 대사)과 수신 위젯(RadioWidget) 분리.
// speaker: 발화자 초상화용(없으면 null — 거대형 경고 등 시스템 메시지).
public static class RadioFeed
{
    public static event Action<string, CharacterData> OnMessage;

    public static void Post(string message, CharacterData speaker = null) => OnMessage?.Invoke(message, speaker);
}
