using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCurrentDatasource : ICurrentDataSource
	{
		private string _name;

		public FakeCurrentDatasource()
		{
		}

		public FakeCurrentDatasource(string name)
		{
			_name = name;
		}

		public IDataSource Current()
		{
			return null;
		}

		public string CurrentName()
		{
			return _name;
		}

		public void FakeName(string name)
		{
			_name = name;
		}
	}
}