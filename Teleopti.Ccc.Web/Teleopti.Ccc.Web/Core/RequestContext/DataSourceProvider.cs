using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class DataSourceProvider : IDataSourceProvider
	{
		private readonly IIdentityProvider _identityProvider;

		public DataSourceProvider(IIdentityProvider identityProvider)
		{
			_identityProvider = identityProvider;
		}

		public IDataSource CurrentDataSource()
		{
			var identity = _identityProvider.Current();
			return identity == null ? null : identity.DataSource;
		}
	}
}