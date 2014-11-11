using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCurrentDatasource : ICurrentDataSource
	{
		private readonly string _name;

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
	}
}