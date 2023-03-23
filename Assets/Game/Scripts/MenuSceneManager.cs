using JKFrame;
using UnityEngine;

public class MenuSceneManager : LogicManagerBase<MenuSceneManager>
{
    public AudioClip BgAudio1;
    public AudioClip BgAudio2;
    protected override void RegisterEventListener() { }

    protected override void CancelEventListener() { }

    void Start()
    {
        UIManager.Instance.Show<UI_MenuSceneMainWindow>();
        InvokeRepeating(nameof(PlayFireSound), 0.2f, BgAudio2.length);
        AudioManager.Instance.PlayBGAudio(BgAudio1);
    }

    void PlayFireSound()
    {
        AudioManager.Instance.PlayOneShot(BgAudio2, Vector3.zero, 1, false);
    }
}