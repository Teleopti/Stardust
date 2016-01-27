using System;
using System.Diagnostics;
using System.Timers;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;

namespace NodeTest.Fakes.Timers
{
    public class NodeStartupNotificationToManagerFake : TrySendNodeStartUpNotificationToManagerTimer
    {
        public NodeStartupNotificationToManagerFake(INodeConfiguration nodeConfiguration = null,
            Uri callbackUri = null,
            ElapsedEventHandler overrideElapsedEventHandler = null,
            double interval = 3000) : base(nodeConfiguration,
                callbackUri,
                overrideElapsedEventHandler,
                interval)
        {
        }

        private void OnTimedEvent(object sender,
            ElapsedEventArgs e)
        {
            Debug.WriteLine("Try send node start up notification to manager.");
        }
    }
}