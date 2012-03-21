using System;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Interfaces.Domain;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    /// <summary>
    /// Abstract class for logon/logoff
    /// </summary>
    public class LogOnOff : ILogOnOff
    {
        private readonly IPrincipalManager _principalManager;

    	public LogOnOff(IPrincipalManager principalManager)
    	{
    		_principalManager = principalManager;
    	}

    	/// <summary>
        /// Logs off the system.
        /// </summary>
        public void LogOff()
        {
            StateHolder.Instance.State.ClearSession();
        }

    	/// <summary>
    	/// Logs on the system.
    	/// </summary>
    	/// <param name="dataSource">The uow factory.</param>
    	/// <param name="loggedOnUser">The logged on user.</param>
    	/// <param name="businessUnit">The business unit.</param>
    	/// <param name="teleoptiAuthenticationType">Win or form authentication?</param>
    	public void LogOn(IDataSource dataSource,
                          IPerson loggedOnUser,
                          IBusinessUnit businessUnit,
								AuthenticationTypeOption teleoptiAuthenticationType)
        {
            _principalManager.SetCurrentPrincipal(loggedOnUser, dataSource, businessUnit, teleoptiAuthenticationType);
            
            SessionData sessionData = new SessionData();
            StateHolder.Instance.State.SetSessionData(sessionData);

        	setRouteDetailsToSignalBroker(dataSource, businessUnit);
        }

    	private static void setRouteDetailsToSignalBroker(IDataSource dataSource, IBusinessUnit businessUnit)
    	{
    		var signalBroker = StateHolder.Instance.State.ApplicationScopeData.Messaging as SignalBroker;
			if (signalBroker == null) return;

    		signalBroker.BusinessUnitId = businessUnit.Id.GetValueOrDefault();
    		signalBroker.DataSource = dataSource.DataSourceName;
    	}
    }
}
