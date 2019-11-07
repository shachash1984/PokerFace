using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Effects : MonoBehaviour {

    public SpriteRenderer buttonSprite;
    public SpriteRenderer cardSprite;
    public Transform letter;
    public Transform suit;
    public float scaleFactor = 0.25f;


    private void OnMouseDown()
    {
        PokerFaceManager.S.PlaySound(SoundType.Click);
        if (!cardSprite || !letter || !suit)
        {
            cardSprite = transform.GetChild(2).GetComponent<SpriteRenderer>();
            letter = cardSprite.transform.GetChild(1);
            suit = cardSprite.transform.GetChild(2);
        }
        
        //cardSprite.color = new Color(0.69f, 0.69f, 0.69f);
        buttonSprite.flipX = true;
        letter.DOScaleX(letter.localScale.x + scaleFactor, 0.1f);
        suit.DOScaleX(suit.localScale.x + scaleFactor, 0.1f);
    }

    private void OnMouseUp()
    {
        //cardSprite.color = Color.white;
        buttonSprite.flipX = false;
        letter.DOScaleX(letter.localScale.x - scaleFactor, 0.1f);
        suit.DOScaleX(suit.localScale.x - scaleFactor, 0.1f);
    }
}
