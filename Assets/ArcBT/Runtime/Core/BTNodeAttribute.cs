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
        /// ノードタイプ（Action/Condition）
        /// </summary>
        public NodeType NodeType { get; }
        
        /// <summary>
        /// アセンブリ名（どのアセンブリの登録コードに含めるか）
        /// </summary>
        public string AssemblyName { get; set; }
        
        public BTNodeAttribute(string scriptName, NodeType nodeType)
        {
            ScriptName = scriptName;
            NodeType = nodeType;
        }
    }
    
    public enum NodeType
    {
        Action,
        Condition
    }
}