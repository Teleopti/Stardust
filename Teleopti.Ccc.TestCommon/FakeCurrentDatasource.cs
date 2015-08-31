using System;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCurrentDatasource : ICurrentDataSource, IDataSourceScope
	{
		private IDataSource _current;

		public FakeCurrentDatasource()
		{
		}

		public FakeCurrentDatasource(string name)
		{
			FakeName(name);
		}

		public IDataSource Current()
		{
			return _current;
		}

		public string CurrentName()
		{
			if (_current != null)
				return _current.DataSourceName;
			return null;
		}

		public void FakeName(string name)
		{
			_current = new FakeDataSource() {DataSourceName = name};
		}

		public IDisposable OnThisThreadUse(IDataSource dataSource)
		{
			_current = dataSource;
			return new GenericDisposable(() =>
			{
				_current = null;
			});
		}
	}
}