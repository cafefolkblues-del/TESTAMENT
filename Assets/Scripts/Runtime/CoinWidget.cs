using TMPro;
using UnityEngine;

// 코인(잔여 컨티뉴=목숨) 위젯 — GameState.coinCount 표시.
// 변화가 드물어(컨티뉴 시에만) polling으로 충분. UI 기능 분리: coinCount만 구독.
public class CoinWidget : MonoBehaviour
{
    [SerializeField] GameState _gameState;
    [SerializeField] TMP_Text  _text;

    void Update() => _text.text = "× " + _gameState.coinCount;   // "× 3"
}
