using System.Collections.Generic;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	/// <summary>
	/// Specification to filter out those days that are not about the days in the shift trade swap details
	/// </summary>
	public class BusinessRuleResponseContainsDateSpecification : Specification<IBusinessRuleResponse>
	{
		private readonly IEnumerable<IShiftTradeSwapDetail> _shiftTradeSwapDetails;

		public BusinessRuleResponseContainsDateSpecification(IEnumerable<IShiftTradeSwapDetail> shiftTradeSwapDetails)
		{
			_shiftTradeSwapDetails = shiftTradeSwapDetails;
		}

		public override bool IsSatisfiedBy(IBusinessRuleResponse obj)
		{
			foreach (IShiftTradeSwapDetail swapDetail in _shiftTradeSwapDetails)
			{
				if (obj.DateOnlyPeriod.Contains(swapDetail.DateFrom))
					return true;
				if (obj.DateOnlyPeriod.Contains(swapDetail.DateTo))
					return true;
			}
			return false;
		}

	}
}