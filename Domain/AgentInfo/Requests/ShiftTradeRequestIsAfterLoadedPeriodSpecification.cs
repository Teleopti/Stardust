using System.Linq;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class ShiftTradeRequestIsAfterLoadedPeriodSpecification : Specification<IPersonRequest>
	{
		private DateTimePeriod _loadedPeriod;
		private readonly EmptyShiftTradeRequestChecker _emptyShiftTradeRequestChecker;

		public ShiftTradeRequestIsAfterLoadedPeriodSpecification(DateTimePeriod loadedPeriod)
		{
			_loadedPeriod = loadedPeriod;
			_emptyShiftTradeRequestChecker = new EmptyShiftTradeRequestChecker();
		}

		public override bool IsSatisfiedBy(IPersonRequest obj)
		{
			if (obj == null) return false;

			var shiftTrade = obj.Request as IShiftTradeRequest;
			if (shiftTrade == null) return false;

			if (shiftTrade.ShiftTradeSwapDetails.All(
					d => _loadedPeriod.StartDateTime <= d.DateFrom && _loadedPeriod.EndDateTime >= d.DateTo))
				return false;

			var shiftTradeStatus = shiftTrade.GetShiftTradeStatus(_emptyShiftTradeRequestChecker);
			return shiftTradeStatus == ShiftTradeStatus.OkByBothParts;
		}
	}
}