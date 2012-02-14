using Rhino.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class BusStartup : IServiceBusAware
    {
        private readonly IInitializePayrollFormats _initializePayrollFormats;
        
        public BusStartup(IInitializePayrollFormats initializePayrollFormats)
        {
            _initializePayrollFormats = initializePayrollFormats;
        }

        public void BusStarting(IServiceBus bus)
        {
            
        }

        public void BusStarted(IServiceBus bus)
        {
            _initializePayrollFormats.Initialize();
        }

        public void BusDisposing(IServiceBus bus)
        {
        }

        public void BusDisposed(IServiceBus bus)
        {
        }
    }
}