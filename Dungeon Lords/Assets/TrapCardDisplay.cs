using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrapCardDisplay : MonoBehaviour
{
    public TrapCard trapCard;
    public Image artwork;
    public Text title;
    public Text description;
    public Text effect;
    // Start is called before the first frame update
    void Start()
    {
        artwork.sprite = trapCard.artwork;
        title.text = trapCard.title;
        description.text = trapCard.description;
        effect.text = trapCard.effect;
    }
}
