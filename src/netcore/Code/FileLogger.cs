using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore.Code
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private string _location;
        public LogLevel _minLogLevel;

        public FileLoggerProvider(string location, LogLevel minLogLevel = LogLevel.Warning)
        {
            _location = location;
            _minLogLevel = minLogLevel;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(_location, _minLogLevel);
        }

        public void Dispose()
        {

        }
    }

    public class FLScopeStack
    {
        static Stack<object> stack = new Stack<object>();

        public static void Push(object o)
        {
            FLScopeStack.stack.Push(o);
        }

        public static object Pop()
        {
            return FLScopeStack.stack.Pop();
        }

        public static List<object> GetList()
        {
            return FLScopeStack.stack.Where(x => x != null).ToList();
        }
    }

    public class FLScope : IDisposable
    {
        public object State { get; set; }
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.State = null;
                    FLScopeStack.Pop();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

    }

    public class FileLogger : ILogger
    {
        private string _location;
        public LogLevel _minLogLevel;

        private static object LogLock = new object();

        public FileLogger(string location, LogLevel minLogLevel)
        {
            _location = location;
            _minLogLevel = minLogLevel;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            //FLScopeStack.Push(state);
            //return new FLScope()
            //{
            //    State = state
            //};
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel >= _minLogLevel)
                Task.Run(() => DoLog(formatter(state, exception)));
        }

        private void DoLog(string msg)
        {
            lock (LogLock)
            {
                File.AppendAllText(_location, msg + Environment.NewLine);
            }

        }
    }
}
