using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PokerPlayer : MonoBehaviour {

    #region Fields
    public int id;
    #endregion

    #region Properties
    [SerializeField] private int _totalAmount;
    public int totalAmount
    {
        get
        {
            return _totalAmount;
        }
        set
        {
            if (value >= 0)
                _totalAmount = value;
            else
                Debug.LogError("Player " + name + "'s totalAmount cannot be less than 0");
        }
    }
    [SerializeField] private int _betAmount;
    public int betAmount
    {
        get
        {
            return _betAmount;
        }
        set
        {
            if (value >= 0)
                _betAmount = value;
        }
    }
    [SerializeField] private int _raiseCashAmount;
    public int raiseCashAmount
    {
        get
        {
            return _raiseCashAmount;
        }
        set
        {
            if (value >= 1 && value <= totalAmount)
                _raiseCashAmount = value;
        }
    }
    #endregion


    public bool isIn;
    public List<CardPokerFace> cards = new List<CardPokerFace>();
    public List<CardPokerFace> selectedCards;
    public PokerHand pokerHand;
    [SerializeField] public Command command;

    public void InitStats(int _total)
    {
        totalAmount = _total;
        betAmount = 0;
        raiseCashAmount = 1;
        isIn = false;
        selectedCards = new List<CardPokerFace>();
    }

    public void InitCards()
    {
        pokerHand = new PokerHand(cards[0], cards[1], cards[2], cards[3], cards[4]);
        PokerFaceManager.S.AddPokerHand(this);
        CalculateHandStrength();
    }

    public void CalculateHandStrength()
    {
        var len = Enum.GetValues(typeof(HandType)).Length;
        for (var handType = HandType.RoyalFlush; (int)handType < len; handType = handType + 1)
        {
            var hand = pokerHand;
            if (hand.IsValid(handType))
            {
                pokerHand.playerHand = handType;
                //Debug.Log(name + " " + pokerHand.playerHand);
                break;
            }
        }
    }

    public void Action()
    {
        command.Execute(this);
    }

    public void SetCommand(Command newCommand)
    {
        command = newCommand;
    }

    public void SelectCard(CardPokerFace card)
    {
        selectedCards.Add(card);
        CardButton.selectedCards++;
    }

    public void DeselectCard(CardPokerFace card)
    {
        selectedCards.Remove(card);
        CardButton.selectedCards--;
    }

    public void ToggleCards(bool on)
    {
        if(cards.Count > 0)
        {
            foreach (CardPokerFace cpf in cards)
            {
                cpf.gameObject.SetActive(on);
            }
        }
    }
}
