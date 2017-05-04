using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace bvmfscrapper.helpers
{
    /// <summary>
    /// Class to create the log4net provider
    /// </summary>
    public class Log4NetProvider : ILoggerProvider
    {/// <summary>
     /// The created loggers
     /// </summary>
        internal ConcurrentDictionary<string, Lazy<ILogger>> Loggers { get; }
        public static ILoggerProvider Instance { get; internal set; } = new Log4NetProvider();

        /// <summary>
        /// Constructor
        /// </summary>
        public Log4NetProvider()
        {
            Loggers = new ConcurrentDictionary<string, Lazy<ILogger>>();
        }

        /// <summary>
        /// Creates a ILogger instance
        /// </summary>
        /// <param name="name">The category name for messages produced by the logger</param>
        /// <returns>The ILogger instance</returns>
        public ILogger CreateLogger(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var logger = Loggers.GetOrAdd(name,
                loggerName =>
                    new Lazy<ILogger>(() => new Log4NetLogger(loggerName), LazyThreadSafetyMode.ExecutionAndPublication));

            return logger.Value;
        }


        /// <summary>
        /// Dispose the loggers
        /// </summary>
        public void Dispose()
        {
            Loggers.Clear();
        }
    }
}
