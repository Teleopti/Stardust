using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class CurrentDataSource : ICurrentDataSource
	{
		private readonly ICurrentIdentity _currentIdentity;
		private readonly DataSourceState _state;

		public static ICurrentDataSource Make()
		{
			return new CurrentDataSource(new CurrentIdentity(new CurrentTeleoptiPrincipal()), new DataSourceState());
		}

		public CurrentDataSource(ICurrentIdentity currentIdentity, DataSourceState state)
		{
			_currentIdentity = currentIdentity;
			_state = state;
		}

		public IDataSource Current()
		{
			if (_state.Get() != null)
				return _state.Get();
			var identity = _currentIdentity.Current();
			if (identity != null)
				return identity.DataSource;
			return null;
		}
		
		public string CurrentName()
		{
			return Current() == null ? null : Current().DataSourceName;
		}
	}
}