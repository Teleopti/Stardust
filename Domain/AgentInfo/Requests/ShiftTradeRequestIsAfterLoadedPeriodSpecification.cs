using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

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

			if (!(obj.Request is IShiftTradeRequest shiftTrade)) return false;
			if (areAllDetailsOutsidePeriod(shiftTrade)) return false;

			var shiftTradeStatus = shiftTrade.GetShiftTradeStatus(_emptyShiftTradeRequestChecker);
			return shiftTradeStatus == ShiftTradeStatus.OkByBothParts;
		}

		private bool areAllDetailsOutsidePeriod(IShiftTradeRequest shiftTrade)
		{
			return shiftTrade.ShiftTradeSwapDetails.All(
				d => _loadedPeriod.StartDateTime <= d.DateFrom.Date && 
				     _loadedPeriod.EndDateTime >= d.DateTo.Date);
		}
	}
}