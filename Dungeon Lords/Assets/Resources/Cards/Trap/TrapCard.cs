using UnityEngine;

[CreateAssetMenu(fileName = "new trap card", menuName = "Cards/Trap Card")]
public class TrapCard : ScriptableObject
{
    public new string name;
    public Sprite artwork;
    public string title;
    public string description;
    public string effect;
}
