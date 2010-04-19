using System;
using System.Diagnostics;

namespace DiffPlex
{
    public static class Log
    {
        [Conditional("LOG")]
        public static void WriteLine(string format, params object[] args)
        {
             Write(format + Environment.NewLine, args);
        }

        [Conditional("LOG")]
        public static void Write(string format, params object[] args)
        {
            Console.Write(format, args);
        }
    }
}