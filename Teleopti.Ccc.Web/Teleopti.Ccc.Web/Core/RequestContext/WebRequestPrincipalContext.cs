using System.Threading;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class WebRequestPrincipalContext : ICurrentPrincipalContext
	{
		private readonly ICurrentHttpContext _httpContext;
		private readonly IPrincipalFactory _factory;

		public WebRequestPrincipalContext(
			ICurrentHttpContext httpContext,
			IPrincipalFactory factory)
		{
			_httpContext = httpContext;
			_factory = factory;
		}

		public void SetCurrentPrincipal(IPerson loggedOnUser, IDataSource dataSource, IBusinessUnit businessUnit, AuthenticationTypeOption teleoptiAuthenticationType)
		{
			var principal = _factory.MakePrincipal(loggedOnUser, dataSource, businessUnit, teleoptiAuthenticationType);
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