using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Logon
{
    public class LogOnOff : ILogOnOff
    {
        private readonly ICurrentPrincipalContext _currentPrincipalContext;
	    private readonly IPrincipalFactory _principalFactory;
	    private readonly ITokenIdentityProvider _tokenIdentityProvider;

	    public LogOnOff(
			ICurrentPrincipalContext currentPrincipalContext, 
			IPrincipalFactory principalFactory,
			ITokenIdentityProvider tokenIdentityProvider)
	    {
		    _currentPrincipalContext = currentPrincipalContext;
		    _principalFactory = principalFactory;
		    _tokenIdentityProvider = tokenIdentityProvider;
	    }

	    public void LogOn(IDataSource dataSource, IPerson user, IBusinessUnit businessUnit)
	    {
		    TokenIdentity token = null;
			if (_tokenIdentityProvider != null)
				token = _tokenIdentityProvider.RetrieveToken();
		    var principal = _principalFactory.MakePrincipal(user, dataSource, businessUnit, token == null ? null : token.OriginalToken);
			_currentPrincipalContext.SetCurrentPrincipal(principal);
    	}
    }
}
