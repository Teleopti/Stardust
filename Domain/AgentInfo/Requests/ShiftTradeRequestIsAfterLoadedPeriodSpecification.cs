using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class ShiftTradeRequestIsAfterLoadedPeriodSpecification : Specification<IPersonRequest>
	{
		private DateTimePeriod _loadedPeriod;

		public ShiftTradeRequestIsAfterLoadedPeriodSpecification(DateTimePeriod loadedPeriod)
		{
			_loadedPeriod = loadedPeriod;
		}

		public override bool IsSatisfiedBy(IPersonRequest obj)
		{
			return !(obj != null &&
					 obj.Request is IShiftTradeRequest &&
					 _loadedPeriod.Intersect(obj.Request.Period));
		}
	}
}