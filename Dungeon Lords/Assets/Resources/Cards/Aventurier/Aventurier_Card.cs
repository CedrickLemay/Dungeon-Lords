using UnityEngine;

[CreateAssetMenu(fileName = "new aventurier Card", menuName = "Cards/Aventurier Card")]
public class Aventurier_Card : ScriptableObject
{
    public new string name;
    public Sprite artwork;
    public int dangerValue;
    public int life;
    public Sprite attackType;
    public int attackValue;
}
