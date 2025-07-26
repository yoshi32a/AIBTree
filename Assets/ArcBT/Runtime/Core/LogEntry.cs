using System;

namespace ArcBT.Core
{
    public struct LogEntry
    {
        public readonly DateTime Timestamp;
        public readonly LogLevel Level;
        public readonly LogCategory Category;
        public readonly string Message;
        public readonly string NodeName;
        public readonly UnityEngine.Object Context;

        public LogEntry(LogLevel level, LogCategory category, string message, string nodeName = "", UnityEngine.Object context = null)
        {
            Timestamp = DateTime.Now;
            Level = level;
            Category = category;
            Message = message;
            NodeName = nodeName;
            Context = context;
        }
    }
}