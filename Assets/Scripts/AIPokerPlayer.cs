using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPokerPlayer : PokerPlayer {


    public int SetAvailableOptions()
    {
        return totalAmount - (PokerFaceManager.S.amountToBet - betAmount);
    }

    public void CalculateMove()
    {
        int option = SetAvailableOptions();
        CalculateHandStrength();
        int rand = UnityEngine.Random.Range(1, 100);
        //Debug.Log(name + "hand: " + pokerHand.playerHand + "calculating... rand: " + rand);
        if (PokerFaceManager.S.amountToBet > 0)
        {
            if (option > 0)
            {
                switch (pokerHand.playerHand)
                {
                    case HandType.RoyalFlush:
                    case HandType.StraightFlush:
                    case HandType.FourOfAKind:
                    case HandType.FullHouse:
                    case HandType.Flush:
                    case HandType.Straight:
                        if (rand % 2 == 0)
                            SetCommand(new RaiseCommand());
                        else
                            SetCommand(new CallCommand());
                        break;
                    case HandType.ThreeOfAKind:
                        if (rand % 3 == 0)
                            SetCommand(new RaiseCommand());
                        else
                            SetCommand(new CallCommand());
                        break;
                    case HandType.TwoPairs:
                        if (rand % 5 == 0)
                            SetCommand(new RaiseCommand());
                        else if (rand % 5 > 0 && rand % 5 < 3)
                            SetCommand(new CallCommand());
                        else
                            SetCommand(new FoldCommand());
                        break;
                    case HandType.OnePair:
                        if (rand % 10 == 0)
                            SetCommand(new RaiseCommand());
                        else if (rand % 10 > 0 && rand % 10 < 8)
                            SetCommand(new FoldCommand());
                        else
                            SetCommand(new CallCommand());
                        break;
                    case HandType.HighCard:
                        if (rand % 20 == 0)
                            SetCommand(new RaiseCommand());
                        else if (rand % 20 > 0 && rand % 20 < 18)
                            SetCommand(new FoldCommand());
                        else
                            SetCommand(new CallCommand());
                        break;
                    default:
                        Debug.LogError(name + " Hand is not valid");
                        break;
                }
            }
            else if (option == 0)
            {
                switch (pokerHand.playerHand)
                {
                    case HandType.RoyalFlush:
                    case HandType.StraightFlush:
                    case HandType.FourOfAKind:
                    case HandType.FullHouse:
                    case HandType.Flush:
                    case HandType.Straight:
                    case HandType.ThreeOfAKind:
                    case HandType.TwoPairs:
                        SetCommand(new AllInCommand());
                        break;
                    case HandType.OnePair:
                        if (rand % 2 == 0)
                            SetCommand(new AllInCommand());
                        else
                            SetCommand(new FoldCommand());
                        break;
                    case HandType.HighCard:
                        if (rand % 2 == 0)
                            SetCommand(new AllInCommand());
                        else
                            SetCommand(new FoldCommand());
                        break;
                    default:
                        Debug.LogError(name + " Hand is not valid");
                        break;
                }
            }
            else
            {
                switch (pokerHand.playerHand)
                {
                    case HandType.RoyalFlush:
                    case HandType.StraightFlush:
                    case HandType.FourOfAKind:
                    case HandType.FullHouse:
                    case HandType.Flush:
                    case HandType.Straight:
                        SetCommand(new AllInCommand());
                        break;
                    case HandType.ThreeOfAKind:
                        if (rand % 2 == 0)
                            SetCommand(new AllInCommand());
                        else
                            SetCommand(new FoldCommand());
                        break;
                    case HandType.TwoPairs:
                        if (rand % 3 == 0)
                            SetCommand(new AllInCommand());
                        else
                            SetCommand(new FoldCommand());
                        break;
                    case HandType.OnePair:
                        if (rand % 6 == 0)
                            SetCommand(new AllInCommand());
                        else
                            SetCommand(new FoldCommand());
                        break;
                    case HandType.HighCard:
                        if (rand % 10 == 0)
                            SetCommand(new AllInCommand());
                        else
                            SetCommand(new FoldCommand());
                        break;
                    default:
                        Debug.LogError(name + " Hand is not valid");
                        break;
                }
            }
        }
        else //TODO
        {
            switch (pokerHand.playerHand)
            {
                case HandType.RoyalFlush:
                case HandType.StraightFlush:
                case HandType.FourOfAKind:
                case HandType.FullHouse:
                case HandType.Flush:
                case HandType.Straight:
                    if (rand % 2 == 0)
                        SetCommand(new RaiseCommand());
                    else
                        SetCommand(new CheckCommand());
                    break;
                case HandType.ThreeOfAKind:
                    if (rand % 4 == 0)
                        SetCommand(new RaiseCommand());
                    else
                        SetCommand(new CheckCommand());
                    break;
                case HandType.TwoPairs:
                    if (rand % 8 == 0)
                        SetCommand(new RaiseCommand());
                    else
                        SetCommand(new CheckCommand());
                    break;
                case HandType.OnePair:
                    if (rand % 10 == 0)
                        SetCommand(new RaiseCommand());
                    else
                        SetCommand(new CheckCommand());
                    break;
                case HandType.HighCard:
                    if (rand % 12 == 0)
                        SetCommand(new RaiseCommand());
                    else
                        SetCommand(new CheckCommand());
                    break;
                default:
                    Debug.LogError(name + " Hand is not valid");
                    break;
            }
        }
    }
}
