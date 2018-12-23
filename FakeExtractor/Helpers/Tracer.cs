using System;
using System.Diagnostics;

namespace FakeExtractor.Helpers
{
    public static class Tracer
    {
        public static void Info(string message)
        {
            Trace.WriteLine(message, "[INFO]");
        }

        public static void Warning(string message)
        {
            Trace.WriteLine(message, "[WARN]");
        }

        public static void Error(Exception exception)
        {
            Trace.WriteLine(exception, "[ERR]");
        }

        public static void Error(Exception exception, string message)
        {
            Trace.WriteLine(message, "[ERR]");
            Trace.WriteLine(exception, "[ERR]");
        }

        public static void EmptyLine()
        {
            Trace.WriteLine(string.Empty);
        }
    }
}