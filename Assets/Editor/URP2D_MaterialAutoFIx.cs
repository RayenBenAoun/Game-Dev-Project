#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class URP2D_MaterialAutoFix
{
    // Map some common, safe URP fallback shaders
    static Shader GetURPSpriteLit() => Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
    static Shader GetURPSpriteUnlit() => Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
    static Shader GetURPUnlit() => Shader.Find("Universal Render Pipeline/Unlit");
    static Shader GetURPParticlesUnlit() => Shader.Find("Universal Render Pipeline/Particles/Unlit");

    static bool IsBrokenShader(Shader s) =>
        s == null || s.name == "Hidden/InternalErrorShader";

    static bool LooksLikeCainos(string shaderName) =>
        !string.IsNullOrEmpty(shaderName) &&
        (shaderName.Contains("Cainos") || shaderName.Contains("ASE") || shaderName.Contains("Top Down Pixel"));

    [MenuItem("Tools/URP 2D/Auto-Fix Materials")]
    public static void AutoFixMaterials()
    {
        var matGuids = AssetDatabase.FindAssets("t:Material");
        int fixedCount = 0, scanned = 0;

        foreach (var guid in matGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;
            scanned++;

            var shader = mat.shader;
            var shaderName = shader ? shader.name : "";

            // Decide if this material needs fixing
            bool needsFix = IsBrokenShader(shader) || LooksLikeCainos(shaderName);

            if (!needsFix)
                continue;

            // Choose fallback based on material name keywords
            string name = mat.name.ToLowerInvariant();
            Shader fallback = null;

            // Order matters: pick the most specific bucket first
            if (name.Contains("water"))
            {
                // Cainos water uses a custom renderer feature not compatible with Unity 6 URP out of the box.
                // Use a simple Unlit as a temporary visual until you add a compatible effect.
                fallback = GetURPUnlit();
            }
            else if (name.Contains("smoke") || name.Contains("fx") || name.Contains("vfx") || name.Contains("particle"))
            {
                fallback = GetURPParticlesUnlit() ?? GetURPUnlit();
            }
            else if (name.Contains("shadow"))
            {
                // Sprite-Unlit is good for masks; you can change blend to Multiply later.
                fallback = GetURPSpriteUnlit();
            }
            else if (name.Contains("tile") || name.Contains("floor") || name.Contains("ground") ||
                     name.Contains("prop") || name.Contains("sprite"))
            {
                fallback = GetURPSpriteLit();
            }
            else
            {
                // Generic sprite fallback
                fallback = GetURPSpriteLit() ?? GetURPUnlit();
            }

            if (fallback == null)
            {
                Debug.LogWarning($"[URP2D AutoFix] Could not find a URP shader for '{mat.name}'. Skipping.");
                continue;
            }

            Undo.RecordObject(mat, "Assign URP Fallback Shader");
            mat.shader = fallback;

            // Ensure proper rendering for transparency-ish materials
            if (fallback == GetURPUnlit() || fallback == GetURPParticlesUnlit() || fallback == GetURPSpriteUnlit() || fallback == GetURPSpriteLit())
            {
                // Some materials had opaque set; force transparent pipeline if texture/alpha is used
                if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f); // 1 = Transparent
                if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);   // Alpha
                if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);
            }

            EditorUtility.SetDirty(mat);
            fixedCount++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[URP2D AutoFix] Scanned {scanned} materials. Fixed {fixedCount}.");
    }
}
#endif
