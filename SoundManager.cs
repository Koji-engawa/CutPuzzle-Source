using System.Collections.Generic;
using UnityEngine;

//サウンドを管理するクラス
public class SoundManager : MonoBehaviour
{
    //AudioSource（スピーカー）を同時に鳴らしたい音の数だけ用意
    private AudioSource[] audioSourceList = new AudioSource[5];

    public AudioClip OKSE;
    public AudioClip NGSE;
    public AudioClip rotationSE;
    public AudioClip clearMusic;
    public AudioSource bgm;

    //インスペクターからBGMをセット
    public List<AudioClip> bgmList = new List<AudioClip>();

    private void Awake()
    {
        for (int i = 0; i < audioSourceList.Length; ++i)
        {
            audioSourceList[i] = gameObject.AddComponent<AudioSource>();
        }
    }

    // シーン開始時にランダムなBGMを選択して再生
    void Start()
    {
        int randomIndex = Random.Range(0, bgmList.Count);
        bgm.clip = bgmList[randomIndex];
        bgm.loop = true;
        bgm.Play();
    }

    //未使用のAudioSourceの取得
    private AudioSource GetUnusedAudioSource()
    {
        for (int i = 0; i < audioSourceList.Length; ++i)
        {
            if (audioSourceList[i].isPlaying == false)
                return audioSourceList[i];
        }

        return null;
    }

    //指定されたAudioClipを未使用のAudioSourceで再生
    public void Play(AudioClip clip)
    {
        AudioSource audioSource = GetUnusedAudioSource();
        if (audioSource == null) return;
        audioSource.clip = clip;
        audioSource.Play();
    }
}