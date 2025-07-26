// Unity Source Generator Configuration
// このファイルはSource Generatorを有効にするための設定です

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ArcBT.Generators
{
    /// <summary>
    /// Unity用Source Generator設定
    /// Unity 2021.2以降でSource Generatorがサポートされています
    /// </summary>
    public static class GeneratorConfig
    {
        [InitializeOnLoadMethod]
        static void EnableSourceGenerators()
        {
            // Unity 2021.2以降でSource Generatorを有効化
            #if UNITY_2021_2_OR_NEWER
            BTLogger.LogSystem("Source Generators are enabled for Unity 2021.2+");
            #else
            Debug.LogWarning("Source Generators require Unity 2021.2 or newer. Manual registration will be used.");
            #endif
        }
    }
}
#endif