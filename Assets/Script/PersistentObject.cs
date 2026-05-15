using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    private static bool alreadyExists;

    void Awake()
    {
        if (alreadyExists)
        {
            Destroy(gameObject);
            return;
        }

        alreadyExists = true;

        DontDestroyOnLoad(gameObject);
    }
}