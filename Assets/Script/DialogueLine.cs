using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [TextArea]
    public string text;

    public string speaker = "Bara";

    public Sprite portrait;
}