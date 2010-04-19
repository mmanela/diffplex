using System;
using System.Diagnostics;

namespace DiffPlex
{
    public static class Log
    {
        public static bool Enabled { get; set;}

        [Conditional("DEBUG")]
        public static void WriteLine(string format, params object[] args)
        {
            if (Enabled)
                Write(format + Environment.NewLine, args);
        }

        [Conditional("DEBUG")]
        public static void Write(string format, params object[] args)
        {
            if (Enabled)
                Console.Write(format, args);
        }
    }
}