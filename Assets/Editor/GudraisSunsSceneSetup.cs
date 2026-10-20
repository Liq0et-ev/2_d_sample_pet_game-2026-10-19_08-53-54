using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// Šis redaktora skripts izveido ainu Assets/Scenes/GudraisSuns.unity (tikai kamera un gaisma;
// pati spēle uzbūvējas, kad nospiež Play) un pievieno to Build Settings sarakstam.
[InitializeOnLoad]
public static class GudraisSunsSceneSetup
{
    const string AinasCels = "Assets/Scenes/GudraisSuns.unity";

    // Šis konstruktors tiek izsaukts automātiski, kad Unity ielādē skriptus
    static GudraisSunsSceneSetup()
    {
        EditorApplication.delayCall += PiemektAinuJaVajag;
    }

    static void PiemektAinuJaVajag()
    {
        if (File.Exists(AinasCels))
        {
            return;
        }
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }
        IzveidotAinu(true);
    }

    // Izvēlnes punkts: Gudrais suns -> Open game scene
    [MenuItem("Gudrais suns/Open game scene")]
    static void AtvertAinu()
    {
        if (File.Exists(AinasCels) == false)
        {
            IzveidotAinu(false);
        }
        else if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(AinasCels);
        }
    }

    static void IzveidotAinu(bool tikaiJaTira)
    {
        // Nepārrakstām ainu, kurā ir nesaglabātas izmaiņas
        if (tikaiJaTira && SceneManager.GetActiveScene().isDirty)
        {
            return;
        }

        Scene aina = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EditorSceneManager.SaveScene(aina, AinasCels);

        // Pievienojam ainu Build Settings sarakstam, ja tur vēl nav
        List<EditorBuildSettingsScene> ainas = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool jauIrSaraksta = false;
        for (int i = 0; i < ainas.Count; i++)
        {
            if (ainas[i].path == AinasCels)
            {
                jauIrSaraksta = true;
            }
        }
        if (jauIrSaraksta == false)
        {
            ainas.Insert(0, new EditorBuildSettingsScene(AinasCels, true));
            EditorBuildSettings.scenes = ainas.ToArray();
        }
    }
}
