using JKFrame;
using UnityEngine;

public class MenuSceneManager : LogicManagerBase<MenuSceneManager>
{
    public AudioClip BgAmbientSound;
    public AudioClip CampfireBurningSound;

    void Start()
    {
        UIManager.Instance.Show<UI_MenuSceneMainWindow>();

        // InvokeRepeating: 在 time 秒后调用 methodName 方法，然后每 repeatRate 秒调用一次。
        InvokeRepeating(nameof(PlayCampfireBurningSound), 0.2f, CampfireBurningSound.length);
        AudioManager.Instance.PlayBGAudio(BgAmbientSound);
    }

    protected override void RegisterEventListener() { }

    protected override void CancelEventListener() { }

    /// <summary>
    /// Play the sound of a campfire burning
    /// </summary>
    void PlayCampfireBurningSound()
    {
        AudioManager.Instance.PlayOneShot(CampfireBurningSound, Vector3.zero, 1, false);
    }
}