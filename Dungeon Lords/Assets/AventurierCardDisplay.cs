using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AventurierCardDisplay : MonoBehaviour
{
    public Aventurier_Card Aventurier;
    public Image artwork;
    public Text life;
    public Image attack_type;
    public Text attack_value;
    public Text danger_value;

    // Start is called before the first frame update
    void Start()
    {
        artwork.sprite = Aventurier.artwork;
        life.text = Aventurier.life.ToString();
        attack_type.sprite = Aventurier.attackType;
        attack_value.text = Aventurier.attackValue.ToString();
        danger_value.text = Aventurier.dangerValue.ToString();
    }

}
