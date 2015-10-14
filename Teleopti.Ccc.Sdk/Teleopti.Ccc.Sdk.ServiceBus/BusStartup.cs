using System;
using System.Security.Policy;
using System.Security.Principal;
using log4net;
using Rhino.ServiceBus;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class BusStartup : IServiceBusAware
    {
	    private static readonly ILog Logger = LogManager.GetLogger(typeof (BusStartup));
     
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

		public void BusStarted(IServiceBus bus)
        {
        }

        public void BusDisposing(IServiceBus bus)
        {
        }

        public void BusDisposed(IServiceBus bus)
        {
        }
    }
}