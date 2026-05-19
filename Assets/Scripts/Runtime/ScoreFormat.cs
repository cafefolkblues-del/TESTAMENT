using UnityEngine;

// 점수 표시 규칙을 ResultUI에서 분리 — 추후 자릿수 채우기/콤마/단위(m, km) 변경 시 여기만 수정.
public static class ScoreFormat
{
    // Mathf.FloorToInt 사용 이유 — 점수는 거리 누적(float). 표시에서 소수점 잘라내고 정수로 노출.
    public static string Display(float score) => Mathf.FloorToInt(score).ToString();
}
