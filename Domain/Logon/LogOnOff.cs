using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Logon
{
	public class LogOnOff : ILogOnOff
    {
		private readonly IDataSourceForTenant _dataSource;
		private readonly IBusinessUnitRepository _businessUnits;
		private readonly IPersonRepository _persons;
		private readonly ICurrentTeleoptiPrincipal _principal;
		private readonly ClaimSetForApplicationRole _claimSetForApplicationRole;
		private readonly ICurrentPrincipalContext _currentPrincipalContext;
	    private readonly IPrincipalFactory _principalFactory;
	    private readonly ITokenIdentityProvider _tokenIdentityProvider;

	    public LogOnOff(
			IDataSourceForTenant dataSource,
			IBusinessUnitRepository businessUnits,
			IPersonRepository persons,
			ICurrentTeleoptiPrincipal principal,
			ClaimSetForApplicationRole claimSetForApplicationRole,
			ICurrentPrincipalContext currentPrincipalContext,
			IPrincipalFactory principalFactory,
			ITokenIdentityProvider tokenIdentityProvider)
	    {
		    _dataSource = dataSource;
		    _businessUnits = businessUnits;
		    _persons = persons;
		    _principal = principal;
		    _claimSetForApplicationRole = claimSetForApplicationRole;
		    _currentPrincipalContext = currentPrincipalContext;
		    _principalFactory = principalFactory;
		    _tokenIdentityProvider = tokenIdentityProvider;
	    }

	    public void LogOn(string tenant, IPerson user, Guid businessUnitId)
		    => LogOn(_dataSource.Tenant(tenant), user, _businessUnits.Load(businessUnitId));

		public void LogOn(IDataSource dataSource, IPerson user, IBusinessUnit businessUnit)
		{
			LogOnWithoutPermissions(dataSource, user, businessUnit);
			setupClaims(dataSource.DataSourceName);
		}

		public void LogOnWithoutPermissions(IDataSource dataSource, IPerson user, IBusinessUnit businessUnit)
	    {
		    TokenIdentity token = null;
			if (_tokenIdentityProvider != null)
				token = _tokenIdentityProvider.RetrieveToken();
		    var principal = _principalFactory.MakePrincipal(user, dataSource, businessUnit, token?.OriginalToken);
			_currentPrincipalContext.SetCurrentPrincipal(principal);
    	}

		private void setupClaims(string tenant)
		{
			var principal = _principal.Current();
			var person  = _persons.Get(principal.PersonId);
			foreach (var applicationRole in person.PermissionInformation.ApplicationRoleCollection)
			{
				principal.AddClaimSet(_claimSetForApplicationRole.Transform(applicationRole, tenant));
			}
		}
	}
}