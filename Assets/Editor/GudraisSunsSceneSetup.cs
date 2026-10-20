using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// Šis redaktora skripts izveido spēles ainas (tikai kamera un gaisma; pati spēle uzbūvējas, kad nospiež Play):
//   Assets/Scenes/GudraisSuns.unity     - 2D versija
//   Assets/Scenes/GudraisSuns3D.unity   - 3D versija
// Ainas tiek pievienotas arī Build Settings sarakstam.
[InitializeOnLoad]
public static class GudraisSunsSceneSetup
{
    const string Aina2D = "Assets/Scenes/GudraisSuns.unity";
    const string Aina3D = "Assets/Scenes/GudraisSuns3D.unity";

    // Šis konstruktors tiek izsaukts automātiski, kad Unity ielādē skriptus
    static GudraisSunsSceneSetup()
    {
        EditorApplication.delayCall += PiemektAinasJaVajag;
    }

    static void PiemektAinasJaVajag()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }
        if (File.Exists(Aina2D) == false)
        {
            IzveidotAinu(Aina2D, true);
        }
        if (File.Exists(Aina3D) == false)
        {
            IzveidotAinu(Aina3D, true);
        }
    }

    // Izvēlnes punkti: Gudrais suns -> Open game scene (2D / 3D)
    [MenuItem("Gudrais suns/Open game scene (2D)")]
    static void AtvertAinu2D()
    {
        AtvertAinu(Aina2D);
    }

    [MenuItem("Gudrais suns/Open game scene (3D)")]
    static void AtvertAinu3D()
    {
        AtvertAinu(Aina3D);
    }

    static void AtvertAinu(string cels)
    {
        if (File.Exists(cels) == false)
        {
            IzveidotAinu(cels, false);
        }
        else if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(cels);
        }
    }

    static void IzveidotAinu(string cels, bool tikaiJaTira)
    {
        // Nepārrakstām ainu, kurā ir nesaglabātas izmaiņas
        if (tikaiJaTira && SceneManager.GetActiveScene().isDirty)
        {
            return;
        }

        Scene aina = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EditorSceneManager.SaveScene(aina, cels);

        // Pievienojam ainu Build Settings sarakstam, ja tur vēl nav
        List<EditorBuildSettingsScene> ainas = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool jauIrSaraksta = false;
        for (int i = 0; i < ainas.Count; i++)
        {
            if (ainas[i].path == cels)
            {
                jauIrSaraksta = true;
            }
        }
        if (jauIrSaraksta == false)
        {
            ainas.Insert(0, new EditorBuildSettingsScene(cels, true));
            EditorBuildSettings.scenes = ainas.ToArray();
        }
    }
}
