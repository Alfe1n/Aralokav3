using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class FindMissingScripts : EditorWindow
{
    static FindMissingScripts()
    {
        // Delay execution slightly so the scene is fully loaded and initialized
        EditorApplication.delayCall += FindMissing;
    }

    [MenuItem("Tools/Find Missing Scripts in Scene")]
    public static void FindMissing()
    {
        int missingCount = 0;
        GameObject[] gos = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (GameObject go in gos)
        {
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.LogWarning($"GameObject '{go.name}' at path '{GetGameObjectPath(go)}' has a missing script component!", go);
                    missingCount++;
                }
            }
        }
        
        Debug.Log($"Scan complete. Found {missingCount} missing script references in the active scene.");
    }
    
    private static string GetGameObjectPath(GameObject obj)
    {
        string path = "/" + obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
    }
}
