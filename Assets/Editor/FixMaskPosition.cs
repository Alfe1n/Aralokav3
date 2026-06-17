using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class FixMaskPosition : EditorWindow
{
    [MenuItem("Araloka/Fix Mask Positions")]
    public static void FixPos()
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (obj.CompareTag("Enemy") || obj.CompareTag("Player") || obj.CompareTag("Player-Orang Utan"))
            {
                Transform mask = obj.transform.Find("Watermask");
                if (mask == null) mask = obj.transform.Find("WaterMask"); // Case sensitive check
                
                if (mask != null)
                {
                    // Reset local position so it sits exactly on the character
                    mask.localPosition = new Vector3(0, 0, 0);
                    // Make it large enough to cover the bottom half
                    mask.localScale = new Vector3(10f, 5f, 1f);
                    
                    EditorUtility.SetDirty(obj);
                }
            }
        }

        // Also fix prefabs
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefab" });
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                Transform mask = prefab.transform.Find("Watermask");
                if (mask == null) mask = prefab.transform.Find("WaterMask");
                
                if (mask != null)
                {
                    mask.localPosition = new Vector3(0, 0, 0);
                    mask.localScale = new Vector3(10f, 5f, 1f);
                    EditorUtility.SetDirty(prefab);
                }
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Done", "Posisi Watermask berhasil dirapikan ke titik tengah karakter!", "OK");
    }
}
