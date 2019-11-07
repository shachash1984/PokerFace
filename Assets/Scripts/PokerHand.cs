using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[System.Serializable]
public class PokerHand : IComparable<PokerHand>
{
    public CardPokerFace[] Cards;
    public HandType playerHand;
    public PokerHand(CardPokerFace c1, CardPokerFace c2, CardPokerFace c3, CardPokerFace c4, CardPokerFace c5)
    {
        Cards = new CardPokerFace[] { c1, c2, c3, c4, c5 };
        Sort();
        if (GetGroupByRankCount(5) != 0)
            throw new Exception("Cannot have five cards with the same rank");
        if (HasDuplicates())
            throw new Exception("Cannot have duplicates");
        
    }
    public int strength = 0;

    public bool Contains(CardPokerFace card)
    {
        return Cards.Where(c => c.rankType == card.rankType && c.suitType == card.suitType).Any();
    }

    public bool HasDuplicates()
    {
        return Cards.GroupBy(c => new { c.rankType, c.suitType })
         .Where(c => c.Skip(1).Any()).Any();
    }

    public static bool HasDuplicates(IList<PokerHand> hands)
    {
        for (int i = 0; i < hands.Count; i++)
        {
            foreach (CardPokerFace card in hands[i].Cards)
            {
                for (int j = 0; j < hands.Count; j++)
                {
                    if (i != j && hands[j].Contains(card))
                        return true;
                }
            }
        }
        return false;
    }

    private void Sort()
    {
        Cards = Cards.OrderBy(c => c.rankType).OrderBy(c => Cards.Where(c1 => c1.rankType == c.rankType).Count()).ToArray();

        if (Cards[4].rankType == RankType.Ace && Cards[0].rankType == RankType.Two && (int)Cards[3].rankType - (int)Cards[0].rankType == 3)
        {
            Debug.Log("Moved Ace to beginning cards[0] : " + Cards[0].name);
            Cards = new CardPokerFace[] { Cards[4], Cards[0], Cards[1], Cards[2], Cards[3] };
        }
            
    }

    public int CompareTo(PokerHand other)
    {
        int counter = 0;
        switch (other.playerHand)
        {
            case HandType.RoyalFlush:
                if (Cards[0].rank > other.Cards[0].rank)
                {
                    counter = 1;
                    Debug.LogError("Both Players have Royal Flush but it seems the original player has a better hand...");
                }
                else if (Cards[0].rank < other.Cards[0].rank)
                {
                    counter = -1;
                    Debug.LogError("Both Players have Royal Flush but it seems the other player has a better hand...");
                }
                else
                    counter = 0;
                break;
            case HandType.StraightFlush:
                if (Cards[0].rank > other.Cards[0].rank)
                    counter = 1;
                else if (Cards[0].rank < other.Cards[0].rank)
                    counter = -1;
                else
                    counter = 0;
                    break;
            case HandType.FourOfAKind:
                if (Cards[0].rank > other.Cards[0].rank)
                    counter = 1;
                else if (Cards[0].rank < other.Cards[0].rank)
                    counter = -1;
                else
                {
                    if (Cards[4].rank > other.Cards[4].rank)
                        counter = 1;
                    else if (Cards[4].rank < other.Cards[4].rank)
                        counter = -1;
                    else
                        counter = 0;
                }
                break;
            case HandType.FullHouse:
                if (Cards[0].rank > other.Cards[0].rank)
                    counter = 1;
                else if (Cards[0].rank < other.Cards[0].rank)
                    counter = -1;
                else
                {
                    Debug.Log("Both players have full house with the same Three - not possible");
                }
                break;
            case HandType.Flush:
                if (Cards[4].rank > other.Cards[4].rank)
                    counter = 1;
                else if (Cards[4].rank < other.Cards[4].rank)
                    counter = -1;
                else
                    counter = 0;
                break;
            case HandType.Straight:
                if (Cards[4].rank > other.Cards[4].rank)
                    counter = 1;
                else if (Cards[4].rank < other.Cards[4].rank)
                    counter = -1;
                else
                    counter = 0;
                break;
            case HandType.ThreeOfAKind:
                if (Cards[0].rank > other.Cards[0].rank)
                    counter = 1;
                else if (Cards[0].rank < other.Cards[0].rank)
                    counter = -1;
                else
                {
                    if (Cards[4].rank > other.Cards[4].rank)
                        counter = 1;
                    else if (Cards[4].rank < other.Cards[4].rank)
                        counter = -1;
                    else
                        counter = 0;
                }
                break;
            case HandType.TwoPairs:
                if (Cards[0].rank > other.Cards[0].rank)
                    counter = 1;
                else if (Cards[0].rank < other.Cards[0].rank)
                    counter = -1;
                else
                {
                    if (Cards[2].rank > other.Cards[2].rank)
                        counter = 1;
                    else if (Cards[2].rank < other.Cards[2].rank)
                        counter = -1;
                    else
                    {
                        if (Cards[4].rank > other.Cards[4].rank)
                            counter = 1;
                        else if (Cards[4].rank < other.Cards[4].rank)
                            counter = -1;
                        else
                            counter = 0;
                    }
                }
                break;
            case HandType.OnePair:
                if (Cards[0].rank > other.Cards[0].rank)
                    counter = 1;
                else if (Cards[0].rank < other.Cards[0].rank)
                    counter = -1;
                else
                {
                    if (Cards[4].rank > other.Cards[4].rank)
                        counter = 1;
                    else if (Cards[4].rank < other.Cards[4].rank)
                        counter = -1;
                    else
                        counter = 0;
                }
                break;
            case HandType.HighCard:
                if (Cards[4].rank > other.Cards[4].rank)
                    counter = 1;
                else if (Cards[4].rank < other.Cards[4].rank)
                    counter = -1;
                else
                    counter = 0;
                break;
            default:
                break;
        }
        return counter;
        /*for (var i = 4; i >= 0; i--)
        {
            RankType rank1 = Cards[i].rankType;
            RankType rank2 = other.Cards[i].rankType;
            if (rank1 > rank2)
                return 1;
            if (rank1 < rank2)
                return -1;
        }
        return 0;*/
    }

