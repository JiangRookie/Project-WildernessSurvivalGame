using JKFrame;
using UnityEngine;

[Pool]
public class HitEffect : MonoBehaviour
{
    void OnParticleSystemStopped() => this.PushGameObj2Pool();
}