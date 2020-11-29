using Assets;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using System.Drawing;

public class PlayerBoardScript : BoardScript
{
    private const string pathToActions = "ScriptableObject/Actions/";
    private const string pathToOrderCards = "Prefab/OrderCard";
    private const string pathToOrderCardsSprites = "Sprite/OrderCard/";

    public PlayerBoard playerBoard;
    public GameObject orderCardUI;
    public Sprite tunnel;

    public Text foodStorageText;
    public Text treasuryText;
    public Text impDenText;
    public Text taxMarkText;

    public TextMeshProUGUI stepNameTxt;
    public TextMeshProUGUI stepDescTxt;

    public Button nextStepBtn;
    public Button execStepBtn;

    //OrderCard parent empty object use for position
    public GameObject ordersSection;

    AsyncOperation asyncLoadLevel;

    Game gameRef;


    void Start()
    {
        gameRef = GameObject.Find("__app").GetComponent<Game>();
        playerBoard = gameRef.GetPlayerByID(0);
        tunnel = Resources.Load<Sprite>("Sprite/Cross");

        UpdateConsumableQuantity();

        printBoardContents();

        gameRef.SetCurrentScene(this);

        UpdateBoard();


        base.Start();
    }

    private void printBoardContents()
    {
        //Destroy all the gameobject of each card.
        foreach (Transform child in ordersSection.transform)
        {
            if (child.childCount > 0)
            {
                foreach (Transform childOfChild in child.transform)
                {
                    GameObject.Destroy(childOfChild.gameObject);
                }
            }

        }

        //Show the new one


        PlayerBoard.OrderCard[] placedCards = new PlayerBoard.OrderCard[5];


        for (int i = 0; i < playerBoard.OrderUsed.Length; i++)
        {
            placedCards[i] = playerBoard.OrderUsed[i];
        }

        for (int i = 0; i < playerBoard.OrderInnaccessible.Length; i++)
        {
            placedCards[3 + i] = playerBoard.OrderInnaccessible[i];
        }

        GameObject cardPrefab;
        cardPrefab = Resources.Load<GameObject>(pathToOrderCards);
        GameObject cardIcon = cardPrefab.transform.Find("CardIcon").gameObject;

        Sprite s;
        string spritePath;

        for (int i = 0; i < placedCards.Length; i++)
        {
            if (placedCards[i] == PlayerBoard.OrderCard.None) continue;

            spritePath = "Sprite/OrderCard/" + ((PlayerBoard.OrderCard)placedCards[i]).ToString();
            s = Resources.Load<Sprite>(spritePath);
            cardIcon.GetComponent<Image>().sprite = s;

            printContent(cardPrefab, ordersSection.transform.GetChild(i).gameObject);
        }

        Sprite clone;
        Transform go = GameObject.Find("PlayerBoard").transform.Find("Dungeon");
        int index = 0;
        foreach (Transform t in go)
        {
            int X = (index / 5);
            int Y = (index % 5);
            if (playerBoard.dungeon[X, Y] == PlayerBoard.DungeonTile.Tunnel)
            {
                //   GameObject.Instantiate(tunnel, t);
                t.GetComponent<Image>().sprite = tunnel;
                t.GetComponent<Image>().color = new UnityEngine.Color(1,1,1,1);
            }
            ++index;
        }
    }

    private void printContent(GameObject obj, GameObject parent)
    {
        GameObject.Instantiate(obj, parent.transform);
    }

    private void UpdateConsumableQuantity()
    {
        foodStorageText.text = playerBoard.Food.ToString();
        treasuryText.text = playerBoard.Coin.ToString();
        taxMarkText.text = playerBoard.TaxMark.ToString();
        impDenText.text = (playerBoard.WorkersCount - playerBoard.WorkersUsed) + " / " + playerBoard.WorkersCount;
    }

    private void ResetUsedImps()
    {
        playerBoard.ResetUsedImps();
    }

