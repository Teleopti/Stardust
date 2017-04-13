using System;
using System.Windows.Threading;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public interface IDispatcherWrapper
    {
        void BeginInvoke(Delegate method);
    }

    public class DispatcherWrapper : IDispatcherWrapper
    {
        private readonly Dispatcher _dispatcher;

        public DispatcherWrapper()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public void BeginInvoke(Delegate method)
        {
            _dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,method);
        }
    }
}
