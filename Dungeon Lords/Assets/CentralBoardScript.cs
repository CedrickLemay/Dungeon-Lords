using Assets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CentralBoardScript : BoardScript
{
    private Game gameRef;
    public Button nextStepBtn;
    public Button execStepBtn;
    public TextMeshProUGUI stepNameTxt;
    public TextMeshProUGUI stepDescTxt;
    private bool isMonsterSelected;
    private int selectedMonsterIndex;

    private const string pathToActions = "ScriptableObject/Actions/";

    public void Start()
    {
        gameRef = GameObject.Find("__app").GetComponent<Game>();
        gameRef.SetCurrentScene(this);
        UpdateBoard();
        isMonsterSelected = false;
    }

    private void PlaceMinions()
    {
        Transform actionsSections = GameObject.Find("CentralBoard").transform.Find("Actions");

        Sprite s = Resources.Load<Sprite>("Sprite/check_mark");

        int sectionIndex = 0;
        foreach (Transform section in actionsSections)
        {
            int subSectionIndex = 0;
            foreach (Transform subSection in section)
            {
                if (gameRef.minionPlacement[sectionIndex, subSectionIndex] != -1)
                {
                    subSection.GetComponent<Image>().sprite = s;    //will be set back to null in end turn state.
                    subSection.GetComponent<Image>().color = new Color(1, 1, 1, 1); //make it invisible
                }
                subSectionIndex++;
            }
            sectionIndex++;
        }
    }

    private void PlaceEvil()
    {
        Transform evilCounter = GameObject.Find("CentralBoard").transform.Find("EvilCounter");
        //Change sprite of each
        foreach(Transform child in evilCounter.transform)
        {
            foreach(Transform childOfChild in child.transform)
            {
                childOfChild.GetComponent<Image>().sprite = null;
                childOfChild.GetComponent<Image>().color = new Color(1, 1, 1, 0); ;
            }
        }

        //Place new
        Sprite s = Resources.Load<Sprite>("Sprite/check_mark");

        int evilValue = 14 - gameRef.GetPlayerByID(0).EvilCounter;

        Transform t = evilCounter.GetChild(evilValue);
        t = t.GetChild(0);  //TODO: eventually each player
        t.GetComponent<Image>().sprite = s;
        t.GetComponent<Image>().color = new Color(1, 1, 1, 1);

    }

    private void PlaceMonsterCard()
    {
        //Get rid of existing ones

        
        Transform go = GameObject.Find("CentralBoard").transform.Find("AvailableMonster");

        for (int i = 0; i < 3; i++)
        {
            foreach (Transform child in go.transform.Find("AM" + (i + 1).ToString()))
            {                
                GameObject.Destroy(child.gameObject);                 
            }            
        }        

        //New ones, if necessary

        GameObject cardPrefab = Resources.Load<GameObject>("Prefab/MonsterCard");

        Vector3 pos = new Vector3(0.0f, 0.0f);

        GameObject clone;
        MonsterCard mc;
        MonsterCardDisplay mcd;

        int index = 0;  //temp?
        foreach (Transform t in go)
        {
            mc = gameRef.availableMonsters[index];
            mcd = cardPrefab.GetComponent("MonsterCardDisplay") as MonsterCardDisplay;
            mcd.monsterCard = mc;

            clone = GameObject.Instantiate(cardPrefab, t);

            float scaleFactor = ((RectTransform)t).rect.height / ((RectTransform)cardPrefab.transform).rect.height;

            clone.transform.localPosition = pos;
            clone.transform.localScale = new Vector3(scaleFactor, scaleFactor);
            clone.name = "Monster" + (index + 1).ToString();

            index++;
        }
    }

    private void PlaceTrapCard()
    {
        Transform t = GameObject.Find("CentralBoard").transform.Find("TrapDeck");

        GameObject cardPrefab = Resources.Load<GameObject>("Prefab/TrapCard");

        Vector3 pos = new Vector3(0.0f, 0.0f);

        GameObject clone;
        TrapCard tc;
        TrapCardDisplay tcd;

        tc = gameRef.availableTrap;
        tcd = cardPrefab.GetComponent("TrapCardDisplay") as TrapCardDisplay;
        tcd.trapCard = tc;

        clone = GameObject.Instantiate(cardPrefab, t);

        float scaleFactor = ((RectTransform)t).rect.height / ((RectTransform)cardPrefab.transform).rect.height;

        clone.transform.localPosition = pos;
        clone.transform.localScale = new Vector3(scaleFactor, scaleFactor);
        clone.name = "Trap";
        
    }

    public void nextStep()
    {
        gameRef.ChangeToNextStep();
        PlaceMonsterCard();
        PlaceTrapCard();
        PlaceMinions();
        UpdateBoard();

    }
    public override void UpdateBoard()
    {
        SetStateSpecific();
        PlaceMonsterCard();
        PlaceTrapCard();
        PlaceMinions();
        PlaceEvil();

        nextStepBtn.interactable = false;
        execStepBtn.interactable = true;
    }

    private void SetStateSpecific()
    {
        SetStateText();

        if (gameRef.isStatePlaceMinion())
        {
           
        }
        else if (gameRef.isStateImproveReputation())
        {
           
        }
        else if (gameRef.isStateBuyTraps())
        {
           
        }
        else if (gameRef.isStateHireMonster())
        {
            for (int i = 0; i < 3; i++)
            {
                Transform t = GameObject.Find("CentralBoard").transform.Find("AvailableMonster").transform.Find("AM" + (i + 1).ToString());
                GameObject go = t.GetChild(0).gameObject;
                Button b = go.GetComponent<Button>();
                b.onClick.AddListener(() => { MonsterCard_OnClick(); });

            }
        }
        else if (gameRef.isStateBuildRoom())
        {
           
        }
    }

    private void SetStateText()
    {
        if (gameRef.isStatePlaceMinion())
        {
            stepNameTxt.text = "Place Minion";
            stepDescTxt.text = "This will place minions based on the cards you played.";
        }
        else if (gameRef.isStateImproveReputation())
        {
            stepNameTxt.text = "Improve Reputation";
            stepDescTxt.text = "This will improve your reputation (right section of the board).";
        }
        else if (gameRef.isStateBuyTraps())
        {
            stepNameTxt.text = "Buy Traps";
            stepDescTxt.text = "Buy a trap from the trap deck.";
        }
        else if (gameRef.isStateHireMonster())
        {
            stepNameTxt.text = "Hire Monster";
            stepDescTxt.text = "Chose a monster to hire.";
        }
        else if (gameRef.isStateBuildRoom())
        {
            stepNameTxt.text = "Build Room";
            stepDescTxt.text = "Build in your dungeon one of the available room.";
        }
    }

    public void execStep()  // TODO: when there, make it so we deal with every player.
    {
        if (gameRef.isStatePlaceMinion())
        {  
            int index = 0;
            foreach(PlayerBoard.OrderCard oc in gameRef.GetPlayerByID(0).OrderUsed)
            {                
                gameRef.GetPlayerByID(0).AvailableMinion[index] = !PlaceInNextAvailable((int)oc, 0);
                index++;
            }

            PlaceMinions();
        }
        else if (gameRef.isStateImproveReputation())
        {
            int playerID;
            ActionExchange actionExchange;

            for (int i = 0; i < 3; i++)
            {
                playerID = gameRef.GetPlayerIndexAtActionPosition(1, i);
                if (playerID == -1)
                    break;

                actionExchange = Resources.Load<ActionExchange>(pathToActions + "ImproveReputation" + (i+1).ToString());

                gameRef.ExecuteActionExchange(actionExchange, playerID);
            }

            PlaceEvil();

        }
        else if (gameRef.isStateBuyTraps())
        {


            int playerID;
            ActionExchange actionExchange;

            for (int i = 0; i < 3; i++)
            {
                playerID = gameRef.GetPlayerIndexAtActionPosition(5, i);
                if (playerID == -1)
                    break;

                actionExchange = Resources.Load<ActionExchange>(pathToActions + "BuyTraps" + (i + 1).ToString());

                gameRef.ExecuteActionExchange(actionExchange, playerID);
            }
        }
        else if (gameRef.isStateHireMonster())
        {

            //if (!isMonsterSelected)
             //   return;

            int playerID;
            ActionExchange actionExchange;

            for (int i = 0; i < 3; i++)
            {
                playerID = gameRef.GetPlayerIndexAtActionPosition(6, i);
                if (playerID == -1)
                    break;

                actionExchange = Resources.Load<ActionExchange>(pathToActions + "HireMonster" + (i + 1).ToString());

                gameRef.ExecuteActionExchange(actionExchange, playerID);
            }
        }
        else if (gameRef.isStateBuildRoom())
        {
            int playerID;
            ActionExchange actionExchange;

            for (int i = 0; i < 3; i++)
            {
                playerID = gameRef.GetPlayerIndexAtActionPosition(7, i);
                if (playerID == -1)
                    break;

                actionExchange = Resources.Load<ActionExchange>(pathToActions + "BuildRoom" + (i + 1).ToString());

                gameRef.ExecuteActionExchange(actionExchange, playerID);
            }
        }
        nextStepBtn.interactable = true;
        execStepBtn.interactable = false;
    }

    private bool PlaceInNextAvailable(int orderIndex, int playerIndex)
    {
        for (int i = 0; i < 3; i++)
        {
            if(gameRef.minionPlacement[orderIndex,i] == -1)
            {
                gameRef.minionPlacement[orderIndex, i] = playerIndex;
                return true;
            }
        }
        return false;
    }

    private void MonsterCard_OnClick()
    {
        Debug.Log("Test");
    }

    public void CloseScene()
    {
        SceneManager.UnloadSceneAsync("MiddleBoard");
    }
}
