using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCurrentBusinessUnit : ICurrentBusinessUnit
	{
		private readonly IBusinessUnit _businessUnit;

		public FakeCurrentBusinessUnit()
		{
			_businessUnit = BusinessUnitFactory.CreateWithId(" ");
		}

		public IBusinessUnit Current()
		{
			return _businessUnit;
		}
	}
}