    public void ChoseOrder(int usedCardIndex)    //TODO: change name
    {
        orderCardUI.SetActive(true);

        GameObject cardPrefab;
        cardPrefab = Resources.Load<GameObject>(pathToOrderCards);
        GameObject cardIcon = cardPrefab.transform.Find("CardIcon").gameObject;

        Sprite s;
        string spritePath;

        float cardWidth = ((RectTransform)cardPrefab.transform).rect.width;

        const float SPACEWIDTH = 20.0f;

        int index = 1;
        foreach (PlayerBoard.OrderCard oc in playerBoard.OrderAccessible)
        {
            spritePath = pathToOrderCardsSprites + oc.ToString();
            s = Resources.Load<Sprite>(spritePath);
            cardIcon.GetComponent<Image>().sprite = s;

            Vector3 pos = new Vector3(0.0f, 0.0f);
            pos.x = -((((float)playerBoard.OrderAccessible.Count / 2 - index) * (cardWidth + SPACEWIDTH)) + ((cardWidth + SPACEWIDTH) / 2));

            GameObject clone = GameObject.Instantiate(cardPrefab, orderCardUI.transform);

            clone.transform.localPosition = pos;

            //int returnValue = index; //it will follow index value if i dont use this.
            clone.GetComponent<Button>().onClick.AddListener(() => { SetUsedCard(usedCardIndex, oc); });
            index++;            
        }
    }

    public void CloseOrderChoice()
    {
        foreach (Transform child in orderCardUI.transform)
            Destroy(child.gameObject);
        orderCardUI.SetActive(false);
    }

    private void SetUsedCard(int index, PlayerBoard.OrderCard oc)
    {
        playerBoard.OrderUsed[index] = oc;
        playerBoard.OrderAccessible.Remove(oc);
        CloseOrderChoice();
        printBoardContents();

        //checkIfAllSet();
    }
/*
    private void checkIfAllSet()
    {
        bool isAllSet = true;

        foreach (PlayerBoard.OrderCard oc in playerBoard.OrderUsed)
        { 
            if(oc == PlayerBoard.OrderCard.None)
            {
                isAllSet = false;
                break;
            }
        }

        if(isAllSet)
        {
            gameRef.
        }

    }
*/
    public void orderCardCycle()
    {
        ApplyOrder(playerBoard.OrderUsed);
        playerBoard.OrderAccessible.Add(playerBoard.OrderUsed[0]);
        playerBoard.OrderAccessible.Add(playerBoard.OrderInnaccessible[0]);
        playerBoard.OrderAccessible.Add(playerBoard.OrderInnaccessible[1]);

        playerBoard.OrderInnaccessible[0] = playerBoard.OrderUsed[1];
        playerBoard.OrderInnaccessible[1] = playerBoard.OrderUsed[2];

        playerBoard.OrderUsed[0] = PlayerBoard.OrderCard.None;
        playerBoard.OrderUsed[1] = PlayerBoard.OrderCard.None;
        playerBoard.OrderUsed[2] = PlayerBoard.OrderCard.None;

        ResetUsedImps();
        UpdateConsumableQuantity();
        printBoardContents();

    }

