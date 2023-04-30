using System.Collections.Generic;

namespace JKFrame
{
    /// <summary>
    /// 普通类 对象 对象池数据
    /// </summary>
    public class ObjectPoolData
    {
        public Queue<object> poolQueue = new Queue<object>(); // 对象容器
        public ObjectPoolData(object obj) => Push(obj);
        public void Push(object obj) => poolQueue.Enqueue(obj);
        public object Get() => poolQueue.Dequeue();
    }
}