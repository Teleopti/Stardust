using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class ShiftTradeValidator : IShiftTradeValidator
	{
		private readonly IEnumerable<IShiftTradeSpecification> _shiftTradeSpecifications;

		public ShiftTradeValidator(IEnumerable<IShiftTradeSpecification> shiftTradeSpecifications)
		{
			_shiftTradeSpecifications = shiftTradeSpecifications;
		}

		public ShiftTradeRequestValidationResult Validate(IList<IShiftTradeSwapDetail> shiftTradeDetails)
		{
			foreach (var specification in _shiftTradeSpecifications)
			{
				var result = specification.Validate(shiftTradeDetails);
				if (!result.Value)
				{
					return result;
				}
			}
			return new ShiftTradeRequestValidationResult(true);
		}

		public ShiftTradeRequestValidationResult Validate(IShiftTradeRequest shiftTradeRequest)
		{
			if (new IsShiftTradeRequestNotNullSpecification().IsSatisfiedBy(shiftTradeRequest))
			{
				return Validate(shiftTradeRequest.ShiftTradeSwapDetails);
			}
			return new ShiftTradeRequestValidationResult(false);
		}
	}
}
