// Place this file under any Editor/ folder in your Unity project.
// Then use the menu: Tools > Skybox > Generate Presets
// It will create 4 material files next to the shader.

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public static class SkyboxPresetGenerator
{
    private const string ShaderName = "Skybox/Procedural Advanced URP";

    [MenuItem("Tools/Skybox/Generate Presets")]
    public static void Generate()
    {
        Shader shader = Shader.Find(ShaderName);
        if (shader == null)
        {
            EditorUtility.DisplayDialog("Error",
                $"Shader \"{ShaderName}\" not found.\nMake sure ProceduralSkyboxURP.shader is imported.", "OK");
            return;
        }

        string folder = EditorUtility.OpenFolderPanel("Save Preset Materials", "Assets", "");
        if (string.IsNullOrEmpty(folder)) return;

        // Convert absolute path to Assets-relative
        if (folder.StartsWith(Application.dataPath))
            folder = "Assets" + folder.Substring(Application.dataPath.Length);
        else
        {
            EditorUtility.DisplayDialog("Error", "Please choose a folder inside your Assets.", "OK");
            return;
        }

        CreateDawn(shader, folder);
        CreateNoon(shader, folder);
        CreateDusk(shader, folder);
        CreateNight(shader, folder);

        AssetDatabase.Refresh();
        Debug.Log($"[Skybox] 4 preset materials created in {folder}");
    }

    // ============================================================
    //  Dawn  晨曦
    // ============================================================
    static void CreateDawn(Shader s, string folder)
    {
        Material m = new Material(s);
        m.SetColor("_ZenithColor",   new Color(0.25f, 0.35f, 0.62f));
        m.SetColor("_HorizonColor",  new Color(0.95f, 0.55f, 0.30f));
        m.SetColor("_GroundColor",   new Color(0.18f, 0.14f, 0.12f));
        m.SetFloat("_GradientIntensity", 1.2f);

        m.SetFloat("_HorizonOffset",   0.0f);
        m.SetFloat("_HorizonBlur",     0.15f);
        m.SetFloat("_HorizonGradient", 0.35f);

        m.SetVector("_SunDir",       new Vector4(1.0f, 0.12f, 0.3f, 0));
        m.SetFloat("_SunSize",       0.045f);
        m.SetFloat("_SunIntensity",  1.8f);
        m.SetColor("_SunColor",      new Color(1.0f, 0.6f, 0.25f));
        m.SetFloat("_SunHaloSize",   0.22f);
        m.SetFloat("_SunHaloIntensity", 0.9f);

        m.SetFloat("_Exposure",      1.05f);

        Save(m, folder, "Skybox_Dawn");
    }

    // ============================================================
    //  Noon  正午
    // ============================================================
    static void CreateNoon(Shader s, string folder)
    {
        Material m = new Material(s);
        m.SetColor("_ZenithColor",   new Color(0.12f, 0.38f, 0.85f));
        m.SetColor("_HorizonColor",  new Color(0.60f, 0.75f, 0.90f));
        m.SetColor("_GroundColor",   new Color(0.30f, 0.26f, 0.20f));
        m.SetFloat("_GradientIntensity", 1.5f);

        m.SetFloat("_HorizonOffset",   0.0f);
        m.SetFloat("_HorizonBlur",     0.10f);
        m.SetFloat("_HorizonGradient", 0.28f);

        m.SetVector("_SunDir",       new Vector4(0.0f, 0.95f, 0.2f, 0));
        m.SetFloat("_SunSize",       0.035f);
        m.SetFloat("_SunIntensity",  2.0f);
        m.SetColor("_SunColor",      new Color(1.0f, 0.98f, 0.92f));
        m.SetFloat("_SunHaloSize",   0.10f);
        m.SetFloat("_SunHaloIntensity", 0.35f);

        m.SetFloat("_Exposure",      1.2f);

        Save(m, folder, "Skybox_Noon");
    }

    // ============================================================
    //  Dusk  傍晚
    // ============================================================
    static void CreateDusk(Shader s, string folder)
    {
        Material m = new Material(s);
        m.SetColor("_ZenithColor",   new Color(0.15f, 0.12f, 0.35f));
        m.SetColor("_HorizonColor",  new Color(0.92f, 0.40f, 0.18f));
        m.SetColor("_GroundColor",   new Color(0.15f, 0.10f, 0.08f));
        m.SetFloat("_GradientIntensity", 1.0f);

        m.SetFloat("_HorizonOffset",   0.0f);
        m.SetFloat("_HorizonBlur",     0.18f);
        m.SetFloat("_HorizonGradient", 0.40f);

        m.SetVector("_SunDir",       new Vector4(-0.8f, 0.08f, 0.5f, 0));
        m.SetFloat("_SunSize",       0.055f);
        m.SetFloat("_SunIntensity",  2.2f);
        m.SetColor("_SunColor",      new Color(1.0f, 0.42f, 0.10f));
        m.SetFloat("_SunHaloSize",   0.28f);
        m.SetFloat("_SunHaloIntensity", 1.1f);

        m.SetFloat("_Exposure",      0.95f);

        Save(m, folder, "Skybox_Dusk");
    }

    // ============================================================
    //  Night  黑夜
    // ============================================================
    static void CreateNight(Shader s, string folder)
    {
        Material m = new Material(s);
        m.SetColor("_ZenithColor",   new Color(0.01f, 0.01f, 0.05f));
        m.SetColor("_HorizonColor",  new Color(0.04f, 0.04f, 0.10f));
        m.SetColor("_GroundColor",   new Color(0.02f, 0.02f, 0.03f));
        m.SetFloat("_GradientIntensity", 1.8f);

        m.SetFloat("_HorizonOffset",   0.0f);
        m.SetFloat("_HorizonBlur",     0.08f);
        m.SetFloat("_HorizonGradient", 0.25f);

        // Moon-like light source
        m.SetVector("_SunDir",       new Vector4(0.3f, 0.6f, -0.7f, 0));
        m.SetFloat("_SunSize",       0.025f);
        m.SetFloat("_SunIntensity",  0.4f);
        m.SetColor("_SunColor",      new Color(0.70f, 0.75f, 0.90f));
        m.SetFloat("_SunHaloSize",   0.08f);
        m.SetFloat("_SunHaloIntensity", 0.15f);

        m.SetFloat("_Exposure",      0.55f);

        Save(m, folder, "Skybox_Night");
    }

    static void Save(Material m, string folder, string name)
    {
        string path = Path.Combine(folder, name + ".mat");
        AssetDatabase.CreateAsset(m, path);
    }
}
#endif
