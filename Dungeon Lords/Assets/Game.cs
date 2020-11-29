using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    const string MainMenuStr = "MainMenu";
    const string PlayerBoardStr = "PlayerBoard";
    const string CentralBoardStr = "CentralBoard";

    //Decks
    private Stack<MonsterCard> MonsterDeck;
    private Stack<Aventurier_Card> AdventurerDeck;
    private Stack<TrapCard> TrapDeck;

    //Players
    private PlayerBoard[] players;

    //TODO: give a name to this section

    public MonsterCard[] availableMonsters;
    public TrapCard availableTrap;

    public int[,] minionPlacement;

    //Game States. Used to set the scenes.
    private enum GameState {
        PlaceCard,
        PlaceMinion,
        GetFood,
        ImproveReputation,
        DigTunnels,
        MineGold,
        RecruitImps,
        BuyTraps,
        HireMonster,
        BuildRoom,
        EndTurn
    }

    private GameState currentState = GameState.PlaceCard;
    private int stateQty = Enum.GetValues(typeof(GameState)).Length;


    private enum SceneNames
    {
        MainMenu,
        PlayerBoard,
        CentralBoard
    }

    //Current Scene
    string currentScene;
    BoardScript boardRef;

    //Methods
    private void SetCentralBoardContent()
    {
        //Available Monster
        availableMonsters = new MonsterCard[3];

        for (int i = 0; i < availableMonsters.Length; i++)
        {
            availableMonsters[i] = MonsterDeck.Pop();
        }

        //Available Trap
        availableTrap = TrapDeck.Pop();

        //Minion placement for each actions (set at -1 to say there is none)
        minionPlacement = new int[8, 3];
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                minionPlacement[i, j] = -1;
            }
        }
    }

    public void SetNextSeason()
    {
        ResetMinionPlacement();
        PlayersCardRotation();
        ResetImpsUsages();
    }

    private void ResetMinionPlacement()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                minionPlacement[i, j] = -1;
            }
        }
    }

    private void PlayersCardRotation()
    {
        foreach (PlayerBoard pb in players)
        {
            pb.DoCardRotation();
        }
    }

    private void ResetImpsUsages()
    {
        foreach (PlayerBoard pb in players)
        {
            pb.ResetUsedImps();
        }
    }

    public int GetPlayerIndexAtActionPosition(int actionIndex, int actionPriority)
    {
        return minionPlacement[actionIndex, actionPriority];
    }

    private void SetDecks()
    {
        MonsterDeck = new Stack<MonsterCard>();
        TrapDeck = new Stack<TrapCard>();

        SetMonsterDeck();
        //SetAdventurerDeck();
        SetTrapDeck();
    }

    private void SetTrapDeck()
    {
        UnityEngine.Object[] trapCards = Resources.LoadAll("ScriptableObject/Trap/", typeof(TrapCard));

        foreach (TrapCard tc in trapCards)
        {
            TrapDeck.Push(tc);
        }
    }

    private void SetAdventurerDeck()
    {
        throw new NotImplementedException();
    }

    private void SetMonsterDeck()   //todo: make it random
    {
        UnityEngine.Object[] monsterCards = Resources.LoadAll("ScriptableObject/Monsters/", typeof(MonsterCard));

        foreach (MonsterCard mc in monsterCards)
        {
            MonsterDeck.Push(mc);
        }
    }

    private void SetPlayers()
    {
        players = new PlayerBoard[4];

        for (int i = 0; i < players.Length; i++)
        {
            players[i] = new PlayerBoard();
        }
    }
    public PlayerBoard GetPlayerByID(int index)
    {
        return players[index];
    }

    public void ChangeToNextStep()
    {
        currentState++;
        currentState = (GameState)(((int)currentState) % stateQty);
        setNextState();
    }

    public void StartGame()
    {
        SetDecks();
        SetPlayers();
        SetCentralBoardContent();

        setNextState();
    }

    public void SetCurrentScene(BoardScript bs)
    {
        currentScene = SceneManager.GetActiveScene().name;
        boardRef = bs;
    }

    //Setting states
    private void setNextState()
    {
        switch(currentState)
        {
            case GameState.PlaceCard:
                setStatePlaceCard();
                break;
            case GameState.PlaceMinion:
                setStatePlaceMinion();
                break;
            case GameState.GetFood:
                setStateGetFood();
                break;
            case GameState.ImproveReputation:
                setStateImproveReputation();
                break;
            case GameState.DigTunnels:
                setStateDigTunnels();
                break;
            case GameState.MineGold:
                setStateMineGold();
                break;
            case GameState.RecruitImps:
                setStateRecruitImps();
                break;
            case GameState.BuyTraps:
                setStateBuyTraps();
                break;
            case GameState.HireMonster:
                setStateHireMonster();
                break;
            case GameState.BuildRoom:
                setStateBuildRoom();
                break;
            case GameState.EndTurn:
                setStateEndTurn();
                break;
        }
    }

    internal void ExecuteActionExchange(ActionExchange actionExchange, int playerID)
    {
        PlayerBoard player = players[playerID];
        List<ActionExchange.ReceptionOptions> alreadyPaid = new List<ActionExchange.ReceptionOptions>();
        bool canPay = true;

        foreach (var paymentOption in actionExchange.payment)
        {
            switch (paymentOption)
            {
                case ActionExchange.PaymentOptions.None:
                    break;
                case ActionExchange.PaymentOptions.Coin:
                    if (player.LoseCoin(1))
                        alreadyPaid.Add(ActionExchange.ReceptionOptions.Coin);
                    else
                        canPay = false;
                    break;
                case ActionExchange.PaymentOptions.Evil:
                    if (player.RiseEvil(1))
                        alreadyPaid.Add(ActionExchange.ReceptionOptions.Good);
                    else
                        canPay = false;
                    break;
                case ActionExchange.PaymentOptions.Food:
                    if (player.LoseFood(1))
                        alreadyPaid.Add(ActionExchange.ReceptionOptions.Food);
                    else
                        canPay = false;
                    break;
                case ActionExchange.PaymentOptions.ForeImp:
                    player.UseImp();
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
                    if (player.UseImp())
                        player.GainCoin(1);
                    break;
                case ActionExchange.ReceptionOptions.Food:
                    player.GainFood(1);
                    break;
                case ActionExchange.ReceptionOptions.Good:
                    player.LowerEvil(1);
                    break;
                case ActionExchange.ReceptionOptions.Imp:
                    player.GainImp();
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

    private void setStatePlaceCard()
    {
        LoadScene(PlayerBoardStr);
    }
    private void setStatePlaceMinion()
    {
        LoadScene(CentralBoardStr);
    }
    private void setStateGetFood()
    {
        LoadScene(PlayerBoardStr);
    }
    private void setStateImproveReputation()
    {
        LoadScene(CentralBoardStr);
    }
    private void setStateDigTunnels()
    {
        LoadScene(PlayerBoardStr);
    }
    private void setStateMineGold()
    {
        LoadScene(PlayerBoardStr);
    }
    private void setStateRecruitImps()
    {
        LoadScene(PlayerBoardStr);
    }
    private void setStateBuyTraps()
    {
        LoadScene(CentralBoardStr);
    }
    private void setStateHireMonster()
    {
        LoadScene(CentralBoardStr);
    }
    private void setStateBuildRoom()
    {
        LoadScene(CentralBoardStr);
    }
    private void setStateEndTurn()
    {
        LoadScene(PlayerBoardStr);
    }

    private void LoadScene(string sceneName)
    {
        if (currentScene != sceneName)
        {
            currentScene = sceneName;
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            boardRef.UpdateBoard();
        }
    }

    //Verifying state

    public bool isStatePlaceCard()
    {
        return currentState == GameState.PlaceCard;    
    }
    public bool isStatePlaceMinion()
    {
        return currentState == GameState.PlaceMinion;
    }
    public bool isStateGetFood()
    {
        return currentState == GameState.GetFood;
    }
    public bool isStateImproveReputation()
    {
        return currentState == GameState.ImproveReputation;
    }
    public bool isStateDigTunnels()
    {
        return currentState == GameState.DigTunnels;
    }
    public bool isStateMineGold()
    {
        return currentState == GameState.MineGold;    
    }
    public bool isStateRecruitImps()
    {
        return currentState == GameState.RecruitImps;
    }
    public bool isStateBuyTraps()
    {
        return currentState == GameState.BuyTraps;
    }
    public bool isStateHireMonster()
    {
        return currentState == GameState.HireMonster;
    }
    public bool isStateBuildRoom()
    {
        return currentState == GameState.BuildRoom;
    }
    public bool isStateEndTurn()
    {
        return currentState == GameState.EndTurn;
    }

}
