#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VehicleSystem.EditorTools
{
    /// <summary>Editor helpers for the audio module. Menu: Tools > Vehicle.</summary>
    public static class AudioBankTools
    {
        const string ResourcesFolder = "Assets/Scripts/Audio/Resources";
        const string BankFolder = ResourcesFolder + "/VehicleSystem";
        const string BankPath = BankFolder + "/AudioBank.asset";

        [MenuItem("Tools/Vehicle/Create Audio Bank")]
        public static void CreateBank()
        {
            AudioBank existing = FindBank();
            if (existing != null)
            {
                int added = existing.AddMissingDefaults();
                EditorUtility.SetDirty(existing);
                AssetDatabase.SaveAssets();
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);

                string path = AssetDatabase.GetAssetPath(existing);
                if (!path.Contains("/Resources/VehicleSystem/"))
                    Debug.LogWarning("[Audio] The AudioBank is at " + path + ". The game only loads it from a Resources/VehicleSystem folder.");
                Debug.Log("[Audio] AudioBank already exists (" + path + "). Added " + added + " missing default entries.");
                return;
            }

            EnsureFolder("Assets/Scripts/Audio", "Resources");
            EnsureFolder(ResourcesFolder, "VehicleSystem");

            AudioBank bank = ScriptableObject.CreateInstance<AudioBank>();
            bank.AddMissingDefaults();
            AssetDatabase.CreateAsset(bank, BankPath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = bank;
            EditorGUIUtility.PingObject(bank);
            Debug.Log("[Audio] Created " + BankPath + ". Drag clips into the entries.");
        }

        [MenuItem("Tools/Vehicle/Apply Audio Import Settings To Bank")]
        public static void ApplyImportSettings()
        {
            AudioBank bank = FindBank();
            if (bank == null)
            {
                Debug.LogWarning("[Audio] No AudioBank found. Use Tools > Vehicle > Create Audio Bank first.");
                return;
            }

            // Music wins over loop wins over short effect if a clip is used by several entries.
            var wanted = new Dictionary<string, int>(); // path -> 0 sfx, 1 loop, 2 music
            foreach (AudioBank.Entry e in bank.entries)
            {
                if (e == null || e.clips == null) continue;
                int kind = e.category == AudioCategory.Music ? 2 : (e.loop || e.category == AudioCategory.Engine ? 1 : 0);
                foreach (AudioClip clip in e.clips)
                {
                    if (clip == null) continue;
                    string path = AssetDatabase.GetAssetPath(clip);
                    if (string.IsNullOrEmpty(path)) continue;
                    int old;
                    if (!wanted.TryGetValue(path, out old) || kind > old) wanted[path] = kind;
                }
            }

            int changed = 0;
            foreach (KeyValuePair<string, int> pair in wanted)
            {
                AudioImporter importer = AssetImporter.GetAtPath(pair.Key) as AudioImporter;
                if (importer == null) continue;

                AudioImporterSampleSettings s = importer.defaultSampleSettings;
                s.compressionFormat = AudioCompressionFormat.Vorbis;
                s.quality = 0.5f;
                s.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
                bool mono = true;
                bool preload = true;
                switch (pair.Value)
                {
                    case 0: s.loadType = AudioClipLoadType.DecompressOnLoad; break;
                    case 1: s.loadType = AudioClipLoadType.CompressedInMemory; break;
                    default:
                        s.loadType = AudioClipLoadType.Streaming;
                        mono = false;
                        preload = false;
                        break;
                }

                importer.defaultSampleSettings = s;
                importer.forceToMono = mono;
                importer.preloadAudioData = preload;
                importer.SaveAndReimport();
                changed++;
            }

            Debug.Log("[Audio] Applied mobile import settings to " + changed + " clip(s).");
        }

        static AudioBank FindBank()
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioBank");
            if (guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<AudioBank>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        static void EnsureFolder(string parent, string child)
        {
            string full = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(full)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}
#endif
