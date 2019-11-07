using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {

    #region Fields
    static public UIManager S;
    public Camera mainCamera;
    public Camera canvasCamera;
    public CardButton[] cardButtons;
    [SerializeField] private Image _turnTimer;
    [SerializeField] private CanvasGroup _buyInPanel;
    [SerializeField] private CanvasGroup _swapPanel;
    [SerializeField] private Text _swapButtonText;
    [SerializeField] private CanvasGroup _revealPanel;
    [SerializeField] private CanvasGroup _playPanel;
    [SerializeField] private CanvasGroup _raisePanel;
    [SerializeField] private CanvasGroup _confirmRaiseButton;
    [SerializeField] private CanvasGroup _messagePanel;
    [SerializeField] private CanvasGroup _endGamePanel;
    [SerializeField] private CanvasGroup _cardInspectionPanel;
    [SerializeField] private Button _swapButton;
    [SerializeField] private Button _revealButton;
    [SerializeField] private Slider _raiseSlider;
    [SerializeField] private Text _raiseText;
    [SerializeField] private Text _messageText;
    [SerializeField] private Text _potText;
    [SerializeField] private Text _movingBetText;
    [SerializeField] private Text[] _betTexts;
    [SerializeField] private RectTransform[] _betTextsBGs;
    [SerializeField] private Text[] _totalTexts;
    [SerializeField] private Image[] _playerImages;
    [SerializeField] private Text _checkCallAllinButtonText;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Transform _sparkAnchor;
    [SerializeField] private ParticleSystem _spark;
    [SerializeField] private ParticleSystem _winnerFX;
    [SerializeField] private Transform _dealerChip;
    [SerializeField] private Vector3[] _dealerPositions;
    [SerializeField] private Vector3[] _timerPositions;
    [SerializeField] private Vector3[] _winnerFXPositions;
    [SerializeField] [TextArea(3, 5)] private string[] _messages;
    public Button[] buyTimeButtons;
    public bool isBuyTimeButtonFlashing = false;
    private PokerPlayer _localPlayer;
    #endregion

    #region MonoBehaviour Callbacks
    private void Awake()
    {
        if (S != null)
            Destroy(gameObject);
        S = this;
    }

    private void Start()
    {
        _localPlayer = PokerFaceManager.S.localPlayer;
        ToggleUIElement(_playPanel, false, true);
        ToggleUIElement(_raisePanel, false, true);
        ToggleUIElement(_confirmRaiseButton, false, true);
        ToggleUIElement(_swapPanel, false, true);
        ToggleUIElement(_revealPanel, false, true);
        ToggleUIElement(_messagePanel, false, true);
        ToggleUIElement(_endGamePanel, false, true);
        ToggleUIElement(_cardInspectionPanel, false, true);
        ToggleCardButtonFlash(false);
        _winnerFX.gameObject.SetActive(false);
        _movingBetText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (PokerFaceManager.S.gameState == GameState.Inspection || PokerFaceManager.S.gameState == GameState.Play || PokerFaceManager.S.gameState == GameState.Swap || PokerFaceManager.S.gameState == GameState.BuyIn || PokerFaceManager.S.gameState == GameState.Reveal1 || PokerFaceManager.S.gameState == GameState.Reveal2)
        {
            _turnTimer.fillAmount = PokerFaceManager.S.timer / PokerFaceManager.S.maxTime;
            if (_turnTimer.fillAmount < 0.25f && !_audioSource.isPlaying)
                PlaySound(SoundType.Tick);
            if(_turnTimer.fillAmount < 0.25f && !isBuyTimeButtonFlashing)
            {
                isBuyTimeButtonFlashing = true;
                Button bt = _buyInPanel.transform.GetChild(3).GetComponent<Button>();
                switch (PokerFaceManager.S.gameState)
                {
                    case GameState.Standby:
                        break;
                    case GameState.Swap:
                        bt = _swapPanel.transform.GetChild(1).GetComponent<Button>();
                        break;
                    case GameState.Play:
                        bt = _playPanel.transform.GetChild(3).GetComponent<Button>();
                        break;
                    case GameState.FirstRound:
                        break;
                    case GameState.SecondRound:
                        break;
                    case GameState.FinalRound:
                        break;
                    case GameState.Over:
                        break;
                    case GameState.BuyIn:
                        break;
                    case GameState.Reveal2:
                    case GameState.Reveal1:
                        bt = _revealPanel.transform.GetChild(1).GetComponent<Button>();
                        break;
                    case GameState.DeclareWinner:
                        break;
                    case GameState.Inspection:
                        bt = _cardInspectionPanel.transform.GetChild(1).GetComponent<Button>();
                        break;
                    default:
                        break;
                }
                ToggleBuyTimeButtonFlash(true, bt);
            }
            else if(_turnTimer.fillAmount >= 0.25f && isBuyTimeButtonFlashing)
            {
                isBuyTimeButtonFlashing = false;
            }
        }
    }

    #endregion

    #region Methods
    public void SetTimerPosition(PokerPlayer p)
    {
        _turnTimer.transform.localPosition = _timerPositions[p.id];
    }

    private void InitBuyInPanel()
    {
        ToggleSpark(true);
        Button[] buyInPanelButtons = _buyInPanel.GetComponentsInChildren<Button>();
        buyInPanelButtons[0].onClick.RemoveAllListeners();
        buyInPanelButtons[1].onClick.RemoveAllListeners();
        buyInPanelButtons[2].onClick.RemoveAllListeners();
        buyInPanelButtons[0].onClick.AddListener(() =>
        {
            _localPlayer.SetCommand(new BuyInCommand()); //buy in
            _localPlayer.Action();
            ToggleSpark(false);
            PokerFaceManager.S.AddToActivePlayers(_localPlayer); // add local player to active players
            StartCoroutine(CollectBetsAnimation()); //collect bets animation
            StartCoroutine(PokerFaceManager.S.InitSwap());
            ToggleBuyInPanel(false);

        });
        buyInPanelButtons[1].onClick.AddListener(() =>
        {
            _localPlayer.SetCommand(new FoldCommand()); // fold
            _localPlayer.Action();
            PokerFaceManager.S.RemoveFromActivePlayers(_localPlayer);
            PokerFaceManager.S.RemovePokerHand(_localPlayer);
            ToggleSpark(false);
            StartCoroutine(PokerFaceManager.S.InitSwap());
            ToggleBuyInPanel(false);
        });
        buyInPanelButtons[2].onClick.AddListener(()=>
        {
            _localPlayer.SetCommand(new BuyTimeCommand());
            _localPlayer.Action();
        });
    }

    public void ToggleBuyInPanel(bool on)
    {
        if (on)
            InitBuyInPanel();
        ToggleUIElement(_buyInPanel, on);
    }

    private void InitSwapPanel()
    {
        ToggleSpark(true);
        ToggleCardButtonFlash(true);
        ToggleMessagePanel(true, GameState.Swap,_messages[0], 1f);
        _swapButton.transform.GetChild(1).GetComponent<Text>().text = "הפלחה אלל ךשמה";
        _swapButton.onClick.RemoveAllListeners();
        _swapButton.onClick.AddListener(() =>
        {
            //change selected cards
            int delay = _localPlayer.selectedCards.Count;
            PokerFaceManager.S.ResetTimer();
            _localPlayer.SetCommand(new ChangeCardsCommand()); // swap selected cards
            _localPlayer.Action();
            ToggleSpark(false);
            ToggleCardButtonFlash(false);
            foreach (CardPokerFace c in _localPlayer.cards)
            {
                cardButtons[c.index].ChangePlayerCard(c); //change the card on the correct card button
            }
            PokerFaceManager.S.InitReveal2(delay); //
            ToggleSwapPanel(false);
        });
        Button buyTimeButton = _swapPanel.transform.GetChild(1).GetComponent<Button>();
        buyTimeButton.onClick.RemoveAllListeners();
        buyTimeButton.onClick.AddListener(() =>
        {
            _localPlayer.SetCommand(new BuyTimeCommand());
            _localPlayer.Action();
        });
    }

    public void ToggleSwapPanel(bool on)
    {
        if (on)
            InitSwapPanel();
        ToggleUIElement(_swapPanel, on);
    }

    public void SetSwapButtonText()
    {
        if (CardButton.selectedCards > 0)
            _swapButtonText.text = "ףלחה";
        else
            _swapButtonText.text = "הפלחה אלל ךשמה";
    }

    private void InitRevealPanel(int amountToReveal)
    {
        ToggleSpark(true);
        ToggleCardButtonFlash(true);
        _revealButton.interactable = false;
        ToggleButtonHalo(_revealButton, false);
        if (amountToReveal == 2)
        {
            _revealButton.transform.GetChild(1).GetComponent<Text>().text = "םיפלק ינש רחב";
            ToggleMessagePanel(true, GameState.Reveal2,_messages[1], 1f);
        }
        else
        {
            _revealButton.transform.GetChild(1).GetComponent<Text>().text = "ףלק רחב";
            ToggleMessagePanel(true, GameState.Reveal1, _messages[2], 1f);
        }

        _revealButton.onClick.RemoveAllListeners();
        _revealButton.onClick.AddListener(() =>
        {

            //reveal selected cards
            _localPlayer.SetCommand(new RevealCardsCommand()); // swap selected cards
            _localPlayer.Action();
            ToggleSpark(false);
            ToggleCardButtonFlash(false);
            if (PokerFaceManager.S.gameState == GameState.Reveal1)
                PokerFaceManager.S.RevealAIPlayersCards(1);
            ToggleRevealPanel(false);
            if (amountToReveal == 2)
                PokerFaceManager.S.InitPlay();
            else
                StartCoroutine(PokerFaceManager.S.PlayGame());
        });
        Button buyTimeButton = _revealPanel.transform.GetChild(1).GetComponent<Button>();
        buyTimeButton.onClick.RemoveAllListeners();
        buyTimeButton.onClick.AddListener(() =>
        {
            _localPlayer.SetCommand(new BuyTimeCommand());
            _localPlayer.Action();
        });

    }

    public void ToggleRevealPanel(bool on, int amtToReveal = 1)
    {
        if (on)
            InitRevealPanel(amtToReveal);
        ToggleUIElement(_revealPanel, on);
    }

    private void InitPlayPanel()
    {
        //spark is activated from PokerFaceManager (PlayGame())
        Button[] playButtons = _playPanel.GetComponentsInChildren<Button>();
        foreach (Button b in playButtons)
        {
            b.onClick.RemoveAllListeners();
        }

        //setting the correct text to call/check button
        if (PokerFaceManager.S.amountToBet > 0)
        {
            if (PokerFaceManager.S.amountToBet - (_localPlayer.totalAmount + _localPlayer.betAmount) < 0)
                _checkCallAllinButtonText.text = "הוושה";
            else
                _checkCallAllinButtonText.text = "םינפב לכה";
        }
        else
            _checkCallAllinButtonText.text = "ראשיה";

        playButtons[2].onClick.AddListener(() =>
        {
            _localPlayer.SetCommand(new FoldCommand());
            _localPlayer.Action();
            TogglePlayPanel(false);
            ToggleSpark(false);
            PokerFaceManager.S.InitPlay();
        });
        playButtons[1].onClick.AddListener(() =>
        {
            if (PokerFaceManager.S.amountToBet > 0)
            {
                if (PokerFaceManager.S.amountToBet - (_localPlayer.totalAmount + _localPlayer.betAmount) < 0)
                    _localPlayer.SetCommand(new CallCommand());
                else
                    _localPlayer.SetCommand(new AllInCommand());
            }
            else
                _localPlayer.SetCommand(new CheckCommand());

            _localPlayer.Action();
            TogglePlayPanel(false);
            ToggleSpark(false);
            PokerFaceManager.S.InitPlay();
        });
        playButtons[0].onClick.AddListener(() =>
        {
            //_raiseSlider.minValue = PokerFaceManager.S.amountToBet > 0 ? PokerFaceManager.S.amountToBet : 1;
            //_raiseSlider.maxValue = _localPlayer.totalAmount;
            //ToggleRaisePanel(true);
            //_raiseText.text = string.Format("{0}$", _raiseSlider.value);
            ToggleRaisePanel(true);

        });
        playButtons[3].onClick.AddListener(() =>
        {
            _localPlayer.SetCommand(new BuyTimeCommand());
            _localPlayer.Action();

        });

        
        
    }

    public void TogglePlayPanel(bool on)
    {
        if (on)
            InitPlayPanel();
        ToggleUIElement(_playPanel, on);
    }

    private void InitRaisePanel()
    {
        //_raiseSlider.value = 1;
        TogglePlayPanel(false);
        Button[] raiseButtons = _raisePanel.GetComponentsInChildren<Button>();
        raiseButtons[0].onClick.RemoveAllListeners();
        raiseButtons[0].onClick.AddListener(() =>
        {
            _localPlayer.SetCommand(new DecreaseBetCommand());
            _localPlayer.Action();
        });
        raiseButtons[1].onClick.RemoveAllListeners();
        raiseButtons[1].onClick.AddListener(() =>
        {
            _localPlayer.SetCommand(new IncreaseBetCommand());
            _localPlayer.Action();
        });

        Button confirmRaiseButton = _confirmRaiseButton.GetComponent<Button>();
        confirmRaiseButton.onClick.RemoveAllListeners();
        confirmRaiseButton.onClick.AddListener(() =>
        {
            //Debug.Log("Confirm Raise pressed");
            _localPlayer.SetCommand(new RaiseCommand());
            _localPlayer.Action();
            ToggleRaisePanel(false);
            ToggleSpark(false);
            //ToggleCameras(false);
            PokerFaceManager.S.InitPlay();
        });
    }

    public void ToggleRaisePanel(bool on)
    {
        if (on)
            InitRaisePanel();
        ToggleUIElement(_raisePanel, on);
        ToggleUIElement(_confirmRaiseButton, on);
        //ToggleCameras(on);
    }

    private void InitInspectionPanel()
    {
        Button continueButton = _cardInspectionPanel.transform.GetChild(0).GetComponent<Button>();
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() =>
        {
            ToggleInspectionPanel(false);
            ToggleSpark(false);
            PokerFaceManager.S.inspectionOver = true;
        });
        Button buyTimeButton = _cardInspectionPanel.transform.GetChild(1).GetComponent<Button>();
        buyTimeButton.onClick.RemoveAllListeners();
        buyTimeButton.onClick.AddListener(() =>
        {
            _localPlayer.SetCommand(new BuyTimeCommand());
            _localPlayer.Action();
        });
    }

    public void ToggleInspectionPanel(bool on)
    {
        if (on)
            InitInspectionPanel();
        ToggleUIElement(_cardInspectionPanel, on);
    }

    public void InitMessagePanel(string str, GameState gs)
    {
        _messageText.text = "";
        _messageText.DOText(str, 1f);
        Button b = _messagePanel.transform.GetChild(1).GetComponent<Button>();
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() =>
        {
            ToggleMessagePanel(false, gs);

        });
    }

    public void ToggleMessagePanel(bool on, GameState gs, string str = "", float delay = 0)
    {
        if (delay == 0)
        {
            if (on)
            {
                InitMessagePanel(str, gs);
                PokerFaceManager.S.SetGameState(GameState.Standby);
            }
            else
                PokerFaceManager.S.SetGameState(gs);
                
            ToggleUIElement(_messagePanel, on);
            ToggleCameras(on);
        }
        else
        {
            StartCoroutine(OpenMesagePanelWithDelay(delay, on, gs, str));
        }
            
    }

    IEnumerator OpenMesagePanelWithDelay(float delay, bool on, GameState gs, string str)
    {
        yield return new WaitForSeconds(delay);
        if (on)
        {
            InitMessagePanel(str, gs);
            PokerFaceManager.S.SetGameState(GameState.Standby);
        }
        else
        {
            PokerFaceManager.S.SetGameState(gs);
        }
            
        ToggleUIElement(_messagePanel, on);
        ToggleCameras(on);
    }

    public void InitEndGamePanel()
    {
        Button[] endGameButtons = new Button[] { _endGamePanel.transform.GetChild(0).GetComponent<Button>(), _endGamePanel.transform.GetChild(1).GetComponent<Button>() };
        endGameButtons[0].onClick.RemoveAllListeners();
        endGameButtons[1].onClick.RemoveAllListeners();
        endGameButtons[0].onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
        endGameButtons[1].onClick.AddListener(() => Application.Quit());
    }

    public void ToggleEndGamePanel(bool on)
    {
        if (on)
            InitEndGamePanel();
        ToggleUIElement(_endGamePanel, on);
        ToggleCameras(on);
    }

    public void SetAmountToRaise()
    {
        _raiseText.text = string.Format("{0}$", (int)_raiseSlider.value);
        _localPlayer.raiseCashAmount = ((int)_raiseSlider.value);
    }

    public void ToggleRevealButtonInteraction(bool on)
    {
        ToggleButtonInteraction(_revealButton, on);
        ToggleButtonHalo(_revealButton, on);
    }

    private void ToggleButtonInteraction(Button b, bool on)
    {
        b.interactable = on;
    }
    #endregion

    #region Animation and Tween Methods
    private void ToggleUIElement(CanvasGroup element, bool show, bool immediate = false)
    {

        if (immediate)
        {
            if (show)
            {
                element.gameObject.SetActive(true);
                element.DOFade(1f, 0f);
                element.blocksRaycasts = true;
            }
            else
            {
                element.DOFade(0f, 0f);
                element.blocksRaycasts = false;
                element.gameObject.SetActive(false);
            }
        }
        else
        {
            if (show)
            {
                element.gameObject.SetActive(true);
                element.DOFade(1f, 0.5f);
                element.blocksRaycasts = true;
            }
            else
            {
                element.DOFade(0f, 0.5f);
                element.blocksRaycasts = false;
                element.gameObject.SetActive(false);
            }
        }
    }

    public void PlayCollectBetsAnimation()
    {
        StartCoroutine(CollectBetsAnimation());
    }

    IEnumerator CollectBetsAnimation()
    {
        yield return null;

        Queue<Vector3> betTextPositions = new Queue<Vector3>();
        foreach (Text t in _betTexts)
        {
            if (t.gameObject.activeSelf)
            {
                betTextPositions.Enqueue(t.transform.position);
                t.transform.DOMove(_potText.transform.position, 0.5f);
            }
        }

        PlaySound(SoundType.Coin);
        yield return new WaitForSeconds(0.5f);
        PokerFaceManager.S.CollectBets();
        for (int i = 0; i < PokerFaceManager.S.activePlayers.Count; i++)
        {
            SetTotalAmountText(PokerFaceManager.S.activePlayers[i]);
            SetBetAmountText(PokerFaceManager.S.activePlayers[i]);
        }
        SetPotText(PokerFaceManager.S.pot);
        foreach (Text t in _betTexts)
        {
            if (t.gameObject.activeSelf)
            {
                t.transform.position = betTextPositions.Dequeue();
            }
        }
    }

    public void PlayCollectBetsAnimation(PokerPlayer p)
    {
        StartCoroutine(CollectBetsAnimation(p));
    }

    IEnumerator CollectBetsAnimation(PokerPlayer p)
    {
        Vector3 betTextPosition = _betTexts[p.id].transform.localPosition;
        _betTexts[p.id].transform.DOLocalMove(_potText.transform.localPosition, 0.5f);
        PlaySound(SoundType.Coin);
        yield return new WaitForSeconds(0.5f);
        PokerFaceManager.S.CollectBet(p); 
        SetTotalAmountText(p);
        SetBetAmountText(p);
        SetPotText(PokerFaceManager.S.pot);
        _betTexts[p.id].transform.localPosition = betTextPosition;

    }

    public void PlayCollectWinningsAnimation(PokerPlayer p)
    {
        StartCoroutine(CollectWinningsAnimation(p));
    }

    IEnumerator CollectWinningsAnimation(PokerPlayer p)
    {
        yield return new WaitForSeconds(2f);
        Vector3 totalTextPos = _totalTexts[p.id].transform.position;
        _totalTexts[p.id].transform.position = _potText.transform.position;
        PlaySound(SoundType.Coin);
        _totalTexts[p.id].transform.DOMove(totalTextPos, 1f);
        yield return new WaitForSeconds(1f);
        PokerFaceManager.S.CollectWinnings();
        SetTotalAmountText(p);
        SetPotText(PokerFaceManager.S.pot);

    }

    public void ToggleSpark(bool on)
    {
        _sparkAnchor.gameObject.SetActive(on);
        _spark.gameObject.SetActive(on);
        if (on)
            StartCoroutine(SparkEffectAnimation());
        else
        {
            StopCoroutine(SparkEffectAnimation());
        }
    }

    private IEnumerator SparkEffectAnimation()
    {
        DOTween.Kill(_sparkAnchor);
        _sparkAnchor.localRotation = Quaternion.identity;
        _spark.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        _spark.Play(false);
        Animation:
        while (PokerFaceManager.S.gameState == GameState.Inspection || PokerFaceManager.S.gameState == GameState.Play || PokerFaceManager.S.gameState == GameState.Swap || PokerFaceManager.S.gameState == GameState.BuyIn || PokerFaceManager.S.gameState == GameState.Reveal1 || PokerFaceManager.S.gameState == GameState.Reveal2)
        {
            _sparkAnchor.DOLocalRotate(new Vector3(0f, 0f, _sparkAnchor.transform.localRotation.eulerAngles.z - (360f / PokerFaceManager.S.maxTime) * Time.deltaTime), Time.deltaTime).SetEase(Ease.Linear);
            yield return new WaitForEndOfFrame();
        }
        while (PokerFaceManager.S.gameState == GameState.Standby)
        {
            yield return new WaitForEndOfFrame();
        }
        if (PokerFaceManager.S.gameState == GameState.Play || PokerFaceManager.S.gameState == GameState.Swap || PokerFaceManager.S.gameState == GameState.BuyIn || PokerFaceManager.S.gameState == GameState.Reveal1 || PokerFaceManager.S.gameState == GameState.Reveal2)
            goto Animation;
    }

    public void SetDealerPosition()
    {
        _dealerChip.transform.localPosition = _dealerPositions[PokerFaceManager.S.dealer.id];
    }

    public void SetPotText(int potAmount)
    {
        _potText.text = string.Format("{0}$", potAmount);
    }

    public void SetBetAmountText(PokerPlayer p)
    {
        _betTexts[p.id].text = string.Format("{0}$", p.betAmount);
        /*Sequence seq = DOTween.Sequence();
        seq.Append(_betTextsBGs[p.id].DOScaleX(0, 0.25f).OnComplete(() => _betTexts[p.id].text = string.Format("{0}$", p.betAmount)));
        seq.Append(_betTextsBGs[p.id].DOScaleX(1, 0.25f));
        seq.Play();*/
    }

    public void SetTotalAmountText(PokerPlayer p)
    {
        _totalTexts[p.id].text = string.Format("{0}$", p.totalAmount);
    }

    public void PlaySound(SoundType st)
    {
        _audioSource.clip = SoundManager.S.GetSoundByName(st);
        _audioSource.Play();
    }

    public void ToggleCardButtonFlash(bool on)
    {
        foreach (CardButton cb in cardButtons)
        {
            if (cb.GetPlayerCard() && !cb.GetPlayerCard().faceUp)
            {
                cb.transform.GetChild(1).gameObject.SetActive(on);
            }
            else
            {
                cb.transform.GetChild(1).gameObject.SetActive(false);
            }
        }
    }

    public void ToggleCameras(bool messageMode)
    {
        if (messageMode)
        {
            mainCamera.depth = 0;
            canvasCamera.depth = 1;
        }
        else
        {
            mainCamera.depth = 1;
            canvasCamera.depth = 0;
        }
    }

    public void PlayPlayerImageEffect(int pid)
    {
        _playerImages[pid].transform.DORotate(Vector3.up * 1080f, 1f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic);
    }

    public void PlayWinningFX(PokerPlayer p)
    {
        StartCoroutine(WinningFX(p));
    }

    IEnumerator WinningFX(PokerPlayer p)
    {
        yield return new WaitForSeconds(2f);
        _winnerFX.transform.position = _winnerFXPositions[p.id];
        _winnerFX.gameObject.SetActive(true);
        PlaySound(SoundType.ManyCoins);
        PokerFaceManager.S.PlaySound(SoundType.Applause);
        yield return new WaitForSeconds(5f);
        _winnerFX.gameObject.SetActive(false);
    }

    private void ToggleButtonHalo(Button btn, bool on)
    {
        btn.transform.GetChild(0).gameObject.SetActive(on);
    }

    public void PlaySliderSound()
    {
        PlaySound(SoundType.Coin2);
    }

    public void ToggleCardButtons(bool on)
    {
        foreach (CardButton cb in cardButtons)
        {
            cb.gameObject.SetActive(on);
        }
    }

    public void PlayMovingBetSequence(PokerPlayer p)
    {
        _movingBetText.transform.localPosition = _totalTexts[p.id].transform.localPosition;
        //Debug.Log("<color=yellow>original pos: " + _totalTexts[p.id].transform.localPosition + "</color>");
        _movingBetText.text = string.Format("{0}$", p.betAmount.ToString());
        _movingBetText.DOFade(0f, 0.01f);
        _movingBetText.gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(_movingBetText.DOFade(1f, 0.25f).OnComplete(() => SetTotalAmountText(p)));
        //Debug.Log("<color=blue>wanted pos: " + _betTexts[p.id].transform.localPosition + "</color>");
        seq.Append(_movingBetText.transform.DOLocalMove(_betTexts[p.id].transform.parent.localPosition, 0.35f).OnPlay(() =>
        {
            if (p == _localPlayer)
                PlaySound(SoundType.Coin4);
            else
                PlaySound(SoundType.Coin);
            SetBetAmountText(p);
            //Debug.Log("<color=green>wanted pos: " + _movingBetText.transform.localPosition + "</color>");
        }));
        seq.Append(_movingBetText.DOFade(0f, 0.25f).OnComplete(() => SetTotalAmountText(p)));
        seq.Play();
        
        
    }

    public void ToggleBuyTimeButtonFlash(bool on, Button btb)
    {
        if (on)
            StartCoroutine(BuyTimeButtonFlashSequence(btb));
        else
        {
            StopCoroutine(BuyTimeButtonFlashSequence(btb));
            btb.image.DOFade(1, 0.5f).OnPlay(() => btb.transform.GetChild(0).GetComponent<Text>().DOColor(Color.black, 0.5f));
        }
            
    }

    IEnumerator BuyTimeButtonFlashSequence(Button btb)
    {
        while (isBuyTimeButtonFlashing)
        {
            btb.image.DOFade(0, 0.5f).OnStart(() => btb.transform.GetChild(1).GetComponent<Text>().DOColor(Color.white, 0.5f));
            yield return new WaitForSeconds(0.5f);
            btb.image.DOFade(1, 0.5f).OnStart(() => btb.transform.GetChild(1).GetComponent<Text>().DOColor(Color.black, 0.5f));
            yield return new WaitForSeconds(0.5f);
        }
    }

#endregion

}
