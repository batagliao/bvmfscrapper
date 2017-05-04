using log4net;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bvmfscrapper.helpers
{
    /// <summary>
    /// Class to create the log4net logger
    /// </summary>
    public class Log4NetLogger : ILogger
    {
        /// <summary>
        /// The name given to the logger
        /// </summary>
        public string LoggerName { get; }

        /// <summary>
        /// The logger
        /// </summary>
        internal ILog Logger { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggerName">The logger name</param>
        public Log4NetLogger(string loggerName)
        {
            if (string.IsNullOrWhiteSpace(loggerName))
            {
                throw new ArgumentNullException(nameof(loggerName));
            }

            LoggerName = loggerName;
            var repo = LogManager.GetAllRepositories().First();
            Logger = LogManager.GetLogger(repo.Name, loggerName);
        }

        /// <summary>
        /// Checks if the given logLevel is enabled
        /// </summary>
        /// <param name="logLevel">level to be checked</param>
        /// <returns>A boolean denoting if enabled</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    {
                        return Logger.IsDebugEnabled;
                    }
                case LogLevel.Information:
                    {
                        return Logger.IsInfoEnabled;
                    }
                case LogLevel.Warning:
                    {
                        return Logger.IsWarnEnabled;
                    }
                case LogLevel.Error:
                    {
                        return Logger.IsErrorEnabled;
                    }

                case LogLevel.Critical:
                    {
                        return Logger.IsFatalEnabled;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        /// <summary>
        /// Writes a log entry
        /// </summary>
        /// <param name="logLevel">Entry will be written on this level</param>
        /// <param name="eventId">Id of the event</param>
        /// <param name="state">The entry to be written. Can be also an object</param>
        /// <param name="exception">The exception related to this entry</param>
        /// <param name="formatter">Function to create a string message of the state and exception</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter == null ? state.ToString() : formatter(state, exception);

            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    {
                        Logger.Debug(message, exception);
                        break;
                    }

                case LogLevel.Information:
                    {
                        Logger.Info(message, exception);
                        break;
                    }
                case LogLevel.Warning:
                    {
                        Logger.Warn(message, exception);
                        break;
                    }
                case LogLevel.Error:
                    {
                        Logger.Error(message, exception);
                        break;
                    }
                case LogLevel.Critical:
                    {
                        Logger.Fatal(message, exception);
                        break;
                    }
            }
        }

        /// <summary>
        /// Begins a logical operation scope
        /// </summary>
        /// <param name="state">The identifier for the scope</param>
        /// <returns>An IDisposable that ends the logical operation scope on dispose</returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
