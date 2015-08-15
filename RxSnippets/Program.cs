using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RxSnippets
{
    public class Program
    {
        static void Main(string[] args)
        {
            RecurringAction();
            //FanOutDemo();
            //RefCount();
            //HotExample();
            //ColdExample();
            Console.ReadLine();
        }

        private static void RecurringAction()
        {
            TimeSpan interval = TimeSpan.FromSeconds(5);
            Action work = () =>
            {
                string msg = String.Format("Doing some work... {0}", DateTime.Now);
                Console.WriteLine(msg);
                Thread.Sleep(TimeSpan.FromSeconds(3));
            };

            var schedule =
              Scheduler.Default.ScheduleRecurringAction(interval, work);

            Console.WriteLine("Press return to stop.");
            Console.ReadLine();
            schedule.Dispose();
        }

        private static void FanOutDemo()
        {
            var hot = FanOut();
            var hotDisposable = hot.Connect();
            var history = hot.Replay();
            history.Connect();

            Thread.Sleep(TimeSpan.FromSeconds(5));
            hot.Subscribe(t => Console.WriteLine("Top Of Book: " + t));

            history.Subscribe(f => Console.WriteLine("History Of Prices: " + f));
        }

        private static IConnectableObservable<long> FanOut()
        {
            IConnectableObservable<long> hot = Observable.Interval(TimeSpan.FromSeconds(1))
                                 .Do(l => Console.WriteLine("Publishing: " + l))
                                 .Publish();
            return hot;
        }

        private static void RefCount()
        {
            var item = Observable.Interval(TimeSpan.FromSeconds(5))
                                 .Do(l => Console.WriteLine("Publishing: " + l))
                                 .Publish().RefCount();

            Thread.Sleep(TimeSpan.FromSeconds(7));
            item.Subscribe(t => Console.WriteLine("Sub1: " + t));
            Thread.Sleep(TimeSpan.FromSeconds(7));
            item.Subscribe(f => Console.WriteLine("Sub2: " + f));
        }

        private static void HotExample()
        {
            var item = Observable.Interval(TimeSpan.FromSeconds(5))
                                 .Do(l => Console.WriteLine("Publishing: " + l))
                                 .Publish();
            item.Connect();
            Thread.Sleep(TimeSpan.FromSeconds(7));
            item.Subscribe(t => Console.WriteLine("Sub1: " + t));
            Thread.Sleep(TimeSpan.FromSeconds(7));
            item.Subscribe(f => Console.WriteLine("Sub2: " + f));
        }

        private static void ColdExample()
        {
            var item = Observable.Interval(TimeSpan.FromSeconds(5));
            item.Subscribe(t => Console.WriteLine(t));
            Thread.Sleep(TimeSpan.FromSeconds(7));
            item.Subscribe(f => Console.WriteLine("f" + f));
        }


    }

    public static class Extensions
    {
        public static IDisposable ScheduleRecurringAction(this IScheduler scheduler, TimeSpan interval, Action action)
        {
            return scheduler.Schedule(interval, scheduleNext =>
            {
                action();
                scheduleNext(interval);
            });
        }
    }
   
}
