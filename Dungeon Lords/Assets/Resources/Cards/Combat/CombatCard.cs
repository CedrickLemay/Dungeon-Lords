using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new combat Card", menuName = "Cards/Combat Card")]
public class CombatCard : ScriptableObject
{
    public new string name;
    public Sprite artwork;
    public string title;
    public string description;
    public string effect;
    public int blood;
}
