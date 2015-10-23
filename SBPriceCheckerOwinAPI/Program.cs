using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Owin.Hosting;
using System.Threading;
using System.Timers;
using SBPriceCheckerCore.Helpers;

namespace SBPriceCheckerOwinAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            // Specify the URI to use for the local host:
            string baseUri = "http://*:8089";

            Console.WriteLine("Starting web Server...");
            WebApp.Start<Startup>(baseUri);

            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += (s, e) => OnTimedEvent(s, e).SwallowException();
            aTimer.Interval = 3600000; //1h
            aTimer.Enabled = true;

            Helper Helper = new Helper();
            if (!Helper.PricesInCache("APIBeer"))
                Helper.StorePricesInCache().ConfigureAwait(false);

            Console.WriteLine("Server running at {0} - press Enter to quit. ", baseUri);
            //Console.ReadLine();

            while (true)
            {
                Thread.Sleep(50);
            }
        }

        private static async Task OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Helper Helper = new Helper();

            if(!Helper.PricesInCache("APIBeer"))
                await Helper.StorePricesInCache().ConfigureAwait(false);
        }
    }

    public static class TaskExtensions
    {
        public static void SwallowException(this Task task)
        {
            task.ContinueWith(_ => { return; });
        }
    }
}
