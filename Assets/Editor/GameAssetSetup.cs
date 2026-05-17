using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameAssetSetup
{
    [MenuItem("TESTAMENT/Setup Game Assets")]
    public static void SetupGameAssets()
    {
        EnsureFolder("Assets", "Data");
        EnsureFolder("Assets/Data", "Characters");
        EnsureFolder("Assets/Data", "Companions");

        var hell = GetOrCreate<CharacterData>("Assets/Data/Characters/Hell.asset");
        var mill = GetOrCreate<CharacterData>("Assets/Data/Characters/Mill.asset");
        var lucy = GetOrCreate<CharacterData>("Assets/Data/Characters/Lucy.asset");
        var lil  = GetOrCreate<CharacterData>("Assets/Data/Characters/Lil.asset");
        var bill = GetOrCreate<CharacterData>("Assets/Data/Characters/Bill.asset");

        SetCharacter(hell, "헬",  AbilityType.MachineGunExplosive, AbilityType.ManualAim);
        SetCharacter(mill, "밀",  AbilityType.RevolverSingle,       AbilityType.ShotgunModeHold);
        SetCharacter(lucy, "루시", AbilityType.AutoWeaponSwap,       AbilityType.ManualWeaponSwap);
        SetCharacter(lil,  "릴",  AbilityType.MeleeAutoHit,         AbilityType.GuardAndCharge);
        SetCharacter(bill, "빌",  AbilityType.HackingTool,          AbilityType.EnemyHack);

        var c0 = GetOrCreate<CompanionData>("Assets/Data/Companions/Companion0.asset");
        var c1 = GetOrCreate<CompanionData>("Assets/Data/Companions/Companion1.asset");
        var c2 = GetOrCreate<CompanionData>("Assets/Data/Companions/Companion2.asset");
        var c3 = GetOrCreate<CompanionData>("Assets/Data/Companions/Companion3.asset");

        var roster = GetOrCreate<CharacterRoster>("Assets/Data/CharacterRoster.asset");
        {
            var so = new SerializedObject(roster);
            var arr = so.FindProperty("characters");
            arr.arraySize = 5;
            arr.GetArrayElementAtIndex(0).objectReferenceValue = hell;
            arr.GetArrayElementAtIndex(1).objectReferenceValue = mill;
            arr.GetArrayElementAtIndex(2).objectReferenceValue = lucy;
            arr.GetArrayElementAtIndex(3).objectReferenceValue = lil;
            arr.GetArrayElementAtIndex(4).objectReferenceValue = bill;
            so.ApplyModifiedProperties();
        }

        var selectedChar = GetOrCreate<SelectedCharacter>("Assets/Data/SelectedCharacter.asset");

        var gameState = GetOrCreate<GameState>("Assets/Data/GameState.asset");
        {
            var so = new SerializedObject(gameState);
            var list = so.FindProperty("companions");
            list.arraySize = 4;
            list.GetArrayElementAtIndex(0).objectReferenceValue = c0;
            list.GetArrayElementAtIndex(1).objectReferenceValue = c1;
            list.GetArrayElementAtIndex(2).objectReferenceValue = c2;
            list.GetArrayElementAtIndex(3).objectReferenceValue = c3;
            so.ApplyModifiedProperties();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        WireGameScene(selectedChar, gameState, roster);

        Debug.Log("[TESTAMENT] Setup complete.");
    }

    static void WireGameScene(SelectedCharacter selectedChar, GameState gameState, CharacterRoster roster)
    {
        // --- GameManager ---
        var gameManagerGO = GetOrCreateGO("GameManager");
        var gsm           = GetOrAddComponent<GameSceneManager>(gameManagerGO);

        // --- EnemySpawner ---
        var spawnerGO = GetOrCreateGO("EnemySpawner");
        var spawner   = GetOrAddComponent<EnemySpawner>(spawnerGO);

        var spawnPointGO = GetOrCreateGO("SpawnPoint");
        spawnPointGO.transform.position = new Vector3(20f, 0f, 0f);

        // --- ResultUI (Canvas 하위 UI 계층) ---
        // TMP_Text 사용 중 — 아트 에셋 준비 시 Image 기반 UI로 교체 가능
        var canvasGO  = GameObject.Find("Canvas");
        var resultUIGO = GetOrCreateChildUIGO(canvasGO, "ResultUI");
        StretchFull(resultUIGO.GetComponent<RectTransform>());
        var resultUICG = GetOrAddComponent<CanvasGroup>(resultUIGO);
        var resultUI   = GetOrAddComponent<ResultUI>(resultUIGO);

        var scoreTextGO = GetOrCreateChildUIGO(resultUIGO, "ScoreText");
        var scoreText   = GetOrAddComponent<TextMeshProUGUI>(scoreTextGO);
        scoreText.text  = "0";

        var coinTextGO = GetOrCreateChildUIGO(resultUIGO, "CoinText");
        var coinText   = GetOrAddComponent<TextMeshProUGUI>(coinTextGO);
        coinText.text  = "3";

        var countdownGO = GetOrCreateChildUIGO(resultUIGO, "Countdown");
        var countText   = GetOrAddComponent<TextMeshProUGUI>(countdownGO);
        countText.text  = "10";
        var countdown   = GetOrAddComponent<ContinueCountdown>(countdownGO);

        // FadeOverlay (기존 오브젝트)
        CanvasGroup fadeOverlay = null;
        var fadeGO = GameObject.Find("FadeOverlay");
        if (fadeGO != null) fadeOverlay = fadeGO.GetComponent<CanvasGroup>();

        // Player refs
        var playerGO         = GameObject.Find("Player");
        PlayerController playerController = null;
        PlayerHealth     playerHealth     = null;
        if (playerGO != null)
        {
            playerController = playerGO.GetComponent<PlayerController>();
            playerHealth     = playerGO.GetComponent<PlayerHealth>();
        }

        // Wire GameSceneManager
        {
            var so = new SerializedObject(gsm);
            so.FindProperty("_selectedCharacter").objectReferenceValue = selectedChar;
            so.FindProperty("_gameState").objectReferenceValue         = gameState;
            so.FindProperty("_roster").objectReferenceValue            = roster;
            so.FindProperty("_spawner").objectReferenceValue           = spawner;
            so.FindProperty("_resultUI").objectReferenceValue          = resultUI;
            so.FindProperty("_playerController").objectReferenceValue  = playerController;
            so.FindProperty("_playerHealth").objectReferenceValue      = playerHealth;
            so.ApplyModifiedProperties();
        }

        // Wire EnemySpawner
        {
            var so = new SerializedObject(spawner);
            so.FindProperty("_gameState").objectReferenceValue  = gameState;
            so.FindProperty("_spawnPoint").objectReferenceValue = spawnPointGO.transform;
            so.ApplyModifiedProperties();
        }

        // Wire ResultUI
        {
            var so = new SerializedObject(resultUI);
            so.FindProperty("_panel").objectReferenceValue       = resultUICG;
            so.FindProperty("_scoreText").objectReferenceValue   = scoreText;
            so.FindProperty("_coinText").objectReferenceValue    = coinText;
            so.FindProperty("_countdown").objectReferenceValue   = countdown;
            so.FindProperty("_fadeOverlay").objectReferenceValue = fadeOverlay;
            so.ApplyModifiedProperties();
        }

        // Wire ContinueCountdown
        {
            var so = new SerializedObject(countdown);
            so.FindProperty("_countText").objectReferenceValue = countText;
            so.ApplyModifiedProperties();
        }

        // Wire PlayerCharacterLoader
        if (playerGO != null)
        {
            var loader = playerGO.GetComponent<PlayerCharacterLoader>();
            if (loader != null)
            {
                var so = new SerializedObject(loader);
                so.FindProperty("_selectedCharacter").objectReferenceValue = selectedChar;
                so.FindProperty("_spriteRenderer").objectReferenceValue    = playerGO.GetComponent<SpriteRenderer>();
                so.ApplyModifiedProperties();
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }

    static GameObject GetOrCreateGO(string name)
    {
        var go = GameObject.Find(name);
        if (go == null) go = new GameObject(name);
        return go;
    }

    static GameObject GetOrCreateChildUIGO(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        if (t != null) return t.gameObject;
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        if (c == null) c = go.AddComponent<T>();
        return c;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void SetCharacter(CharacterData data, string charName, AbilityType left, AbilityType right, float speed = 5f, float jump = 10f)
    {
        var so = new SerializedObject(data);
        so.FindProperty("characterName").stringValue   = charName;
        so.FindProperty("moveSpeed").floatValue        = speed;
        so.FindProperty("jumpForce").floatValue        = jump;
        so.FindProperty("leftAbility").enumValueIndex  = (int)left;
        so.FindProperty("rightAbility").enumValueIndex = (int)right;
        so.ApplyModifiedProperties();
    }

    static T GetOrCreate<T>(string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;
        var instance = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(instance, path);
        return instance;
    }

    static void EnsureFolder(string parent, string name)
    {
        string full = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(full))
            AssetDatabase.CreateFolder(parent, name);
    }
}
