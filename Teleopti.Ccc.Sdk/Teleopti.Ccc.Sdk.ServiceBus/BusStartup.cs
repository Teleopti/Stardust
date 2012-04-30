using System;
using System.Security.Policy;
using System.Security.Principal;
using log4net;
using Rhino.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class BusStartup : IServiceBusAware
    {
        private readonly IInitializePayrollFormats _initializePayrollFormats;
    	private static readonly ILog Logger = LogManager.GetLogger(typeof (BusStartup));
        
        public BusStartup(IInitializePayrollFormats initializePayrollFormats)
        {
            _initializePayrollFormats = initializePayrollFormats;
        }

        public void BusStarting(IServiceBus bus)
        {
        	setDefaultGenericPrincipal();
        }

    	private static void setDefaultGenericPrincipal()
    	{
    		try
			{
				Logger.Debug("Trying to set default generic principal.");
				AppDomain.CurrentDomain.SetThreadPrincipal(new GenericPrincipal(new GenericIdentity("Anonymous"), new string[] { }));
    		}
    		catch (PolicyException policyException)
    		{
    			Logger.Warn("Failed to set thread principal for app domain.",policyException);
    		}
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