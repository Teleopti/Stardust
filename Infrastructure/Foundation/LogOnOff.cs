using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    /// <summary>
    /// Abstract class for logon/logoff
    /// </summary>
    public class LogOnOff : ILogOnOff
    {
        private readonly ICurrentPrincipalContext _currentPrincipalContext;

    	public LogOnOff(ICurrentPrincipalContext currentPrincipalContext)
    	{
    		_currentPrincipalContext = currentPrincipalContext;
    	}

    	/// <summary>
        /// Logs off the system.
        /// </summary>
        public void LogOff()
        {
        }

    	/// <summary>
    	/// Logs on the system.
    	/// </summary>
    	/// <param name="dataSource">The uow factory.</param>
    	/// <param name="loggedOnUser">The logged on user.</param>
    	/// <param name="businessUnit">The business unit.</param>
		public void LogOn(
			IDataSource dataSource,
			IPerson loggedOnUser,
			IBusinessUnit businessUnit)
    	{
    		_currentPrincipalContext.SetCurrentPrincipal(loggedOnUser, dataSource, businessUnit);
    	}
    }
}
