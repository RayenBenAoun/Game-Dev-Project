// Assets/Editor/FixShadersURP.cs
using UnityEngine;
using UnityEditor;
using System.Linq;

public static class FixShadersURP
{
    // Common URP shader paths
    const string URP_Lit = "Universal Render Pipeline/Lit";
    const string URP_Unlit = "Universal Render Pipeline/Unlit";
    const string URP_2D_SpriteLit = "Universal Render Pipeline/2D/Sprite-Lit-Default";
    const string URP_2D_SpriteUnlit = "Universal Render Pipeline/2D/Sprite-Unlit-Default";
    const string URP_Particles_Unlit = "Universal Render Pipeline/Particles/Unlit";
    const string TMP_URP_SDF = "TextMeshPro/SRP/TMP_SDF-URP";
    const string TMP_URP_SDF_Mobile = "TextMeshPro/SRP/TMP_SDF-Mobile-URP";

    static bool LooksLikeTMP(Material m)
        => m != null && (m.name.ToLower().Contains("tmp") || m.shader != null && m.shader.name.Contains("TextMeshPro"));

    static bool LooksLikeSprite(Material m)
        => m != null && (m.name.ToLower().Contains("sprite") || (m.shader != null && m.shader.name.ToLower().Contains("sprite")));

    static bool LooksLikeParticle(Material m)
        => m != null && (m.name.ToLower().Contains("particle") || (m.shader != null && m.shader.name.ToLower().Contains("particle")));

    static bool IsLegacyOrHDRP(Material m)
        => m.shader == null ||
           m.shader.name.StartsWith("HDRP") ||
           m.shader.name.StartsWith("HDRenderPipeline") ||
           m.shader.name.StartsWith("Legacy Shaders") ||
           m.shader.name.StartsWith("Standard") ||
           m.shader.name.Contains("Built-in") ||
           m.shader.name.Contains("TMP_SDF-HDRP");

    [MenuItem("Tools/URP Utilities/Audit Missing/Legacy Shaders")]
    public static void Audit()
    {
        var guids = AssetDatabase.FindAssets("t:Material");
        int count = 0;
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;

            if (mat.shader == null)
            {
                Debug.LogWarning($"[Missing Shader] {path}");
                count++;
            }
            else if (IsLegacyOrHDRP(mat))
            {
                Debug.LogWarning($"[Legacy/HDRP/Built-In] {path} -> {mat.shader.name}");
                count++;
            }
        }
        Debug.Log($"Audit complete. {count} materials need attention.");
    }

    [MenuItem("Tools/URP Utilities/Fix Missing/Legacy Shaders (Assign URP)")]
    public static void Fix()
    {
        var s_URP_Lit = Shader.Find(URP_Lit);
        var s_URP_Unlit = Shader.Find(URP_Unlit);
        var s_URP_SpriteLit = Shader.Find(URP_2D_SpriteLit);
        var s_URP_SpriteUnlit = Shader.Find(URP_2D_SpriteUnlit);
        var s_URP_Particles_Unlit = Shader.Find(URP_Particles_Unlit);
        var s_TMP_URP_SDF = Shader.Find(TMP_URP_SDF);
        var s_TMP_URP_SDF_Mobile = Shader.Find(TMP_URP_SDF_Mobile);

        var guids = AssetDatabase.FindAssets("t:Material");
        int fixedCount = 0;

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) continue;

                if (!IsLegacyOrHDRP(mat)) continue;

                Shader target = null;

                if (LooksLikeTMP(mat))
                {
                    target = s_TMP_URP_SDF != null ? s_TMP_URP_SDF : s_TMP_URP_SDF_Mobile;
                }
                else if (LooksLikeSprite(mat))
                {
                    target = s_URP_SpriteLit != null ? s_URP_SpriteLit : s_URP_SpriteUnlit;
                }
                else if (LooksLikeParticle(mat))
                {
                    target = s_URP_Particles_Unlit != null ? s_URP_Particles_Unlit : s_URP_Unlit;
                }
                else
                {
                    // General fallback (works for most)
                    target = s_URP_Lit != null ? s_URP_Lit : s_URP_Unlit;
                }

                if (target != null)
                {
                    Undo.RecordObject(mat, "Fix Shader to URP");
                    mat.shader = target;
                    EditorUtility.SetDirty(mat);
                    fixedCount++;
                    // Optional: try to preserve main texture if property exists
                    // (URP Lit uses _BaseMap, Sprite uses _MainTex)
                    if (mat.HasProperty("_BaseMap") && mat.HasProperty("_MainTex"))
                        mat.SetTexture("_BaseMap", mat.GetTexture("_MainTex"));
                }
                else
                {
                    Debug.LogWarning($"Could not find target URP shader for {path}");
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
        }

        Debug.Log($"Fix complete. {fixedCount} materials updated to URP.");
        // Reimport to refresh pink in scene/game view
        AssetDatabase.Refresh();
    }
}
