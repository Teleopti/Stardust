using System.Threading;
using Microsoft.IdentityModel.Claims;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext.Initialize
{
	public class WebRequestPrincipalContext : ICurrentPrincipalContext
	{
		private readonly ICurrentHttpContext _httpContext;
		private readonly IPrincipalFactory _factory;
		private readonly ITokenIdentityProvider _tokenIdentityProvider;

		public WebRequestPrincipalContext(
			ICurrentHttpContext httpContext,
			IPrincipalFactory factory, ITokenIdentityProvider tokenIdentityProvider)
		{
			_httpContext = httpContext;
			_factory = factory;
			_tokenIdentityProvider = tokenIdentityProvider;
		}

		public void SetCurrentPrincipal(IPerson loggedOnUser, IDataSource dataSource, IBusinessUnit businessUnit)
		{
			var token = _tokenIdentityProvider.RetrieveToken();
			var principal = _factory.MakePrincipal(loggedOnUser, dataSource, businessUnit, token == null ? null : token.OriginalToken);
			Thread.CurrentPrincipal = principal;
			_httpContext.Current().User = principal;
		}

		public void SetCurrentPrincipal(ITeleoptiPrincipal principal)
		{
			Thread.CurrentPrincipal = principal;
			_httpContext.Current().User = principal;
		}
	}
}