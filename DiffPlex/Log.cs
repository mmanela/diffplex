using System;
using System.Diagnostics;

namespace DiffPlex
{
    public static class Log
    {
        [Conditional("LOG")]
        public static void WriteLine(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
        }

        [Conditional("LOG")]
        public static void Write(string format, params object[] args)
        {
            Debug.Write(string.Format(format, args));
        }
    }
}