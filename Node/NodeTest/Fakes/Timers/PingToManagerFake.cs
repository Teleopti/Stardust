using System.Threading;
using System.Timers;
using log4net;
using Stardust.Node.Helpers;
using Timer = System.Timers.Timer;

namespace NodeTest.Fakes.Timers
{
    public class PingToManagerFake : Timer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (PingToManagerFake));

        public ManualResetEventSlim Wait = new ManualResetEventSlim();

        public PingToManagerFake(double interval = 20000): base(interval)
        {
            Elapsed += OnTimedEvent;

            AutoReset = true;
            Enabled = true;
        }

        private void OnTimedEvent(object sender,
                                  ElapsedEventArgs e)
        {
            LogHelper.LogInfoWithLineNumber(Logger,
                                            "Try ping to manager fake.");

            Wait.Set();
        }
    }
}