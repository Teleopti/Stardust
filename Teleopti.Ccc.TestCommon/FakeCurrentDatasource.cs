using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCurrentDatasource : ICurrentDataSource
	{
		private readonly DataSourceState _state;
		private IDataSource _current;

		public FakeCurrentDatasource(DataSourceState state)
		{
			_state = state;
		}

		public FakeCurrentDatasource(string name)
		{
			FakeName(name);
		}

		public IDataSource Current()
		{
			if (_current != null)
				return _current;
			if (_state.Get() != null)
				return _state.Get();
			return null;
		}

		public string CurrentName()
		{
			if (Current() != null)
				return Current().DataSourceName;
			return null;
		}

		public void FakeName(string name)
		{
			_current = new FakeDataSource {DataSourceName = name};
		}
	}
}