using JKFrame;
using UnityEngine;

public static class ProjectTool
{
    public static void PlayAudio(AudioType audioType)
    {
        AudioClip clip = ConfigManager.Instance.GetConfig<PlayerConfig>(ConfigName.PLAYER).AudioClipDict[audioType];
        AudioManager.Instance.PlayOneShot(clip, Vector3.zero, 1, false);
    }
}