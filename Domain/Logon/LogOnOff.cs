using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Logon
{
	public class LogOnOff : ILogOnOff
    {
		private readonly IDataSourceForTenant _dataSource;
		private readonly IBusinessUnitRepository _businessUnits;
		private readonly ICurrentPrincipalContext _currentPrincipalContext;
	    private readonly IPrincipalFactory _principalFactory;
	    private readonly ITokenIdentityProvider _tokenIdentityProvider;

	    public LogOnOff(
			IDataSourceForTenant dataSource,
			IBusinessUnitRepository businessUnits,
			ICurrentPrincipalContext currentPrincipalContext,
			IPrincipalFactory principalFactory,
			ITokenIdentityProvider tokenIdentityProvider)
	    {
		    _dataSource = dataSource;
		    _businessUnits = businessUnits;
		    _currentPrincipalContext = currentPrincipalContext;
		    _principalFactory = principalFactory;
		    _tokenIdentityProvider = tokenIdentityProvider;
	    }

		public void LogOn(string tenant, IPerson user, Guid businessUnitId)
		{
			LogOn(_dataSource.Tenant(tenant), user, _businessUnits.Load(businessUnitId));
		}

		public void LogOn(IDataSource dataSource, IPerson user, IBusinessUnit businessUnit)
	    {
		    TokenIdentity token = null;
			if (_tokenIdentityProvider != null)
				token = _tokenIdentityProvider.RetrieveToken();
		    var principal = _principalFactory.MakePrincipal(user, dataSource, businessUnit, token?.OriginalToken);
			_currentPrincipalContext.SetCurrentPrincipal(principal);
    	}
    }
}