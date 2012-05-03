using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class CurrentDataSourceProvider : ICurrentDataSourceProvider
	{
		private readonly IPrincipalProvider _principalProvider;


		public CurrentDataSourceProvider(IPrincipalProvider principalProvider)
		{
			_principalProvider = principalProvider;
		}

		public IDataSource CurrentDataSource()
		{
			TeleoptiPrincipal teleoptiPrincipal = _principalProvider.Current();
			return teleoptiPrincipal == null ? null : ((TeleoptiIdentity) teleoptiPrincipal.Identity).DataSource;
		}
	}
}