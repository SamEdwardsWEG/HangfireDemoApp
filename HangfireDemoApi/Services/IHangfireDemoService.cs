using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;

namespace HangfireDemoApi.Services
{
    public static class HangfireDemoService
    {
        public static void FireAndForget()
        {
            for (var i = 0; i < 1000; i++)
            {
                Console.WriteLine("Fire and Forget " + i);
            }
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
