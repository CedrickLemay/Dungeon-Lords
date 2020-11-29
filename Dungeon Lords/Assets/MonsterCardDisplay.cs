using UnityEngine;
using UnityEngine.UI;

public class MonsterCardDisplay : MonoBehaviour
{
    public MonsterCard monsterCard;

    public Image artwork;
    public Image cost1;
    public Image cost2;
    public Image cost3;
    public Image cost4;

    public Image attack_type;
    public Text attack_value;

    public Image first_attack_type;
    public Text first_attack_value;
    public Image second_attack_type;
    public Text second_attack_value;
    // Start is called before the first frame update
    void Start()
    {
        artwork.sprite = monsterCard.artwork;
        cost1.sprite = monsterCard.cost1;
        cost2.sprite = monsterCard.cost2;
        cost3.sprite = monsterCard.cost3;
        cost4.sprite = monsterCard.cost4;

        attack_type.sprite = monsterCard.attack_type;
        attack_value.text = monsterCard.attack_value;

        first_attack_type.sprite = monsterCard.first_attack_type;
        first_attack_value.text = monsterCard.first_attack_value;
        second_attack_type.sprite = monsterCard.second_attack_type;
        second_attack_value.text = monsterCard.second_attack_value;
    }
}
