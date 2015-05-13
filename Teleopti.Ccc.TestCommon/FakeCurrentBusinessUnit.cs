using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCurrentBusinessUnit : ICurrentBusinessUnit
	{
		private IBusinessUnit _businessUnit;

		public void FakeBusinessUnit(IBusinessUnit businessUnit)
		{
			_businessUnit = businessUnit;
		}

		public IBusinessUnit Current()
		{
			return _businessUnit;
		}
	}

}