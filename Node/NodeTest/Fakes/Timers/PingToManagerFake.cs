using System.Diagnostics;
using System.Timers;

namespace NodeTest.Fakes.Timers
{
    public class PingToManagerFake : Timer
    {
        public PingToManagerFake()
        {
            Elapsed += OnTimedEvent;

            AutoReset = true;
            Enabled = true;
        }

        private void OnTimedEvent(object sender,
            ElapsedEventArgs e)
        {
            Debug.WriteLine("Try Ping to Manager");
        }
    }
}