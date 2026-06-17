using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class RescueManager : MonoBehaviour
{
    public static RescueManager instance;

    private HashSet<string> rescued = new HashSet<string>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        if (instance != null) return;
        GameObject go = new GameObject("RescueManager");
        go.AddComponent<RescueManager>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshRescuedNPCs();
    }

    public void RefreshRescuedNPCs()
    {
        RescuedNPCAppear[] npcs = Resources.FindObjectsOfTypeAll<RescuedNPCAppear>();
        foreach (var npc in npcs)
        {
            if (npc == null || npc.gameObject == null) continue;

            // Pastikan objek ada di scene aktif, bukan asset prefab di Project window
            if (npc.gameObject.scene.name == null || string.IsNullOrEmpty(npc.gameObject.scene.name))
            {
                continue;
            }

            bool rescuedStatus = IsRescued(npc.rescueId);
            npc.gameObject.SetActive(rescuedStatus);
            Debug.Log($"[RescueManager] Set NPC '{npc.gameObject.name}' active state to: {rescuedStatus} (rescueId: {npc.rescueId})");
        }
    }

    public void Rescue(string id)
    {
        rescued.Add(id);
        PlayerPrefs.SetInt("rescued_" + id, 1);
        PlayerPrefs.Save();
        
        // Segera refresh NPC di scene jika ada yang statusnya berubah
        RefreshRescuedNPCs();
    }

    public bool IsRescued(string id)
    {
        if (rescued.Contains(id)) return true;

        // fallback PlayerPrefs supaya persist antar sesi
        bool fromPrefs = PlayerPrefs.GetInt("rescued_" + id, 0) == 1;
        if (fromPrefs) rescued.Add(id);
        return fromPrefs;
    }
}
