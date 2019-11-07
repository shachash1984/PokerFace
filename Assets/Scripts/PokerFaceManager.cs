using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum GameState { Standby, Swap, Play, FirstRound, SecondRound, FinalRound, Over, BuyIn, Reveal2, Reveal1, DeclareWinner, Inspection }
public enum Turn : int { None, Down, Left, Up, Right }
public class PokerFaceManager : MonoBehaviour {

    #region Events & Delegates

    #endregion


    #region Fields
    static public PokerFaceManager S;
    [SerializeField] private PokerPlayer[] _players;
    [SerializeField] private Dictionary<PokerPlayer, PokerHand> _activePlayersHands;
    [SerializeField] private Deck deck;
    [SerializeField] private TextAsset deckXML;
    public Layout layout;
    [SerializeField] private TextAsset layoutXML;
    public Vector3 layoutCenter;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Transform layoutAnchor;
    public List<CardPokerFace> table;
    public List<CardPokerFace> discardPile;
    public List<CardPokerFace> drawPile;
    public int buyInAmount = 50;
    public bool inspectionOver = false;
    [SerializeField] private AudioSource audioSource;
    #endregion

    #region Properties
    [SerializeField] private PokerPlayer _currentPlayer;
    public PokerPlayer currentPlayer
    {
        get
        {
            return _currentPlayer;
        }
        private set
        {
            _currentPlayer = value;
        }
    }
    [SerializeField] private PokerPlayer _finalPlayer;
    public PokerPlayer finalPlayer
    {
        get
        {
            return _finalPlayer;
        }
        private set
        {
            _finalPlayer = value;
        }
    }
    [SerializeField] private PokerPlayer _lastPlayerToSpeak;
    public PokerPlayer lastPlayerToSpeak
    {
        get
        {
            return _lastPlayerToSpeak;
        }
        private set
        {
            _lastPlayerToSpeak = value;
        }
    }
    public PokerPlayer localPlayer { get; private set; }
    public PokerPlayer dealer { get; private set; }
    [SerializeField] private List<PokerPlayer> _winners;
    public List<PokerPlayer> winners
    {
        get
        {
            return _winners;
        }
        set
        {
            _winners = value;
        }
    }
    [SerializeField] private List<PokerPlayer> _activePlayers;
    public List<PokerPlayer> activePlayers
    {
        get
        {
            return _activePlayers;
        }
        private set
        {
            _activePlayers = value;
        }
    }
    [SerializeField] private int _amountToBet;
    public int amountToBet
    {
        get
        {
            return _amountToBet;
        }
        private set
        {
            _amountToBet = value;
        }
    }
    [SerializeField] private float _timer;
    public float timer
    {
        get
        {
            return _timer;
        }
        private set
        {
            _timer = value;
        }
    }
    public float maxTime { get; private set; }
    [SerializeField] private GameState _gameState;
    public GameState gameState
    {
        get
        {
            return _gameState;
        }
        private set
        {
            _gameState = value;
        }
    }
    public int pot { get; private set; }
    #endregion

    #region MonoBehaviour Callbacks
    private void Awake()
    {
        if (S != null)
            Destroy(gameObject);
        else
            S = this;
        InitGame();
    }

    private void Start()
    {
        InitBuyIn();
    }

