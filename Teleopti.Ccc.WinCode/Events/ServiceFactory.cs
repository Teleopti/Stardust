using Microsoft.Practices.Composite.Events;

namespace Teleopti.Ccc.WinCode.Events
{
    public class ServiceFactory:IServiceFactory
    {
        // Singleton instance of the EventAggregator service
        private static EventAggregator _eventService;

        // Lock (sync) object
        private static object _syncRoot = new object();

        public IEventAggregator EventService
        {
            get
            {
                // Lock execution thread in case of multi-threaded
                // (concurrent) access.
                lock (_syncRoot)
                {
                    if (null == _eventService)
                    {
                        _eventService = new EventAggregator();
                    }
                    // Return singleton instance
                    return _eventService;
                } // lock
            }
        }
    }
}