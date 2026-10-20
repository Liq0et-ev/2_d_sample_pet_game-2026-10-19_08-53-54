using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// Creates Assets/Scenes/GudraisSuns.unity (camera only; the game builds itself on Play) and adds it to Build Settings.
[InitializeOnLoad]
public static class GudraisSunsSceneSetup
{
    const string ScenePath = "Assets/Scenes/GudraisSuns.unity";

    static GudraisSunsSceneSetup()
    {
        EditorApplication.delayCall += () =>
        {
            if (File.Exists(ScenePath) || EditorApplication.isPlayingOrWillChangePlaymode) return;
            Create(true);
        };
    }

    [UnityEditor.MenuItem("Gudrais suns/Open game scene")]
    static void Open()
    {
        if (!File.Exists(ScenePath)) Create(false);
        else if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath);
    }

    static void Create(bool onlyIfClean)
    {
        if (onlyIfClean && SceneManager.GetActiveScene().isDirty) return;
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, ScenePath);
        var list = EditorBuildSettings.scenes.ToList();
        if (!list.Any(s => s.path == ScenePath))
        {
            list.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
