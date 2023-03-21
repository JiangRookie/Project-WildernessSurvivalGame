using System;

namespace JKFrame
{
    /// <summary>
    /// UI元素的特性，每个UI窗口都应该添加。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class UIElementAttribute : Attribute
    {
        public bool IsCache;
        public string ResPath;
        public int LayerNum;

        /// <param name="isCache">是否缓存？如果缓存了关闭则不会销毁。</param>
        /// <param name="resPath">加载路径，在 Resources 文件夹中的地址</param>
        /// <param name="layerNum">层级</param>
        public UIElementAttribute(bool isCache, string resPath, int layerNum)
        {
            IsCache = isCache;
            ResPath = resPath;
            LayerNum = layerNum;
        }
    }
}