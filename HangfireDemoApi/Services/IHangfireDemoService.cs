using System;
using HangfireDemoApi.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NLog.Extensions.Logging;

namespace HangfireDemoApi.Services
{
    public class HangfireDemoService
    {
        private static readonly ILogger<HangfireDemoService> _logger;
        private static readonly WeatherForecastController WeatherForecastController;

        static HangfireDemoService()
        {
            _logger = new Logger<HangfireDemoService>(new NLogLoggerFactory());
            WeatherForecastController = new WeatherForecastController(new NullLogger<WeatherForecastController>());
        }

        public static object FireAndForget()
        {
            _logger.LogInformation("starting fire and forget job");
            
            for (var i = 0; i < 1000; i++)
            {
                Console.WriteLine("Fire and Forget " + i);
            }

            _logger.LogInformation("finishing fire and forget job");

            var result = WeatherForecastController.Get();

            return result;
        }

        public static void Delayed(int time)
        {
            Console.WriteLine("Delayed after " + time + " minutes");
        }

        public static void Recurring()
        {
            Console.WriteLine("The time is " + DateTime.Now.TimeOfDay);
        }

        public static void LongRecurring()
        {
            for (var i = 0; i < 250000; i++)
            {
                Console.WriteLine("This is a long recurring job processed every 8 minutes " + i);
            }
        }
    }
}
