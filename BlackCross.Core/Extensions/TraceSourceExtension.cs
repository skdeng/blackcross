using System.Diagnostics;

namespace BlackCross.Core.Extensions
{
    public static class TraceSourceExtension
    {
        public static void TraceVerbose(this TraceSource traceSource, string message)
        {
            traceSource.TraceEvent(TraceEventType.Verbose, (int)TraceEventType.Verbose, message);
        }

        public static void TraceWarning(this TraceSource traceSource, string message)
        {
            traceSource.TraceEvent(TraceEventType.Warning, (int)TraceEventType.Warning, message);
        }
    }
}
