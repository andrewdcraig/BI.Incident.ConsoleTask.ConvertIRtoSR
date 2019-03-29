using System;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;

namespace BI.WorkItem.ConsoleTask.ConvertWI.Helpers
{
    public enum SCSMTraceType
    { Console, File, EventLog };

    public static class SCSMTrace
    {
        static EventLog log = new EventLog("Operations Manager") { Source = "Console Operations" };

        static Stopwatch _timer = new Stopwatch();
        static Stopwatch Timer
        { get { return _timer; } }

        static SCSMTraceType _traceType = SCSMTraceType.Console;
        static SCSMTraceType TraceType
        {
            get { return _traceType; }
            set { _traceType = value; }
        }
        static string path = String.Empty;

        static SCSMTrace()
        {
            object DebugState = Registry.GetValue("HKEY_LOCAL_MACHINE\\Software\\Syliance\\Debug", "DebugState", 0);
            if (DebugState != null && (int)Registry.GetValue("HKEY_LOCAL_MACHINE\\Software\\Syliance\\Debug", "DebugState", 0) == 1)
            {
                object debugType = Registry.GetValue("HKEY_LOCAL_MACHINE\\Software\\Syliance\\Debug", "DebugType", "Console");
                if (debugType != null)
                {
                    try
                    {
                        SCSMTrace.TraceType = (SCSMTraceType)Enum.Parse(typeof(SCSMTraceType), (string)debugType);
                    }
                    catch { }

                    if (SCSMTrace.TraceType == SCSMTraceType.File)
                    {
                        path = Environment.ExpandEnvironmentVariables((string)Registry.GetValue("HKEY_LOCAL_MACHINE\\Software\\Syliance\\Debug", "DebugFilePath", ""));
                        if (!string.IsNullOrEmpty(path))
                        {
                            SCSMTrace.Timer.Start();
                            WriteFile("");
                            WriteFile(string.Format(" ========= START NEW SESSION at '{0}' =============", DateTime.Now));
                            WriteFile("");
                        }
                    }
                    else
                        SCSMTrace.Timer.Start();
                }
            }
        }

        public static void WriteString(string Message)
        {
            if (SCSMTrace.Timer.IsRunning)
            {
                switch (SCSMTrace.TraceType)
                {
                    case SCSMTraceType.Console:
                        SCSMTrace.WriteConsole(Message);
                        break;
                    case SCSMTraceType.EventLog:
                        SCSMTrace.WriteEventLog(Message);
                        break;
                    case SCSMTraceType.File:
                        SCSMTrace.WriteFile(Message);
                        break;
                }
            }
        }

        static void WriteConsole(string Message)
        {
            Console.WriteLine("{0}: \t{1}", SCSMTrace.Timer.Elapsed, Message);
        }

        static void WriteFile(string Message)
        {
            if (!string.IsNullOrEmpty(path))
            {
                System.IO.TextWriter file = null;
                try
                {
                    file = new System.IO.StreamWriter(path, true, Encoding.UTF8);
                    file.WriteLine("{0}: \t{1}", SCSMTrace.Timer.Elapsed, Message);
                }
                catch { }
                finally
                {
                    if (file != null)
                        file.Close();
                }
            }
        }

        static void WriteEventLog(string Message)
        {
            log.WriteEntry(Message, EventLogEntryType.Information);
        }

    }
}
