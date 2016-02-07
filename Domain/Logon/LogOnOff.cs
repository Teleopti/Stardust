using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Logon
{
    public class LogOnOff : ILogOnOff
    {
        private readonly ICurrentPrincipalContext _currentPrincipalContext;

    	public LogOnOff(ICurrentPrincipalContext currentPrincipalContext)
    	{
    		_currentPrincipalContext = currentPrincipalContext;
    	}

		public void LogOn(
			IDataSource dataSource,
			IPerson user,
			IBusinessUnit businessUnit)
    	{
    		_currentPrincipalContext.SetCurrentPrincipal(user, dataSource, businessUnit);
    	}
    }
}
