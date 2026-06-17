using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        if (instance != null) return;
        GameObject go = new GameObject("Inventory");
        go.AddComponent<Inventory>();
        DontDestroyOnLoad(go);
    }

    [Header("Inventory Items")]
    public List<string> items = new List<string>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddItem(string itemId)
    {
        if (!items.Contains(itemId))
        {
            items.Add(itemId);
            SaveInventory();
            Debug.Log($"[Inventory] Added item: {itemId}");
        }
    }

    public bool HasItem(string itemId)
    {
        return items.Contains(itemId);
    }

    public void RemoveItem(string itemId)
    {
        if (items.Contains(itemId))
        {
            items.Remove(itemId);
            SaveInventory();
            Debug.Log($"[Inventory] Removed item: {itemId}");
        }
    }

    public void Clear()
    {
        items.Clear();
        SaveInventory();
        Debug.Log("[Inventory] Cleared all items.");
    }

    private void SaveInventory()
    {
        string joined = string.Join(",", items);
        PlayerPrefs.SetString("InventoryItems", joined);
        PlayerPrefs.Save();
        Debug.Log($"[Inventory] Saved items to PlayerPrefs: {joined}");
    }

    private void LoadInventory()
    {
        items.Clear();
        string joined = PlayerPrefs.GetString("InventoryItems", "");
        if (!string.IsNullOrEmpty(joined))
        {
            items.AddRange(joined.Split(','));
        }
        Debug.Log($"[Inventory] Loaded items from PlayerPrefs: {joined}");
    }
}
