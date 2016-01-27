using System.Diagnostics;
using System.Timers;

namespace NodeTest.Fakes.ElapsedEventHandlers
{
    public class ElapsedEventHandlersFake
    {
        public ElapsedEventHandlersFake()
        {
            // Job done elapsed event handler.
            SendJodDoneElapsedEventHandler = (s, e) => { Debug.WriteLine("Job done elapsed event handler."); };

            // Job canceled elapsed event handler.
            SendJodCanceledElapsedEventHandler = (s,
                e) =>
            { Debug.WriteLine("Job canceled elapsed event handler."); };

            // Job faulted elapsed event handler.
            SendJodFaultedElapsedEventHandler = (s,
                e) =>
            { Debug.WriteLine("Job faulted elapsed event handler."); };

            // Node start up elapsed event handler.
            NodeStartupElapsedEventHandler = (s,
                e) =>
            { Debug.WriteLine("Node start up event handler."); };
        }

        public ElapsedEventHandler NodeStartupElapsedEventHandler { get; private set; }

        public ElapsedEventHandler SendJodDoneElapsedEventHandler { get; private set; }

        public ElapsedEventHandler SendJodFaultedElapsedEventHandler { get; private set; }

        public ElapsedEventHandler SendJodCanceledElapsedEventHandler { get; private set; }
    }
}