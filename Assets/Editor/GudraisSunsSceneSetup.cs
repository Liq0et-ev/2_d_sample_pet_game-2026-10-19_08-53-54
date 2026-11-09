using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// Šis redaktora skripts izveido spēles ainas (tikai kamera un gaisma; pati spēle uzbūvējas, kad nospiež Play):
//   Assets/Scenes/GalvenaIzvelne.unity  - galvenā izvēlne (sākuma aina)
//   Assets/Scenes/GudraisSuns.unity     - 2D versija
//   Assets/Scenes/GudraisSuns3D.unity   - 3D versija
// Ainas tiek pievienotas Build Settings sarakstam (galvenā izvēlne ir pirmā, jo spēle sākas ar to).
[InitializeOnLoad]
public static class GudraisSunsSceneSetup
{
    const string AinaIzvelne = "Assets/Scenes/GalvenaIzvelne.unity";
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
        // Izvēlnes ainu veidojam pēdējo, lai tā paliktu atvērta redaktorā
        if (File.Exists(Aina2D) == false)
        {
            IzveidotAinu(Aina2D, true);
        }
        if (File.Exists(Aina3D) == false)
        {
            IzveidotAinu(Aina3D, true);
        }
        if (File.Exists(AinaIzvelne) == false)
        {
            IzveidotAinu(AinaIzvelne, true);
        }
        SakartotBuildSettings();
    }

    // Izvēlnes punkti: Gudrais suns -> Open ...
    [MenuItem("Gudrais suns/Open main menu scene")]
    static void AtvertIzvelni()
    {
        AtvertAinu(AinaIzvelne);
    }

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
        SakartotBuildSettings();
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
    }

    // Build Settings saraksts: 0 = galvenā izvēlne, 1 = 2D, 2 = 3D, pēc tam citas ainas (ja ir)
    static void SakartotBuildSettings()
    {
        string[] musuAinas = { AinaIzvelne, Aina2D, Aina3D };
        List<EditorBuildSettingsScene> jaunaisSaraksts = new List<EditorBuildSettingsScene>();

        for (int i = 0; i < musuAinas.Length; i++)
        {
            if (File.Exists(musuAinas[i]))
            {
                jaunaisSaraksts.Add(new EditorBuildSettingsScene(musuAinas[i], true));
            }
        }

        // Citas ainas, kas jau bija sarakstā, paliek aiz mūsējām
        EditorBuildSettingsScene[] vecais = EditorBuildSettings.scenes;
        for (int i = 0; i < vecais.Length; i++)
        {
            bool irMusuAina = vecais[i].path == AinaIzvelne || vecais[i].path == Aina2D || vecais[i].path == Aina3D;
            if (irMusuAina == false)
            {
                jaunaisSaraksts.Add(vecais[i]);
            }
        }
        EditorBuildSettings.scenes = jaunaisSaraksts.ToArray();
    }
}
