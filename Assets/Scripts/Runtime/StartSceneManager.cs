using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneManager : MonoBehaviour
{
    [SerializeField] string nextSceneName = "Tutorial";

    bool _inputEnabled = false;

    void Start()
    {
        // 씬 로드 직후 오입력 방지
        Invoke(nameof(EnableInput), 0.5f);
    }

    void EnableInput() => _inputEnabled = true;

    void Update()
    {
        if (!_inputEnabled) return;
        if (Input.anyKeyDown)
            SceneManager.LoadScene(nextSceneName);
    }
}
