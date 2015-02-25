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
    			Logger.Info("Failed to set thread principal for app domain, because it was already set.",policyException);
    		}
    	}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void BusStarted(IServiceBus bus)
        {
			if(bus.Endpoint.Uri.AbsolutePath.Equals("/payroll"))
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