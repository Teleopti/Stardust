using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class CurrentDataSource : ICurrentDataSource
	{
		private readonly ICurrentIdentity _currentIdentity;

		public CurrentDataSource(ICurrentIdentity currentIdentity)
		{
			_currentIdentity = currentIdentity;
		}

		public IDataSource Current()
		{
			var identity = _currentIdentity.Current();
			return identity == null ? null : identity.DataSource;
		}

		public string CurrentName()
		{
			return Current().DataSourceName;
		}

	}
}