using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum CardState
{
    DrawPile,
    Hand,
    Table,
    Discard,
    Target
}

public class CardPokerFace : Card {
    public float selectedYPos = -0.85f;
    public float unselectedYPos = -1.35f;
    private int _index;
    public int index
    {
        get { return _index; }
        set {
            if (value >= 0 && value < 5)
                _index = value;
            else
                Debug.LogError("index can only be within 0 and 4");
            }
    }
    public bool selected = false;
    public CardState state = CardState.DrawPile;
    public int layoutID;
    public SlotDef slotDef;
    //the hiddenBy list stores which other cards will keep this one face down
    public List<CardPokerFace> hiddenBy = new List<CardPokerFace>();

    public bool ToggleSelection()
    {
        if(PokerFaceManager.S.gameState == GameState.Swap)
        {
            if (!selected)
            {
                if (CardButton.selectedCards < 3)
                {
                    transform.DOLocalMoveY(selectedYPos, 0.5f);
                    selected = true;
                }
            }
            else
            {
                transform.DOLocalMoveY(unselectedYPos, 0.5f);
                selected = false;
            }
            UIManager.S.SetSwapButtonText(); // setting the correct text on the button
        }
        else if(PokerFaceManager.S.gameState == GameState.Reveal2)
        {
            if (!faceUp)
            {
                if (!selected)
                {
                    if (CardButton.selectedCards < 2)
                    {
                        transform.DOLocalMoveY(selectedYPos, 0.5f);
                        selected = true;
                    }
                }
                else
                {
                    transform.DOLocalMoveY(unselectedYPos, 0.5f);
                    selected = false;
                }
            }
            
        }
        else if(PokerFaceManager.S.gameState == GameState.Reveal1)
        {
            if (!faceUp)
            {
                if (!selected)
                {
                    if (CardButton.selectedCards < 1)
                    {

                        transform.DOLocalMoveY(selectedYPos, 0.5f);
                        selected = true;
                    }
                }
                else
                {
                    transform.DOLocalMoveY(unselectedYPos, 0.5f);
                    selected = false;
                }
            }
            else
                return false;
        }
        return selected;
    }

    
}
