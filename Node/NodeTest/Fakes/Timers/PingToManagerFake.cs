using System.Threading;
using System.Timers;
using log4net;
using Timer = System.Timers.Timer;

namespace NodeTest.Fakes.Timers
{
    public class PingToManagerFake : Timer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (PingToManagerFake));

        public ManualResetEventSlim Wait = new ManualResetEventSlim();

        public PingToManagerFake()
        {
            Elapsed += OnTimedEvent;

            AutoReset = true;
            Enabled = true;
        }

        private void OnTimedEvent(object sender,
                                  ElapsedEventArgs e)
        {
            Logger.Info("Try ping to manager fake.");

            Wait.Set();
        }
    }
}