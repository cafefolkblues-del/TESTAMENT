using System.Collections.Generic;
using UnityEngine.SceneManagement;

// 씬 이름 마법 문자열 4곳 분산 → SceneFlow 한 곳으로 일원화
// 빌드세팅의 파일명과 정확히 일치해야 함 (대소문자/공백 포함)
public enum SceneId
{
    Start,
    // TODO(Intro): GDD상 Start→Intro→Tutorial. Intro 씬 + 스킵 기능(아무 입력 시 즉시 Tutorial로) 필요 — 미구현.
    Tutorial,
    CharacterSelect,
    Game,
}

public static class SceneFlow
{
    // Dictionary 사용 이유 — enum→파일명 매핑이 명시적이고,
    // 누락 시 KeyNotFoundException으로 호출 시점에 즉시 드러남 (switch fall-through로 빈값 나가는 사고 방지)
    static readonly Dictionary<SceneId, string> _names = new()
    {
        { SceneId.Start,           "Start" },
        { SceneId.Tutorial,        "Tutorial" },
        { SceneId.CharacterSelect, "Character selection" }, // 파일명에 공백 + lowercase s 주의
        { SceneId.Game,            "Game" },
    };

    // 씬 전환 단일 진입점 — 각 매니저에서 SceneManager.LoadScene 직접 호출 분산 방지
    public static void Load(SceneId id) => SceneManager.LoadScene(_names[id]);
}
