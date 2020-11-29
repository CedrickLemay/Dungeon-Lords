using UnityEngine;

[CreateAssetMenu(fileName = "new monster card", menuName = "Cards/Monster Card")]
public class MonsterCard : ScriptableObject
{
    public new string name;
    
    public Sprite artwork;
    public Sprite cost1;
    public Sprite cost2;
    public Sprite cost3;
    public Sprite cost4;

    public Sprite attack_type;
    public string attack_value;

    public Sprite first_attack_type;
    public string first_attack_value;
    public Sprite second_attack_type;
    public string second_attack_value;

}
