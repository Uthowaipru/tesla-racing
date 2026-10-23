#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace VehicleSystem.EditorTools
{
    /// <summary>Tools > Vehicle > Create Scoring Rules From Match Manager: saves the numbers currently on the scene's MatchManager as an asset and assigns it.</summary>
    public static class ScoringRulesTools
    {
        [MenuItem("Tools/Vehicle/Create Scoring Rules From Match Manager")]
        static void CreateFromMatchManager()
        {
            MatchManager manager = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<MatchManager>() : null;
            if (manager == null) manager = Object.FindFirstObjectByType<MatchManager>();
            if (manager == null)
            {
                EditorUtility.DisplayDialog("Scoring Rules", "No MatchManager found in the open scene.", "OK");
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject("Save Scoring Rules", "ScoringRules", "asset",
                "Choose where to save the scoring rules asset.");
            if (string.IsNullOrEmpty(path)) return;

            var rules = ScriptableObject.CreateInstance<ScoringRules>();
            rules.CaptureFrom(manager);
            AssetDatabase.CreateAsset(rules, path);
            AssetDatabase.SaveAssets();

            Undo.RecordObject(manager, "Assign Scoring Rules");
            manager.rules = rules;
            EditorUtility.SetDirty(manager);

            EditorGUIUtility.PingObject(rules);
            Selection.activeObject = rules;
        }
    }
}
#endif
