using UnityEditor;

public static class BuildSettingsSetup
{
    [MenuItem("TESTAMENT/Setup Build Settings")]
    public static void SetupScenes()
    {
        var scenes = new[]
        {
            "Assets/Scenes/Start.unity",
            "Assets/Scenes/Tutorial.unity",
            "Assets/Scenes/Character selection.unity",
            "Assets/Scenes/Game.unity",
            "Assets/Scenes/Result.unity",
        };

        var builds = new EditorBuildSettingsScene[scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
            builds[i] = new EditorBuildSettingsScene(scenes[i], true);

        EditorBuildSettings.scenes = builds;
        UnityEngine.Debug.Log("Build Settings 등록 완료");
    }
}
