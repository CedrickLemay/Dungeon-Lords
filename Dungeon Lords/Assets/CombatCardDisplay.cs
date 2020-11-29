using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatCardDisplay : MonoBehaviour
{
    public CombatCard combatCard;
    public Image artwork;
    public Text title;
    public Text description;
    public Text effect;
    public Text blood;
    // Start is called before the first frame update
    void Start()
    {
        artwork.sprite = combatCard.artwork;
        title.text = combatCard.title;
        description.text = combatCard.description;
        effect.text = combatCard.effect;
        blood.text = combatCard.blood.ToString();
    }

}
