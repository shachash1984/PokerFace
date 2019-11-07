using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public abstract class Command  {

    public abstract void Execute(PokerPlayer p);
}

public class CallCommand : Command
{
    public override void Execute(PokerPlayer p)
    {
        Debug.Log(p.name + " Call, AmountToBet: " + PokerFaceManager.S.amountToBet);
        int amountToBet = PokerFaceManager.S.amountToBet - p.betAmount;
        p.betAmount += amountToBet;
        p.totalAmount -= amountToBet;
        UIManager.S.SetBetAmountText(p);
        UIManager.S.SetTotalAmountText(p);
        UIManager.S.PlaySound(SoundType.Coin2);
    }
}

public class FoldCommand : Command
{

    public override void Execute(PokerPlayer p)
    {
        Debug.Log(p.name + " Fold");
        p.isIn = false;
        p.ToggleCards(false);
        UIManager.S.PlaySound(SoundType.Fold);
    }
}

public class CheckCommand : Command
{
    public override void Execute(PokerPlayer p)
    {
        Debug.Log(p.name + " Check");
        PokerFaceManager.S.PlaySound(SoundType.Check);
    }
}

public class RaiseCommand : Command
{
    public override void Execute(PokerPlayer p)
    {
        if (p is AIPokerPlayer)
        {
            if(PokerFaceManager.S.amountToBet < (p.totalAmount - p.betAmount))
                p.raiseCashAmount = Random.Range(PokerFaceManager.S.amountToBet + 1, p.totalAmount);
            else
            {
                Debug.LogError(p.name + " tried to raise, but doesnt have enough money");
            }
            p.betAmount += p.raiseCashAmount;
            p.totalAmount -= p.raiseCashAmount;
            UIManager.S.PlayMovingBetSequence(p);
            Debug.Log(p.name + " Raise " + p.raiseCashAmount);
        }
        else
        {
            Debug.Log(p.name + " Raise " + p.betAmount);
        }
            
        PokerFaceManager.S.SetAmountToBet(p.betAmount);
        
    }   
}

public class AllInCommand : Command
{
    public override void Execute(PokerPlayer p)
    {
        Debug.Log(p.name + " Allin");
        p.betAmount += p.totalAmount;
        p.totalAmount = 0;
        UIManager.S.PlayMovingBetSequence(p);
    }
}

public class ChangeCardsCommand : Command
{
    public override void Execute(PokerPlayer p)
    {
        
        CardPokerFace newCard = null;
        Vector3 cardPosition = Vector3.zero;
        Sequence seq = DOTween.Sequence();
        Layout layout = PokerFaceManager.S.layout;
        while (p.selectedCards.Count > 0)
        {
            cardPosition = p.selectedCards[0].transform.localPosition;
            cardPosition.y -= 0.5f;
            int layerID = p.selectedCards[0].spriteRenderer.sortingLayerID;
            PokerFaceManager.S.MoveToDiscard(p.selectedCards[0]);
            // move old card to discard
            seq.Append(p.selectedCards[0].transform.DOMove(new Vector3(0f, 4.5f, 0f), 0.5f));
            newCard = PokerFaceManager.S.Draw(); // get new card from drawpile
            newCard.spriteRenderer.sortingLayerID = layerID;
            newCard.spriteRenderer.sortingOrder = 0;
            foreach (SpriteRenderer sr in newCard.spriteRenderers)
            {
                sr.sortingLayerID = newCard.spriteRenderer.sortingLayerID;
                if (sr.name == "back")
                    sr.sortingOrder = 2;
                else
                {
                    sr.sortingOrder = 1;
                    Vector3 wantedPos = sr.transform.localPosition;
                    wantedPos.z = -1;
                    sr.transform.localPosition = wantedPos;
                }
                    
            }
            seq.Append(newCard.transform.DOLocalMove(new Vector3(0f, 4.5f, 0f), 0.01f));
            seq.Append(newCard.transform.DOLocalMove(cardPosition, 0.5f).OnStart(() => PokerFaceManager.S.PlaySound(SoundType.CardMoveSlow))); // animate movement
            seq.Append(p.selectedCards[0].transform.DOMove(new Vector3(layout.multiplier.x * layout.discardPile.x - 80, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID + 0.5f), 0f));
            seq.Play();
            newCard.index = p.selectedCards[0].index; // assigning the correct index
            p.cards[newCard.index] = newCard; // assign the correct card to player card list
            p.DeselectCard(p.selectedCards[0]); // make sure the card is no longer selected
        }
        PokerFaceManager.S.RemovePokerHand(p);
        PokerFaceManager.S.AddPokerHand(p);
        
    }
}

public class RevealCardsCommand : Command
{
    public override void Execute(PokerPlayer p)
    {
        foreach (CardPokerFace card in p.selectedCards)
        {
            float xScale = card.transform.localScale.x;
            Sequence seq = DOTween.Sequence();
            seq.Append(card.transform.DOScaleX(0f, 0.25f).OnComplete(() => card.Flip())); // play flip animation
            seq.Append(card.transform.DOScaleX(xScale, 0.25f)); // play flip animation
            PokerFaceManager.S.PlaySound(SoundType.Flip);
            seq.Play();
            
        }
        for (int i = p.selectedCards.Count-1; i >= 0; i--)
        {
            p.DeselectCard(p.selectedCards[i]);
        }
    }
}

public class BuyInCommand : Command
{
    public override void Execute(PokerPlayer p)
    {
        p.betAmount = PokerFaceManager.S.buyInAmount;
        p.totalAmount -= p.betAmount;
        p.isIn = true;
        UIManager.S.SetBetAmountText(p);
        UIManager.S.SetTotalAmountText(p);
        //PokerFaceManager.S.AddToActivePlayers(p);
    }
}

public class BuyTimeCommand : Command
{
    public override void Execute(PokerPlayer p)
    {
        p.totalAmount -= 50;
        UIManager.S.ToggleSpark(false);
        PokerFaceManager.S.BuyMoreTime();
        UIManager.S.ToggleSpark(true);
        UIManager.S.SetTotalAmountText(p);
        UIManager.S.PlaySound(SoundType.Coin);

    }
}

public class IncreaseBetCommand : Command
{
    public override void Execute(PokerPlayer p)
    {
        p.totalAmount -= 100; //property cannot be lower than 0
        if ((p.totalAmount - 100) >= 0)
            p.betAmount += 100;
        UIManager.S.PlayMovingBetSequence(p);
        //play bet animation
    }
}

public class DecreaseBetCommand : Command
{
    public override void Execute(PokerPlayer p)
    {
        if (p.betAmount > 0)
            p.totalAmount += 100;
        p.betAmount -= 100; //property cannot be lower than 0
        UIManager.S.SetTotalAmountText(p);
        UIManager.S.SetBetAmountText(p);
        UIManager.S.PlaySound(SoundType.Coin2);
        //play bet animation
    }
}
