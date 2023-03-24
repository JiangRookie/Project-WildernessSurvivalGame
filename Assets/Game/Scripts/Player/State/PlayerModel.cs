using System;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    Action<int> m_FootstepAction;
    [SerializeField] Transform m_WeaponRoot;
    public Transform WeaponRoot => m_WeaponRoot;

    public void Init(Action<int> footstepAction)
    {
        m_FootstepAction = footstepAction;
    }

    #region 动画事件

    private void Footstep(int index)
    {
        m_FootstepAction?.Invoke(index);
    }

    #endregion
}