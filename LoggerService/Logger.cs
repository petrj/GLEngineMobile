using System;
using System.Collections.Generic;
using System.Text;

namespace LoggerService
{
    public static class Logger
    {
        private static ILoggingService _loggingService = new DummyLoggingService();

        public static void InitLoggerService(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public static void Debug(string message)
        {
            _loggingService.Debug(message);
        }

        public static void Error(Exception ex, string message = null)
        {
            _loggingService.Error(ex, message);
        }

        public static void Error(string message, Exception ex = null)
        {
            _loggingService.Error(ex, message);
        }

        public static void Info(string message)
        {
            _loggingService.Info(message);
        }
    }
}