    private void Update()
    {
        if (gameState == GameState.Inspection || gameState == GameState.Play || gameState == GameState.Swap || gameState == GameState.BuyIn || gameState == GameState.Reveal1 || gameState == GameState.Reveal2)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 0;
                switch (gameState)
                {
                    case GameState.Play:
                        localPlayer.SetCommand(new FoldCommand());
                        localPlayer.Action();
                        UIManager.S.ToggleSpark(false);
                        UIManager.S.TogglePlayPanel(false);
                        UIManager.S.ToggleCardButtons(false);
                        UIManager.S.ToggleRaisePanel(false);
                        InitPlay();
                        break;
                    case GameState.Swap:
                        UIManager.S.ToggleSpark(false);
                        InitReveal2(0);
                        UIManager.S.ToggleSwapPanel(false);
                        break;
                    case GameState.Reveal2:
                    case GameState.Reveal1:
                        localPlayer.SetCommand(new FoldCommand());
                        localPlayer.Action();
                        UIManager.S.ToggleSpark(false);
                        UIManager.S.ToggleCardButtonFlash(false);
                        if (gameState == GameState.Reveal2)
                        {
                            InitPlay();
                        }
                        else
                        {
                            SetCurrentPlayerToPlayerOnHisLeft();
                            if (localPlayer == finalPlayer)
                                SetFinalPlayerToPlayerOnHisRight();
                            RemoveFromActivePlayers(localPlayer);
                            RemovePokerHand(localPlayer);
                            StartCoroutine(PlayGame());
                        }
                        UIManager.S.ToggleRevealPanel(false);
                        break;
                    case GameState.BuyIn:
                        localPlayer.SetCommand(new FoldCommand()); // fold
                        localPlayer.Action();
                        UIManager.S.ToggleSpark(false);
                        StartCoroutine(InitSwap());
                        UIManager.S.ToggleBuyInPanel(false);
                        break;
                    case GameState.Inspection:
                        UIManager.S.ToggleInspectionPanel(false);
                        inspectionOver = true;
                        break;
                    default:
                        Debug.LogError("timer is not supposed to be running in " + gameState.ToString());
                        break;
                }
            }
        }
    }
    #endregion

    #region Methods
    public CardPokerFace Draw()
    {
        CardPokerFace cp = drawPile[0]; //pull the 0th CardPokerFace
        drawPile.RemoveAt(0); // then remove it from list<> drawPile
        return cp;
    }

    List<CardPokerFace> ConvertListCardsToListCardPokerFaces(List<Card> lCD)
    {
        List<CardPokerFace> lCP = new List<CardPokerFace>();
        CardPokerFace tCP;
        foreach (Card tCD in lCD)
        {
            tCP = tCD as CardPokerFace;
            lCP.Add(tCP);
        }
        return lCP;
    }

    IEnumerator LayoutGame()
    {
        //empty GameObject serves as an anchor for the tableau
        if (layoutAnchor == null)
        {
            GameObject tGo = new GameObject("_LayoutAnchor");
            layoutAnchor = tGo.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        CardPokerFace cp;
        //follow the layout
        for (int i = 0; i < layout.slotDefs.Count; i++)
        {
            SlotDef tSD = layout.slotDefs[i];
            //iterate through all the SlotDefs in the layout.slotDefs as tSD
            cp = Draw(); //pull a card from the top (beginning) of the drawPile
            cp.faceUp = tSD.faceUp; //set its faceUp
            cp.transform.parent = layoutAnchor; //this replaces the previous parent: deck.deckAnchor which appears as _Deck in the Hierarchy
            cp.transform.localPosition = new Vector3(0f, 4.5f, -tSD.layerID);
            cp.transform.localRotation = Quaternion.identity;
            cp.transform.localScale = new Vector3(0.7f, 0.7f, 0.05f);
            if (_players[tSD.playerID].isIn)
            {
                cp.playerID = (PlayerID)tSD.playerID; //assign the card with the correct player id
                cp.state = CardState.Table; // assign the card state
                cp.layoutID = tSD.id; // assign the card layoutID
                cp.SetSortingLayerName(tSD.layerName); // assign the card sorting layer
                cp.slotDef = tSD; // assign the card slotDef
                table.Add(cp); // add the card to cards on table
                _players[tSD.playerID].cards.Add(cp); // add the card to the relevant player's list
                cp.index = _players[tSD.playerID].cards.Count - 1; // assign the card the correct index
                //Debug.Log("<color=red>name: " + _players[tSD.playerID].name + " cards: " + _players[tSD.playerID].cards.Count + "</color>");

            }
            else
            {
                cp.playerID = PlayerID.Dealer;
                cp.state = CardState.DrawPile;
                drawPile.Add(cp);
            }
        }
        foreach (PokerPlayer p in activePlayers)
        {
            p.InitCards();
        }
        foreach (Card c in deck.cards)
        {
            CardPokerFace cpf = c as CardPokerFace;
            if (cpf.state == CardState.DrawPile)
                cpf.transform.localScale = new Vector3(0.7f, 0.7f, 0.05f);
            if (cpf.state != CardState.DrawPile && cpf.state != CardState.Discard)
            {
                //deal the cards...
                SlotDef tSD = cpf.slotDef;
                cpf.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, tSD.zRotation));
                cpf.transform.DOLocalMove(new Vector3(layout.multiplier.x * tSD.x, layout.multiplier.y * tSD.y, -tSD.layerID), 0.6f);
                PlaySound(SoundType.CardMove3);
                yield return new WaitForSeconds(0.25f);
                

            }
        }
        //position the draw pile
        UpdateDrawPile();
    }

    void UpdateDrawPile()
    {
        CardPokerFace cd;
        //go through all the cards of the drawPile
        for (int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];

            cd.transform.parent = layoutAnchor;

            //position it correctly with the layout.drawPile.stagger
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3
                (
                    layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
                    layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
                    -layout.discardPile.layerID + 0.1f * i
                );
            cd.faceUp = false; //make them all face down
            cd.state = CardState.DrawPile;
            //set depth sorting
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
            cd.playerID = (PlayerID)layout.drawPile.playerID;
        }
    }

    public void MoveToDiscard(CardPokerFace cd, bool immediate = false)
    {
        //set the state of the card to discard
        cd.state = CardState.Discard;
        discardPile.Add(cd);
        cd.transform.parent = layoutAnchor;
        /*if (immediate)
        {
            cd.transform.localPosition = new Vector3
                (
                    layout.multiplier.x * layout.discardPile.x - 80,
                    layout.multiplier.y * layout.discardPile.y,
                    -layout.discardPile.layerID + 0.5f
                );
        }
        else
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(cd.transform.DOMove(new Vector3(0f, 4.5f, 0f), 0.25f));
            seq.Append(cd.transform.DOMove(new Vector3(layout.multiplier.x * layout.discardPile.x - 80, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID + 0.5f), 0f));
            seq.Play();
        }*/

        //position it on the discard pile
        cd.faceUp = false;
        //place it on top of the pile for depth sorting
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
        cd.playerID = (PlayerID)layout.discardPile.playerID;
    }

    public void InitGame()
    {
        gameState = GameState.Standby;
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);
        layout = GetComponent<Layout>();
        layout.ReadLayout(layoutXML.text);
        drawPile = ConvertListCardsToListCardPokerFaces(deck.cards);
        activePlayers = new List<PokerPlayer>();
        winners = new List<PokerPlayer>();
        _activePlayersHands = new Dictionary<PokerPlayer, PokerHand>();
        pot = 0;
        amountToBet = 0;
        localPlayer = _players[0];
        UIManager.S.ToggleCardButtons(true);
        inspectionOver = false;
    }

    private void InitBuyIn()
    {
        gameState = GameState.BuyIn;

        //initializing players total amounts
        foreach (PokerPlayer p in _players)
        {
            if (p.totalAmount <= 50)
                p.InitStats(5000); //TODO: needs to remember older games
        }

        //ai players are buying in
        for (int i = 1; i < _players.Length; i++)
        {
            _players[i].SetCommand(new BuyInCommand());
            _players[i].Action();
            if (_players[i].pokerHand == null && _players[i].isIn)
                _players[i].InitCards();
            AddToActivePlayers(_players[i]);
        }

        UIManager.S.SetTimerPosition(localPlayer); //move the timer to the correct position

        maxTime = 25f; //set the timer to 10 seconds
        timer = maxTime;
        //run timer for buying-in (running in Update)
       
        UIManager.S.ToggleBuyInPanel(true); //init and show buy-in panel

        //local player needs to decide whether to buy-in or fold

    }

    public IEnumerator InitSwap()
    {
        gameState = GameState.Standby;
        UIManager.S.PlayCollectBetsAnimation();
        deck.UpdateAcesRank(); // making sure aces ranks are 14 insted of 1
        yield return StartCoroutine(LayoutGame()); //deal cards
        gameState = GameState.Swap;
        if (localPlayer.isIn)
        {
            deck.MakeButtonCards(localPlayer.cards); // assign button cards
            maxTime = 25f; //run timer for swapping
            timer = maxTime;
            UIManager.S.ToggleSwapPanel(true);//show swap panel 
            //local player needs to select cards to swap and add them to selected list
            //local player needs to swap selected cards
            //play swap cards animation
        }
        else
            InitReveal2(0);
        //if local player is not in. the game will go to reveal 2 cards state

    }

    public void InitReveal2(float delay)
    {
        StartCoroutine(InitReveal2Sequence(delay));
    }
    
    public IEnumerator InitReveal2Sequence(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameState = GameState.Reveal2;
        if (activePlayers.Count == 1)
        {
            InitDeclareWinner();
            yield return null;
            StopCoroutine(InitReveal2Sequence(delay));
        }
        foreach (PokerPlayer p in activePlayers)
        {
            p.InitCards();
        }
        if (localPlayer.isIn)
        {
            maxTime = 25; //run timer for revealing
            timer = maxTime;
            UIManager.S.ToggleRevealPanel(true, 2);//show reveal panel
            //local player needs to select 2 cards to reveal
            //local player needs to reveal selected cards
            yield return StartCoroutine(RevealAIPlayersCards(2, 3f));
        }
        else
        {
            RevealAIPlayersCards(2);
            InitPlay();
        }
    }

    public void InitReveal1()
    {
        UIManager.S.PlayCollectBetsAnimation();
        gameState = GameState.Reveal1;
        if (activePlayers.Count == 1)
        {
            InitDeclareWinner();
            return;
        }
        if (localPlayer.isIn) //if local player is in
        {
            maxTime = 25; //run timer for revealing
            timer = maxTime;
            UIManager.S.SetTimerPosition(localPlayer);
            UIManager.S.ToggleRevealPanel(true); //show reveal panel
            //local player needs to select 2 cards to reveal
            //local player needs to reveal selected cards
        }
        else
        {
            //ai players need to select 1 card to reveal
            //ai players need to reveal selected cards
            RevealAIPlayersCards(1);
            InitPlay();
        }
        lastPlayerToSpeak = finalPlayer; // set last player to speak to be the final player
        
    }

    public void SetCurrentPlayerToPlayerOnHisLeft()
    {
        if (activePlayers.IndexOf(currentPlayer) == activePlayers.Count - 1)
            currentPlayer = activePlayers[0];
        else
            currentPlayer = activePlayers[activePlayers.IndexOf(currentPlayer) + 1];
    }

    public void SetLastPlayerToSpeakToPlayerOnHisRight()
    {
        if (activePlayers.IndexOf(lastPlayerToSpeak) == 0)
            lastPlayerToSpeak = activePlayers[activePlayers.Count - 1];
        else
            lastPlayerToSpeak = activePlayers[activePlayers.IndexOf(lastPlayerToSpeak) - 1];
    }

    public void SetFinalPlayerToPlayerOnHisRight()
    {
        if (activePlayers.IndexOf(finalPlayer) == 0)
            finalPlayer = activePlayers[activePlayers.Count - 1];
        else
            finalPlayer = activePlayers[activePlayers.IndexOf(finalPlayer) - 1];
    }

    public PokerPlayer GetPlayerLeftOfFinalPlayer()
    {
        if (activePlayers.IndexOf(finalPlayer) == activePlayers.Count - 1)
            return activePlayers[0];
        else
            return activePlayers[activePlayers.IndexOf(finalPlayer) + 1];
    }

    public void InitPlay()
    {
        //Debug.Log("InitPlay");
        gameState = GameState.Play;

        //if this is the first round
        if (!dealer)
        {
            //set dealer
            int rand = Random.Range(0, activePlayers.Count);
            dealer = activePlayers[rand];
            UIManager.S.SetDealerPosition();
            finalPlayer = dealer;
            lastPlayerToSpeak = dealer;
            //set currentPlayer
            if (activePlayers.IndexOf(dealer) == activePlayers.Count - 1)
                currentPlayer = activePlayers[0];
            else
                currentPlayer = activePlayers[activePlayers.IndexOf(dealer) + 1];


            StartCoroutine(PlayGame()); // start the first round
            return;
        }

        //if no player has money left, open cards and declare the winner
        int totalSum = 0;
        for (int i = 0; i < activePlayers.Count; i++)
        {
            totalSum += activePlayers[i].totalAmount;
        }
        if (totalSum == 0)
        {
            foreach (PokerPlayer p in activePlayers)
            {
                foreach (CardPokerFace c in p.cards)
                {
                    if (!c.faceUp)
                        c.Flip();
                }
            }
            InitDeclareWinner();
            return;
        }

        // if this is NOT the first round

        int openCardsCounter = 0;
        // count the amount of open cards
        for (int i = 0; i < activePlayers[0].cards.Count; i++)
        {
            if (activePlayers[0].cards[i].faceUp)
                openCardsCounter++;
        }

        //if there are less than 4 open cards (not last round)
        if(openCardsCounter < 4)
        {
            if (currentPlayer == lastPlayerToSpeak)
            {
                //if the final player raised, change the final player and resume round
                if (lastPlayerToSpeak.command is RaiseCommand)
                {
                    SetLastPlayerToSpeakToPlayerOnHisRight();

                    //set the new current player 
                    SetCurrentPlayerToPlayerOnHisLeft();

                    StartCoroutine(PlayGame());
                    return;
                }
                //if the final player folded
                else if (lastPlayerToSpeak.command is FoldCommand)
                {
                    //SetLastPlayerToSpeakToPlayerOnHisRight();
                    UIManager.S.PlayCollectBetsAnimation(lastPlayerToSpeak);
                    if(lastPlayerToSpeak == finalPlayer)
                        SetFinalPlayerToPlayerOnHisRight();
                    lastPlayerToSpeak = finalPlayer;

                    //remove the old current player from the active list
                    RemoveFromActivePlayers(currentPlayer);
                    RemovePokerHand(currentPlayer);

                    //set the new current player
                    currentPlayer = GetPlayerLeftOfFinalPlayer();
                }
                //if the final player didnt raise or fold
                else
                {
                    //set the new current player
                    currentPlayer = GetPlayerLeftOfFinalPlayer();
                }
                InitReveal1(); //last player to speak finished his turn, time to reveal 1 card
                return;
            }
            else //if the current player is not the last player to speak
            {
                //if the final player raised, change the final player and resume round
                if (currentPlayer.command is RaiseCommand)
                {
                    //set the last player to speak to the player on the right of the currentPlayer... complicated
                    if (activePlayers.IndexOf(currentPlayer) == 0)
                        lastPlayerToSpeak = activePlayers[activePlayers.Count - 1];
                    else
                        lastPlayerToSpeak = activePlayers[activePlayers.IndexOf(currentPlayer) - 1];

                    //set the new current player 
                    SetCurrentPlayerToPlayerOnHisLeft();

                    StartCoroutine(PlayGame());
                    return;
                }
                //if the final player folded
                else if (currentPlayer.command is FoldCommand)
                {
                    PokerPlayer oldPlayer = currentPlayer; // reference to current player (who folded)
                    if (currentPlayer == finalPlayer)
                    {
                        SetFinalPlayerToPlayerOnHisRight();
                    }
                    //set the new current player
                    SetCurrentPlayerToPlayerOnHisLeft();

                    //remove the old current player from the active list
                    RemoveFromActivePlayers(oldPlayer);
                    RemovePokerHand(oldPlayer);
                    StartCoroutine(PlayGame());
                    return;
                }
                //if the current player didnt raise or fold
                else
                {
                    //set the new current player
                    SetCurrentPlayerToPlayerOnHisLeft();
                    StartCoroutine(PlayGame());
                    return;
                }
            }
        }
        //if this is the last round
        else //if there are 4 cards revealed
        {
            if (currentPlayer == lastPlayerToSpeak)
            {
                //if the final player raised, change the final player and resume round
                if (lastPlayerToSpeak.command is RaiseCommand)
                {
                    SetLastPlayerToSpeakToPlayerOnHisRight();

                    //set the new current player 
                    SetCurrentPlayerToPlayerOnHisLeft();
                    StartCoroutine(PlayGame());
                    return;
                }
                //if the last player to speak folded
                else if (lastPlayerToSpeak.command is FoldCommand)
                {
                    
                    UIManager.S.PlayCollectBetsAnimation(lastPlayerToSpeak);
                    
                    if (lastPlayerToSpeak == finalPlayer)
                        SetFinalPlayerToPlayerOnHisRight();

                    lastPlayerToSpeak = finalPlayer; 

                    //remove the old current player from the active list
                    RemoveFromActivePlayers(currentPlayer);
                    RemovePokerHand(currentPlayer);

                    //set the new current player
                    currentPlayer = GetPlayerLeftOfFinalPlayer();
                }
                foreach (PokerPlayer p in activePlayers)
                {
                    foreach (CardPokerFace c in p.cards)
                    {
                        if (!c.faceUp)
                            c.Flip();
                    }
                }
                InitDeclareWinner();//final player finished his turn, time to see who won
                return;
            }
            else //if the current player is not the last player to speak
            {
                //if the current player raised, change the final player and resume round
                if (currentPlayer.command is RaiseCommand)
                {
                    //set the last player to speak to the player on the right of the currentPlayer... complicated
                    if (activePlayers.IndexOf(currentPlayer) == 0)
                        lastPlayerToSpeak = activePlayers[activePlayers.Count - 1];
                    else
                        lastPlayerToSpeak = activePlayers[activePlayers.IndexOf(currentPlayer) - 1];

                    //set the new current player 
                    SetCurrentPlayerToPlayerOnHisLeft();

                    StartCoroutine(PlayGame());
                    return;
                }
                else if (currentPlayer.command is FoldCommand)
                {
                    PokerPlayer oldPlayer = currentPlayer; // reference to current player (who folded)

                    //set the new current player
                    SetCurrentPlayerToPlayerOnHisLeft();

                    if (currentPlayer == finalPlayer)
                    {
                        SetFinalPlayerToPlayerOnHisRight();
                    }

                    //remove the old current player from the active list
                    RemoveFromActivePlayers(oldPlayer);
                    RemovePokerHand(oldPlayer);
                    StartCoroutine(PlayGame());
                    return;
                }
                else
                {
                    //set the new current player 
                    SetCurrentPlayerToPlayerOnHisLeft();
                    StartCoroutine(PlayGame());
                    return;
                }
            }
        }
    }

    //Called every player's turn
    public IEnumerator PlayGame() 
    {
        //Debug.Log("PlayGame");
        yield return new WaitForEndOfFrame();
        //set the timer in the correct position
        //Debug.Log("currentPlayer: " + currentPlayer);
        maxTime = 25f;
        timer = maxTime;
        UIManager.S.ToggleSpark(true);
        UIManager.S.PlayPlayerImageEffect(currentPlayer.id);
        UIManager.S.SetTimerPosition(currentPlayer);
        if(currentPlayer is AIPokerPlayer)
        {
            float delay = Random.Range(1f, 7f); //delay
            yield return new WaitForSeconds(delay);
            AIPokerPlayer aip = currentPlayer as AIPokerPlayer;
            if (aip)
            {
                aip.CalculateMove(); //calculate move
                aip.Action(); //action 
            }
            //Debug.Log(aip.name + " " + aip.command);
            //play relevant animation
            InitPlay();
        }
        else
        {
            //run play timer

            UIManager.S.TogglePlayPanel(true); //show play panel
            UIManager.S.ToggleRaisePanel(false);
            //local player needs to select wanted command -- done via UI
            //action -- done via UI
            //play relevant animation
        }
    }

    public void AddToActivePlayers(PokerPlayer p)
    {
        if (!activePlayers.Contains(p))
            activePlayers.Add(p);
    }

    public void RemoveFromActivePlayers(PokerPlayer p)
    {
        if (activePlayers.Contains(p))
            activePlayers.Remove(p);
    }

    public void AddPokerHand(PokerPlayer p)
    {
        if (!_activePlayersHands.ContainsKey(p))
            _activePlayersHands.Add(p, p.pokerHand);
    }

    public void RemovePokerHand(PokerPlayer p)
    {
        if (_activePlayersHands.ContainsKey(p))
            _activePlayersHands.Remove(p);
    }

    public void InitDeclareWinner()
    {
        Debug.Log("InitDeclareWinner");
        StartCoroutine(InitDeclareWinnerSequence());
    }

    public IEnumerator InitDeclareWinnerSequence()
    {
        gameState = GameState.DeclareWinner;
        
        UIManager.S.PlayCollectBetsAnimation();
        foreach (PokerPlayer pp in activePlayers)
        {
            foreach (CardPokerFace c in pp.cards)
            {
                if (!c.faceUp)
                    c.Flip();
                if(pp == localPlayer)
                {
                    c.transform.DOLocalMoveY(c.unselectedYPos, 0.25f);
                }
            }
            
            Debug.Log("Check before calculating winners - name: " + pp.name + " hand: " + pp.pokerHand.playerHand.ToString());
        }
        //compare the hands of all the players
        //determine who is/are the winners
        //add winners to winner list
        winners = CalculateWinners();
        if(winners.Count > 1)
        {
            foreach (PokerPlayer p in winners)
            {
                UIManager.S.PlayCollectWinningsAnimation(p);
                UIManager.S.PlayWinningFX(p);
            }
        }
        else
        {
            //play animation of paying the pot
            UIManager.S.PlayCollectWinningsAnimation(winners[0]);
            UIManager.S.PlayWinningFX(winners[0]);
        }

        //wait for player to inspect the cards
        gameState = GameState.Inspection;
        inspectionOver = false;
        UIManager.S.ToggleSpark(false);
        UIManager.S.SetTimerPosition(localPlayer);
        UIManager.S.ToggleInspectionPanel(true);
        ResetTimer();
        UIManager.S.ToggleSpark(true);
        yield return new WaitUntil(() => inspectionOver == true);

        gameState = GameState.Standby;

        //delay few seconds
        yield return new WaitForSeconds(1f);

        //if localplayer.totalAmount < buyInAmount
        if (localPlayer.totalAmount < buyInAmount)
            InitGameOver(); // game over
        else
        {
            ClearAll(); //clear all existing cards
            InitGame(); // restart game
            InitBuyIn(); //first bet
        }
    }

    public void ClearAll()
    {
        Debug.Log("ClearAll");
        gameState = GameState.Standby;
        layout.slotDefs.Clear();
        foreach (Card c in deck.cards)
        {
            Destroy(c.gameObject);
        }
        foreach (PokerPlayer p in _players)
        {
            if(p.cards.Count > 0)
            {
                foreach (CardPokerFace cpf in p.cards)
                {
                    Destroy(cpf.gameObject);
                    
                }
            }
            p.cards.Clear();
            p.command = null;
            if (p is AIPokerPlayer)
            {
                AIPokerPlayer a = p as AIPokerPlayer;
                if (a.totalAmount <= 50)
                    a.totalAmount = 5000;
            }
        }
        foreach (CardButton cb in UIManager.S.cardButtons)
        {
            if (cb.GetUICard())
            {
                Destroy(cb.GetUICard().gameObject);
                cb.SetUICard(null);
                cb.SetPlayerCard(null);
            }
        }
        activePlayers.Clear();
        winners.Clear();
        _activePlayersHands.Clear();
        table.Clear();
        pot = 0;
        amountToBet = 0;
        localPlayer = _players[0];
        dealer = null;
        currentPlayer = null;
    }

    public void InitGameOver()
    {
        gameState = GameState.Over;
        Debug.Log("InitGameOver");
        UIManager.S.ToggleEndGamePanel(true);
        //show endGame panel
        //local player needs to select to restart or quit
    }

    private void AddToPot(int amount)
    {
        pot += amount;
    }

    public void SetAmountToBet(int newAmount)
    {
        amountToBet = newAmount;
    }

    public void CollectBets()
    {
        foreach (PokerPlayer p in activePlayers)
        {
            AddToPot(p.betAmount);
            p.betAmount = 0;
        }
        SetAmountToBet(0);
    }

    public void CollectBet(PokerPlayer p)
    {
        AddToPot(p.betAmount);
        p.betAmount = 0;
    }

    public void CollectWinnings()
    {
        if (pot > 0)
        {
            pot /= winners.Count;
            for (int i = 0; i < winners.Count; i++)
            {
                winners[i].totalAmount += pot;
            }
            pot = 0;
        }
        else
            return;
    }

    public List<PokerPlayer> CalculateWinners()
    {
        return PokerHand.Evaluate(_activePlayersHands);
    }

    public void RevealAIPlayersCards(int cardAmount)
    {
        //flip the AI players cards
        foreach (PokerPlayer p in _players)
        {
            AIPokerPlayer aip = p as AIPokerPlayer;
            List<CardPokerFace> cardsToReveal = new List<CardPokerFace>();
            if (aip)
            {
                foreach (CardPokerFace cpf in aip.cards)
                {
                    if (!cpf.faceUp && !cpf.selected)
                        cardsToReveal.Add(cpf);
                }

                for (int i = 0; i < cardAmount; i++)
                {
                    Randomize:
                    int rand = Random.Range(0, cardsToReveal.Count);
                    if (!cardsToReveal[rand].selected)
                    {
                        aip.SelectCard(cardsToReveal[rand]);
                        cardsToReveal[rand].selected = true;
                    }
                    else
                        goto Randomize;
                }
                aip.SetCommand(new RevealCardsCommand()); //ai players need to select 2 cards to reveal
                aip.Action(); //ai players need to reveal selected cards
            }
        }
    }

    IEnumerator RevealAIPlayersCards(int cardAmount, float delay)
    {
        yield return new WaitForSeconds(delay);
        //flip the AI players cards
        foreach (PokerPlayer p in _players)
        {
            AIPokerPlayer aip = p as AIPokerPlayer;
            List<CardPokerFace> cardsToReveal = new List<CardPokerFace>();
            if (aip)
            {
                foreach (CardPokerFace cpf in aip.cards)
                {
                    if (!cpf.faceUp && !cpf.selected)
                        cardsToReveal.Add(cpf);
                }
                for (int i = 0; i < 2; i++)
                {
                    Randomize:
                    int rand = Random.Range(0, cardsToReveal.Count);
                    if (!cardsToReveal[rand].selected)
                    {
                        aip.SelectCard(cardsToReveal[rand]);
                        cardsToReveal[rand].selected = true;
                    }
                    else
                        goto Randomize;
                }
                aip.SetCommand(new RevealCardsCommand()); //ai players need to select 2 cards to reveal
                aip.Action(); //ai players need to reveal selected cards
                              //play reveal cards animation
            }
        }
    }

    public void PlaySound(SoundType st)
    {
        audioSource.clip = SoundManager.S.GetSoundByName(st);
        audioSource.Play();
    }

    public void SetGameState(GameState gs)
    {
        gameState = gs;
    }

    public void BuyMoreTime()
    {
        ResetTimer();
        UIManager.S.isBuyTimeButtonFlashing = false;
    }

    public void ResetTimer()
    {
        timer = maxTime;
    }
    
    #endregion

}
