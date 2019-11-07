using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundType { CardMoveFast, CardMoveSlow, Click, Notification, Tick, Coin, Flip, Check, Coin2, Fold, Applause, ManyCoins, CardMove3, Coin4}

public class SoundManager : MonoBehaviour {

    static public SoundManager S;
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private SoundType[] audioNames;
    private Dictionary<SoundType, AudioClip> sounds = new Dictionary<SoundType, AudioClip>();

    private void Awake()
    {
        if (S != null)
            Destroy(gameObject);
        S = this;
        
        Init();

    }

    private void Init()
    {
        for (int i = 0; i < audioNames.Length; i++)
        {
            sounds.Add(audioNames[i], audioClips[i]);
        }
    }

    public AudioClip GetSoundByName(SoundType st)
    {
        return sounds[st];
    }
}
