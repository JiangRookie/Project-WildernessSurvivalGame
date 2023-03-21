namespace JKFrame
{
    /// <summary>
    /// 普通 C# 类单例模式基类
    /// </summary>
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        static T s_Instance;

        public static T Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new T();
                }

                return s_Instance;
            }
        }
    }
}