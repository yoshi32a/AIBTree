using System.Collections.Generic;
using System.Diagnostics;

namespace ArcBT.Core
{
    /// <summary>
    /// ノードごとの実行統計を保持する構造体
    /// </summary>
    public struct NodeStats
    {
        /// <summary>実行回数</summary>
        public int ExecutionCount;

        /// <summary>累計実行時間（ミリ秒）</summary>
        public double TotalTimeMs;

        /// <summary>最大実行時間（ミリ秒）</summary>
        public double MaxTimeMs;

        /// <summary>最後の実行結果</summary>
        public BTNodeResult LastResult;

        /// <summary>平均実行時間（ミリ秒）</summary>
        public double AverageTimeMs => ExecutionCount > 0 ? TotalTimeMs / ExecutionCount : 0.0;
    }

    /// <summary>
    /// ビヘイビアツリーの軽量パフォーマンスプロファイラー。
    /// ノードごとの実行回数・所要時間・結果を追跡する。
    /// UNITY_EDITOR または DEVELOPMENT_BUILD でのみ有効。リリースビルドではゼロオーバーヘッド。
    /// </summary>
    public class BTProfiler
    {
        /// <summary>グローバルインスタンス</summary>
        public static BTProfiler Instance { get; } = new();

        readonly Dictionary<string, NodeStats> stats = new();
        readonly Dictionary<string, Stopwatch> activeStopwatches = new();

        /// <summary>プロファイリングが有効かどうか</summary>
        public bool IsEnabled { get; set; }

        BTProfiler()
        {
        }

        /// <summary>
        /// ノードの計測を開始する。
        /// リリースビルドではコンパイル時に除去される。
        /// </summary>
        /// <param name="nodeName">計測対象のノード名</param>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public void BeginNode(string nodeName)
        {
            if (!IsEnabled)
            {
                return;
            }

            if (!activeStopwatches.TryGetValue(nodeName, out var sw))
            {
                sw = new Stopwatch();
                activeStopwatches[nodeName] = sw;
            }

            sw.Restart();
        }

        /// <summary>
        /// ノードの計測を終了し、統計を更新する。
        /// リリースビルドではコンパイル時に除去される。
        /// </summary>
        /// <param name="nodeName">計測対象のノード名</param>
        /// <param name="result">ノードの実行結果</param>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public void EndNode(string nodeName, BTNodeResult result)
        {
            if (!IsEnabled)
            {
                return;
            }

            if (!activeStopwatches.TryGetValue(nodeName, out var sw))
            {
                return;
            }

            sw.Stop();
            var elapsedMs = sw.Elapsed.TotalMilliseconds;

            var nodeStats = stats.TryGetValue(nodeName, out var existing) ? existing : default;
            nodeStats.ExecutionCount++;
            nodeStats.TotalTimeMs += elapsedMs;
            nodeStats.LastResult = result;

            if (elapsedMs > nodeStats.MaxTimeMs)
            {
                nodeStats.MaxTimeMs = elapsedMs;
            }

            stats[nodeName] = nodeStats;
        }

        /// <summary>
        /// 全ノードの統計情報を取得する
        /// </summary>
        /// <returns>ノード名をキーとした統計辞書の読み取り専用ビュー</returns>
        public IReadOnlyDictionary<string, NodeStats> GetStats()
        {
            return stats;
        }

        /// <summary>
        /// 指定ノードの統計情報を取得する
        /// </summary>
        /// <param name="nodeName">ノード名</param>
        /// <param name="nodeStats">取得した統計情報</param>
        /// <returns>統計が存在する場合true</returns>
        public bool TryGetNodeStats(string nodeName, out NodeStats nodeStats)
        {
            return stats.TryGetValue(nodeName, out nodeStats);
        }

        /// <summary>
        /// 全統計をリセットする
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public void Reset()
        {
            stats.Clear();
            activeStopwatches.Clear();
        }
    }
}