    public bool IsValid(HandType handType)
    {
        switch (handType)
        {
            case HandType.RoyalFlush:
                return IsValid(HandType.StraightFlush) && Cards[4].rankType == RankType.Ace;
            case HandType.StraightFlush:
                return IsValid(HandType.Flush) && IsValid(HandType.Straight);
            case HandType.FourOfAKind:
                return GetGroupByRankCount(4) == 1;
            case HandType.FullHouse:
                return IsValid(HandType.ThreeOfAKind) && IsValid(HandType.OnePair);
            case HandType.Flush:
                return GetGroupBySuitCount(5) == 1;
            case HandType.Straight:
                return (int)Cards[4].rankType - (int)Cards[3].rankType == 1 && (int)Cards[3].rankType - (int)Cards[2].rankType == 1 && (int)Cards[2].rankType - (int)Cards[1].rankType == 1 && (int)Cards[1].rankType - (int)Cards[0].rankType == 1; //deck starts from 7
            case HandType.ThreeOfAKind:
                return GetGroupByRankCount(3) == 1;
            case HandType.TwoPairs:
                return GetGroupByRankCount(2) == 2;
            case HandType.OnePair:
                return GetGroupByRankCount(2) == 1;
            case HandType.HighCard:
                return GetGroupByRankCount(1) == 5;
        }
        return false;
    }

    private int GetGroupByRankCount(int n)
    {
        return Cards.GroupBy(c => c.rankType).Count(g => g.Count() == n);
    }

    private int GetGroupBySuitCount(int n)
    {
        return Cards.GroupBy(c => c.rankType).Count(g => g.Count() == n);
    }

    public static List<PokerPlayer> Evaluate(IDictionary<PokerPlayer, PokerHand> hands)
    {
        if (HasDuplicates(hands.Values.ToList()))
            throw new Exception("There are duplicate cards");
        var len = Enum.GetValues(typeof(HandType)).Length;
        var winners = new List<PokerPlayer>();
        HandType winningType = HandType.HighCard;

        int max = int.MinValue;
        foreach (var name in hands.Keys)
        {
            hands[name].CalculateHandStrength();
            Debug.Log(name.name + " " + hands[name].strength);
            if (hands[name].strength > max)
            {
                max = hands[name].strength;
                winners.Clear();
                winners.Add(name);
                winningType = winners[0].pokerHand.playerHand;
            }
            else if (hands[name].strength == max)
            {
                winners.Add(name);
                winningType = winners[0].pokerHand.playerHand;
            }
                
            
        }
        for (int i = 0; i < winners.Count; i++)
        {
            Debug.Log("Winner " + i + ": " + winners[i].name + " " + winningType);
        }
        
        Debug.Log("<color=red>Amount of winners: " + winners.Count + "</color>");
        return winners;
    }

    public void CalculateHandStrength()
    {
        strength = (15 - (int)playerHand) * 100;
        int rankAddition = 0;
        switch (playerHand)
        {
            case HandType.RoyalFlush:
                break;
            case HandType.StraightFlush:
                rankAddition += Cards[4].rank;
                break;
            case HandType.FourOfAKind:
                rankAddition += (Cards[2].rank * 4);
                break;
            case HandType.FullHouse:
                rankAddition += Cards[2].rank;
                break;
            case HandType.Flush:
                rankAddition += Cards[4].rank;
                break;
            case HandType.Straight:
                rankAddition += Cards[4].rank;
                break;
            case HandType.ThreeOfAKind:
                rankAddition += Cards[2].rank;
                break;
            case HandType.TwoPairs:
                rankAddition += Cards[4].rank;
                break;
            case HandType.OnePair:
                rankAddition += Cards[4].rank;
                break;
            case HandType.HighCard:
                rankAddition += Cards[4].rank;
                break;
            default:
                break;
        }
        strength += rankAddition;
        Debug.Log("type: " + playerHand.ToString() + " strength: " + strength + " card[4]: " + Cards[4]);
    }

    public override string ToString()
    {
        return string.Format("{0}, {1}, {2}, {3}, {4}", Cards[0], Cards[1], Cards[2], Cards[3], Cards[4]);
    }
}
