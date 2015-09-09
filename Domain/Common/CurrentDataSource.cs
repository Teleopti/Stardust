using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class CurrentDataSource : ICurrentDataSource
	{
		private readonly ICurrentIdentity _currentIdentity;

		public static ICurrentDataSource Make()
		{
			return new CurrentDataSource(new CurrentIdentity(new CurrentTeleoptiPrincipal()));
		}

		public CurrentDataSource(ICurrentIdentity currentIdentity)
		{
			_currentIdentity = currentIdentity;
		}

		public IDataSource Current()
		{
			if (DataSourceState.ThreadDataSource != null)
				return DataSourceState.ThreadDataSource;
			var identity = _currentIdentity.Current();
			if (identity != null)
				return identity.DataSource;
			return null;
		}
		
		public string CurrentName()
		{
			return Current().DataSourceName;
		}

	}
}