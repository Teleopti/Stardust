using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class CurrentDataSourceProvider : ICurrentDataSourceProvider
	{
		private readonly ICurrentPrincipalProvider _currentPrincipalProvider;


		public CurrentDataSourceProvider(ICurrentPrincipalProvider currentPrincipalProvider)
		{
			_currentPrincipalProvider = currentPrincipalProvider;
		}

		public IDataSource CurrentDataSource()
		{
			TeleoptiPrincipal teleoptiPrincipal = _currentPrincipalProvider.Current();
			return teleoptiPrincipal == null ? null : ((TeleoptiIdentity) teleoptiPrincipal.Identity).DataSource;
		}
	}
}