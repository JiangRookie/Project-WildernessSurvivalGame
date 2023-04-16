using System.Collections.Generic;

namespace JKFrame
{
    public interface IStateMachineOwner { }

    /// <summary>
    /// 状态机控制器
    /// </summary>
    [Pool]
    public class StateMachine
    {
        // 当前生效中的状态
        StateBase m_CurrStateObj;

        // 宿主
        IStateMachineOwner m_Owner;

        // 所有的状态 Key:状态枚举的值 Value:具体的状态
        Dictionary<int, StateBase> m_StateDic = new Dictionary<int, StateBase>();

        // 当前状态
        public int CurrStateType { get; private set; } = -1;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="owner">宿主</param>
        public void Init(IStateMachineOwner owner)
        {
            m_Owner = owner;
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <typeparam name="T">具体要切换到的状态脚本类型</typeparam>
        /// <param name="newStateType">新状态</param>
        /// <param name="reCurrState">新状态和当前状态一致的情况下，是否也要切换</param>
        /// <returns></returns>
        public bool ChangeState<T>(int newStateType, bool reCurrState = false) where T : StateBase, new()
        {
            // 状态一致，并且不需要刷新状态，则切换失败
            if (newStateType == CurrStateType && !reCurrState) return false;

            // 退出当前状态
            if (m_CurrStateObj != null)
            {
                m_CurrStateObj.Exit();
                m_CurrStateObj.RemoveUpdate(m_CurrStateObj.Update);
                m_CurrStateObj.RemoveLateUpdate(m_CurrStateObj.LateUpdate);
                m_CurrStateObj.RemoveFixedUpdate(m_CurrStateObj.FixedUpdate);
            }

            // 进入新状态
            m_CurrStateObj = GetState<T>(newStateType);
            CurrStateType = newStateType;
            m_CurrStateObj.Enter();
            m_CurrStateObj.OnUpdate(m_CurrStateObj.Update);
            m_CurrStateObj.OnLateUpdate(m_CurrStateObj.LateUpdate);
            m_CurrStateObj.OnFixedUpdate(m_CurrStateObj.FixedUpdate);
            return true;
        }

        /// <summary>
        /// 从对象池获取一个状态
        /// </summary>
        StateBase GetState<T>(int stateType) where T : StateBase, new()
        {
            if (m_StateDic.ContainsKey(stateType)) return m_StateDic[stateType];
            StateBase state = ResManager.Load<T>();
            state.Init(m_Owner, stateType, this);
            m_StateDic.Add(stateType, state);
            return state;
        }

        /// <summary>
        /// 停止工作
        /// 把所有状态都释放，但是StateMachine未来还可以工作
        /// </summary>
        public void Stop()
        {
            if (m_CurrStateObj != null)
            {
                // 处理当前状态的额外逻辑
                m_CurrStateObj.Exit();
                m_CurrStateObj.RemoveUpdate(m_CurrStateObj.Update);
                m_CurrStateObj.RemoveLateUpdate(m_CurrStateObj.LateUpdate);
                m_CurrStateObj.RemoveFixedUpdate(m_CurrStateObj.FixedUpdate);
                CurrStateType = -1;
                m_CurrStateObj = null;
            }

            // 处理缓存中所有状态的逻辑
            var enumerator = m_StateDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.Value.UnInit();
            }

            m_StateDic.Clear();
        }

        /// <summary>
        /// 销毁，宿主应该释放掉StateMachine的引用
        /// </summary>
        public void Destroy()
        {
            // 处理所有状态
            Stop();

            // 放弃所有资源的引用
            m_Owner = null;

            // 放进对象池
            this.JKObjectPushPool();
        }
    }
}