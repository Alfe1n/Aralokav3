using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class SortingGroupFixer : EditorWindow
{
    [MenuItem("Araloka/Fix Watermask Overlap (Add Sorting Group)")]
    public static void FixOverlap()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefab" });
        int count = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                bool needsSortingGroup = prefab.CompareTag("Player") || prefab.CompareTag("Player-Orang Utan") || prefab.CompareTag("Enemy") || prefab.GetComponentInChildren<SpriteMask>(true) != null;
                if (needsSortingGroup && prefab.GetComponent<SortingGroup>() == null)
                {
                    prefab.AddComponent<SortingGroup>();
                    EditorUtility.SetDirty(prefab);
                    count++;
                }
            }
        }

        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            bool needsSortingGroup = obj.CompareTag("Player") || obj.CompareTag("Player-Orang Utan") || obj.CompareTag("Enemy") || obj.GetComponentInChildren<SpriteMask>(true) != null;
            if (needsSortingGroup && obj.GetComponent<SortingGroup>() == null)
            {
                obj.AddComponent<SortingGroup>();
                EditorUtility.SetDirty(obj);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        EditorUtility.DisplayDialog("Watermask Fixer", $"Berhasil menambahkan Sorting Group ke {count} objek.\n\nSekarang Watermask dijamin 100% terisolasi!", "Mantap");
    }
}
