using System;

namespace ArcBT.Core
{
    /// <summary>
    /// BTノードクラスに付与して自動登録対象とするための属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class BTNodeAttribute : Attribute
    {
        /// <summary>
        /// .btファイルで使用するスクリプト名
        /// </summary>
        public string ScriptName { get; }

        /// <summary>
        /// BTNodeAttribute コンストラクタ（NodeTypeは基底クラスから自動判定）
        /// </summary>
        /// <param name="scriptName">.btファイルで使用するスクリプト名</param>
        public BTNodeAttribute(string scriptName)
        {
            ScriptName = scriptName;
        }
    }
}
