using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class SpecificBusinessUnit : ICurrentBusinessUnit
	{
		private readonly IBusinessUnit _businessUnit;

		public SpecificBusinessUnit(IBusinessUnit businessUnit)
		{
			_businessUnit = businessUnit;
		}

		public IBusinessUnit Current()
		{
			return _businessUnit;
		}
	}
}