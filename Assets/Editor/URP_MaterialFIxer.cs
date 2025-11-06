using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class URP_MaterialFixer : Editor
{
    [MenuItem("Tools/URP/Replace Missing or Pink Materials (All Prefabs)")]
    public static void FixAllPrefabs()
    {
        // Choose fallback shader and material
        Shader fallbackShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
        if (fallbackShader == null)
        {
            EditorUtility.DisplayDialog("URP Fixer", "Fallback shader not found: URP/2D/Sprite-Lit-Default. Make sure URP 2D is installed.", "OK");
            return;
        }

        // Try to find or create a fallback material
        string[] matGUIDs = AssetDatabase.FindAssets("t:Material Fallback_SpriteLit");
        Material fallbackMat = null;
        if (matGUIDs.Length > 0)
            fallbackMat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(matGUIDs[0]));
        if (fallbackMat == null)
        {
            fallbackMat = new Material(fallbackShader) { name = "Fallback_SpriteLit" };
            string path = "Assets/Fallback_SpriteLit.mat";
            AssetDatabase.CreateAsset(fallbackMat, path);
            AssetDatabase.SaveAssets();
        }

        int prefabCount = 0, rendererCount = 0, replacedCount = 0;
        var prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
        Shader errorShader = Shader.Find("Hidden/InternalErrorShader");

        foreach (var guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            bool changed = false;
            prefabCount++;

            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            rendererCount += renderers.Length;

            foreach (var r in renderers)
            {
                // Shared materials so we edit the prefab asset, not instances
                var mats = r.sharedMaterials;
                bool rChanged = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = mats[i];
                    bool isMissing = m == null;
                    bool isError = (!isMissing && m.shader == errorShader);

                    if (isMissing || isError)
                    {
                        mats[i] = fallbackMat;
                        replacedCount++;
                        rChanged = true;
                    }
                }
                if (rChanged)
                {
                    Undo.RecordObject(r, "URP Material Fix");
                    r.sharedMaterials = mats;
                    EditorUtility.SetDirty(r);
                    changed = true;
                }
            }

            if (changed)
                PrefabUtility.SavePrefabAsset(prefab);
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("URP Fixer",
            $"Checked {prefabCount} prefabs, {rendererCount} renderers.\nReplaced {replacedCount} material slots with Fallback_SpriteLit.",
            "Nice");
    }
}
