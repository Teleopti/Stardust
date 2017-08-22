using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class CurrentDataSource : ICurrentDataSource
	{
		private readonly ICurrentIdentity _currentIdentity;
		private readonly DataSourceState _state;

		public static ICurrentDataSource Make()
		{
			return new CurrentDataSource(CurrentIdentity.Make(), new DataSourceState());
		}

		public CurrentDataSource(ICurrentIdentity currentIdentity, DataSourceState state)
		{
			_currentIdentity = currentIdentity;
			_state = state;
		}

		public IDataSource Current()
		{
			var current = _state.Get();
			if (current != null)
				return current;

			var identity = _currentIdentity.Current();
			return identity?.DataSource;
		}
		
		public string CurrentName()
		{
			var dataSource = Current();
			return dataSource?.DataSourceName;
		}
	}
}