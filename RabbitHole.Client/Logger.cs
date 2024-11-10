using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitHole.Client
{
    public class Logger
    {
        private static Dictionary<Type, ILogger> _loggers = new Dictionary<Type, ILogger>();

        public static ILogger Get<T>()
        {
            lock (_loggers)
            {
                var t = typeof(T);
                if (!_loggers.ContainsKey(t))
                {
                    _loggers[t] = AppLoggerFactory.Value.CreateLogger(t.Name);
                }

                return _loggers[t];
            }
        }
    }

    public static class AppLoggerFactory
    {
        public static readonly ILoggerFactory Value = LoggerFactory.Create(config =>
        {
            config.SetMinimumLevel(LogLevel.Trace);
            config.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.IncludeScopes = false;
                options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Disabled;
            });
        });
    }
}