using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Action Exchange", menuName = "Action Exchange/Action Exchange")]
public class ActionExchange :ScriptableObject
{
 
    public enum PaymentOptions {None, Coin, Evil, Food, ForeImp, Imp};
    //TODO: Add tunnel as reception?
    public enum ReceptionOptions {None, Coin, Food, Good, Imp, Monster, Room, Spy, Trap, Tunnel};
    
    [Range(1, 3)]
    public int timeLenght = 1; //TODO:Make sure of naming
    public List<PaymentOptions> payment;
    public List<ReceptionOptions> reception;

}
