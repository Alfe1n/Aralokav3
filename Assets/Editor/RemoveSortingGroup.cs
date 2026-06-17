using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class RemoveSortingGroup : EditorWindow
{
    [MenuItem("Araloka/Remove Sorting Group")]
    public static void RemoveSG()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefab" });
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                SortingGroup sg = prefab.GetComponent<SortingGroup>();
                if (sg != null)
                {
                    DestroyImmediate(sg, true);
                    EditorUtility.SetDirty(prefab);
                }
            }
        }

        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            SortingGroup sg = obj.GetComponent<SortingGroup>();
            if (sg != null)
            {
                DestroyImmediate(sg, true);
                EditorUtility.SetDirty(obj);
            }
        }

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        EditorUtility.DisplayDialog("Done", "Sorting Group removed.", "OK");
    }
}
