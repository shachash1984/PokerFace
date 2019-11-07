using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardButton : MonoBehaviour {

    static private int _selectedCards;
    static public int selectedCards
    {
        get
        {
            return _selectedCards;
        }
        set
        {
            _selectedCards = value >= 0 ? value : 0;
        }
    }
    private CardPokerFace _playerCard;
    private CardPokerFace _UICard;
    public bool selected = false;

    public void OnMouseUpAsButton()
    {
        
        selected =  _playerCard.ToggleSelection();
        
        if (selected)
            PokerFaceManager.S.localPlayer.SelectCard(_playerCard);
        else if (PokerFaceManager.S.localPlayer.selectedCards.Contains(_playerCard))
            PokerFaceManager.S.localPlayer.DeselectCard(_playerCard);

        UIManager.S.SetSwapButtonText();
        //making sure the button is not interactable until enough cards are selected
        if (PokerFaceManager.S.gameState == GameState.Reveal1)
            UIManager.S.ToggleRevealButtonInteraction(selectedCards == 1);
        else if(PokerFaceManager.S.gameState == GameState.Reveal2)
            UIManager.S.ToggleRevealButtonInteraction(selectedCards == 2);
    }

    public CardPokerFace GetUICard()
    {
        return _UICard;
    }

    public void SetUICard(CardPokerFace c)
    {
        _UICard = c;
    }

    public CardPokerFace GetPlayerCard()
    {
        return _playerCard;
    }

    public void SetPlayerCard(CardPokerFace c)
    {
        _playerCard = c;
    }

    public void ChangePlayerCard(CardPokerFace c)
    {
        Destroy(_UICard.gameObject);
        GameObject tGO = null;
        CardPokerFace tCPF = null;
        
        tGO = Instantiate(c.gameObject, transform);
        tGO.transform.localPosition = Vector3.zero;
        Destroy(tGO.GetComponent<Collider>());

        tCPF = tGO.GetComponent<CardPokerFace>();
        Destroy(tCPF.pipGOs[0]);
        tCPF.decoGOs[0].transform.localPosition = new Vector3(0f, 0.35f, 0);
        tCPF.decoGOs[1].transform.localPosition = new Vector3(0f, -0.45f, 0);
        tCPF.spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        foreach (SpriteRenderer sr in tCPF.spriteRenderers)
        {
            sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }
        tCPF.faceUp = true;
        SetPlayerCard(c);
        SetUICard(tCPF);
    }
}
