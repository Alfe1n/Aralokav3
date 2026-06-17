using UnityEngine;
using UnityEditor;

public class DebugPrefsClear
{
    [MenuItem("Araloka/Reset Save Data (Clear PlayerPrefs)")]
    public static void ClearPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[Araloka Debug] PlayerPrefs (Save Data & Triggers) has been cleared successfully!");
    }
}
