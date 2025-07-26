using System;

namespace ArcBT.Core
{
    /// <summary>
    /// BTスクリプト名を指定するためのAttribute
    /// このAttributeを付けたクラスは、パーサーが自動的に発見してインスタンス化します
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class BTScriptAttribute : Attribute
    {
        /// <summary>BTファイル内で使用するスクリプト名</summary>
        public string ScriptName { get; }

        /// <summary>
        /// BTスクリプト属性のコンストラクタ
        /// </summary>
        /// <param name="scriptName">BTファイル内で使用するスクリプト名</param>
        public BTScriptAttribute(string scriptName)
        {
            ScriptName = scriptName;
        }
    }
}