    /*For now only the first position will be used*/
    private void ApplyOrder(PlayerBoard.OrderCard[] orderUsed)
    {
        ActionExchange actionExchange = null;
        foreach (var order in orderUsed)
        {
            var alreadyPaid = new List<ActionExchange.ReceptionOptions>();
            var canPay = true;
            actionExchange = Resources.Load<ActionExchange>(pathToActions + order + "1"); /*HardCode first position on central board*/
            foreach (var paymentOption in actionExchange.payment)
            {
                switch (paymentOption) 
                {
                    case ActionExchange.PaymentOptions.None:
                        break;
                    case ActionExchange.PaymentOptions.Coin:
                        if(playerBoard.LoseCoin(1))
                            alreadyPaid.Add(ActionExchange.ReceptionOptions.Coin);
                        else
                            canPay = false;
                        break;
                    case ActionExchange.PaymentOptions.Evil:
                        if(playerBoard.RiseEvil(1))
                            alreadyPaid.Add(ActionExchange.ReceptionOptions.Good);
                        else
                            canPay = false;
                        break;
                    case ActionExchange.PaymentOptions.Food:
                        if(playerBoard.LoseFood(1))
                            alreadyPaid.Add(ActionExchange.ReceptionOptions.Food);
                        else
                            canPay = false;
                        break;
                    case ActionExchange.PaymentOptions.ForeImp:
                        playerBoard.UseImp();
                        break;
                    case ActionExchange.PaymentOptions.Imp:
                        break;
                    default:
                        break;
                }
            }

            var reception = canPay ? actionExchange.reception : alreadyPaid;

            foreach (var receptionOption in reception)
            {
                switch (receptionOption)
                {
                    case ActionExchange.ReceptionOptions.None:
                        break;
                    case ActionExchange.ReceptionOptions.Coin:
                        if(playerBoard.UseImp())
                            playerBoard.GainCoin(1);
                        break;
                    case ActionExchange.ReceptionOptions.Food:
                        playerBoard.GainFood(1);
                        break;
                    case ActionExchange.ReceptionOptions.Good:
                        playerBoard.LowerEvil(1);
                        break;
                    case ActionExchange.ReceptionOptions.Imp:
                        playerBoard.GainImp();
                        break;
                    case ActionExchange.ReceptionOptions.Monster:                       
                        break;
                    case ActionExchange.ReceptionOptions.Room:
                        break;
                    case ActionExchange.ReceptionOptions.Spy:
                        break;
                    case ActionExchange.ReceptionOptions.Trap:
                        break;
                    case ActionExchange.ReceptionOptions.Tunnel:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    public void ShowCentralBoard()
    {
        SceneManager.LoadScene("MiddleBoard", LoadSceneMode.Additive);
    }

    public void nextStep()
    {
        gameRef.ChangeToNextStep();
    }

    public void execStep()
    {
        
        if (gameRef.isStatePlaceCard())
        {
            foreach (PlayerBoard.OrderCard oc in playerBoard.OrderUsed)
            {
                if (oc == PlayerBoard.OrderCard.None)
                    return; //Not all set.
            }
        }
        else if (gameRef.isStateGetFood())
        {
            int playerID;
            ActionExchange actionExchange;

            for (int i = 0; i < 3; i++)
            {
                playerID = gameRef.GetPlayerIndexAtActionPosition(0, i);
                if (playerID == -1)
                    break;

                actionExchange = Resources.Load<ActionExchange>(pathToActions + "GetFood" + (i + 1).ToString());

                gameRef.ExecuteActionExchange(actionExchange, playerID);
            }
        }
        else if (gameRef.isStateDigTunnels())
        {
            int playerID;
            ActionExchange actionExchange;

            for (int i = 0; i < 3; i++)
            {
                playerID = gameRef.GetPlayerIndexAtActionPosition(2, i);
                if (playerID == -1)
                    break;

                actionExchange = Resources.Load<ActionExchange>(pathToActions + "DigTunnels" + (i + 1).ToString());

                gameRef.ExecuteActionExchange(actionExchange, playerID);
            }
        }
        else if (gameRef.isStateMineGold())
        {
            int playerID;
            ActionExchange actionExchange;

            for (int i = 0; i < 3; i++)
            {
                playerID = gameRef.GetPlayerIndexAtActionPosition(3, i);
                if (playerID == -1)
                    break;

                actionExchange = Resources.Load<ActionExchange>(pathToActions + "MineGold" + (i + 1).ToString());

                gameRef.ExecuteActionExchange(actionExchange, playerID);
            }
        }
        else if (gameRef.isStateRecruitImps())
        {
            int playerID;
            ActionExchange actionExchange;

            for (int i = 0; i < 3; i++)
            {
                playerID = gameRef.GetPlayerIndexAtActionPosition(4, i);
                if (playerID == -1)
                    break;

                actionExchange = Resources.Load<ActionExchange>(pathToActions + "RecruitImps" + (i + 1).ToString());

                gameRef.ExecuteActionExchange(actionExchange, playerID);
            }
        }
        else if (gameRef.isStateEndTurn())
        {
            gameRef.SetNextSeason();
        }

        execStepBtn.interactable = false;
        nextStepBtn.interactable = true ;
        UpdateConsumableQuantity();
        printBoardContents();
    }

    public override void UpdateBoard()
    {

        SetStateSpecific();
        nextStepBtn.interactable = false;
        execStepBtn.interactable = true;
    }

    private void test()
    {
        Debug.Log("TESTTEST");
    }

    private void SetStateSpecific()
    {
        SetStateText();

        if (gameRef.isStatePlaceCard())
        {
           
        }
        else if (gameRef.isStateGetFood())
        {
           
        }
        else if (gameRef.isStateDigTunnels())
        {
            if (gameRef.minionPlacement[2, 0] == -1 &&
                gameRef.minionPlacement[2, 1] == -1 &&
                gameRef.minionPlacement[2, 2] == -1) return;
            GameObject go = GameObject.Find("Dungeon");
            int index = 0;
            foreach (Transform child in go.transform)
            {
                Point p = new Point(index / 5, index % 5);
                child.GetComponent<Button>().onClick.AddListener(() => { PlaceTunnel(p); });
                index++;
            }
        }
        else if (gameRef.isStateMineGold())
        {
            GameObject go = GameObject.Find("Dungeon");
            int index = 0;
            foreach (Transform child in go.transform)
            {
                Point p = new Point(index / 5, index % 5);
                child.GetComponent<Button>().onClick.RemoveAllListeners();
                index++;
            }
        }
        else if (gameRef.isStateRecruitImps())
        {
         
        }
        else if (gameRef.isStateEndTurn())
        {
         
        }
    }

    private void PlaceTunnel(Point position)
    {
        if (playerBoard.UseImp())
        {
            if (playerBoard.ValidateTilePlacement(position))
            {
                GameObject go = GameObject.Find("Dungeon");

                Transform t = go.transform.GetChild(position.X * 5 + position.Y);

                t.GetComponent<Image>().sprite = tunnel;
                t.GetComponent<Image>().color = new UnityEngine.Color(1, 1, 1, 1);
            }
            UpdateConsumableQuantity();
        }
    }

    private void SetStateText()
    {
        if (gameRef.isStatePlaceCard())
        {
            stepNameTxt.text = "Place Card";
            stepDescTxt.text = "Place your three order cards.";
        }
        else if (gameRef.isStateGetFood())
        {
            stepNameTxt.text = "Get Food";
            stepDescTxt.text = "Exchange money for food.";
        }
        else if (gameRef.isStateDigTunnels())
        {
            stepNameTxt.text = "Dig Tunnels";
            stepDescTxt.text = "Dig tunnels in your dungeons.";
        }
        else if (gameRef.isStateMineGold())
        {
            stepNameTxt.text = "Mine Gold";
            stepDescTxt.text = "Mine for gold and get money!";
        }
        else if (gameRef.isStateRecruitImps())
        {
            stepNameTxt.text = "Recruit Imps";
            stepDescTxt.text = "Recruit imps to work for you!";
        }
        else if (gameRef.isStateEndTurn())
        {
            stepNameTxt.text = "End Turn";
            stepDescTxt.text = "End your turn and let's go to the next season.";
        }

    }


    /*
     * 
     *          ZONE DE FONCTION TEST (il ne devrait plus rien avoir eventuellement)
     * 
     */



    public void Z_InstatiateCard()
    {
        orderCardUI.SetActive(true);

        GameObject cardPrefab;
        cardPrefab = Resources.Load<GameObject>("Cards/Trap/Card");

        TrapCard c = Resources.Load<TrapCard>("Cards/Trap/TrapDoor");
        TrapCardDisplay cd = cardPrefab.transform.GetComponent("TrapCardDisplay") as TrapCardDisplay;
        cd.trapCard = c;

        Vector3 pos = new Vector3(0.0f, 0.0f);


        GameObject clone = GameObject.Instantiate(cardPrefab, orderCardUI.transform);

        clone.transform.localPosition = pos;

    }


    public void Quit()
    {

        Application.Quit();
    }


}
