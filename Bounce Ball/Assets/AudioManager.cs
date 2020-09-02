using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Реализация singlton'а для данного класса
    public static AudioManager instance;
    // Применение singlton'а для обращения к другому классу
    private static Player player { get => Player.instance; }

    public AudioSource audioSourceMusic;

    public AudioSource audioSourceEffectsBall;
    public AudioSource audioSourceEffectsUI;

    public AudioClip musicClip;
    public float musicClipVolume = 0.1f;

    public AudioClip jumpClip;
    public float jumpClipVolume = 0.36f;
    public AudioClip victoryClip;
    public float victoryClipVolume = 0.07f;
    public AudioClip defeatClip;
    public float defeatClipVolume = 0.16f;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        audioSourceMusic.volume = musicClipVolume;
        audioSourceMusic.clip = musicClip;
        audioSourceMusic.Play();
    }

    public static void PlayJumpJingle()
    {
        instance.audioSourceEffectsBall.volume = instance.jumpClipVolume;
        instance.audioSourceEffectsBall.PlayOneShot(instance.jumpClip);
    }
    public static void PlayVictoryJingle()
    {
        instance.audioSourceEffectsUI.volume = instance.victoryClipVolume;
        instance.audioSourceEffectsUI.PlayOneShot(instance.victoryClip);
    }
    public static void PlayDefeatJingle()
    {
        instance.audioSourceEffectsUI.volume = instance.defeatClipVolume;
        instance.audioSourceEffectsUI.PlayOneShot(instance.defeatClip);
    }
}
