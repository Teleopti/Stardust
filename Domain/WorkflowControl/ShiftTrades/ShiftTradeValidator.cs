using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class ShiftTradeValidator : IShiftTradeValidator
	{
		private readonly IShiftTradeLightValidator _shiftTradeLightValidator;
		private readonly IEnumerable<IShiftTradeSpecification> _shiftTradeSpecifications;

		public ShiftTradeValidator(IShiftTradeLightValidator shiftTradeLightValidator, IEnumerable<IShiftTradeSpecification> shiftTradeSpecifications)
		{
			_shiftTradeLightValidator = shiftTradeLightValidator;
			_shiftTradeSpecifications = shiftTradeSpecifications;
		}

		public ShiftTradeRequestValidationResult Validate(IShiftTradeRequest shiftTradeRequest)
		{
			if (!new IsShiftTradeRequestNotNullSpecification().IsSatisfiedBy(shiftTradeRequest))
				return new ShiftTradeRequestValidationResult(false);

			var tradeSwapDetails = shiftTradeRequest.ShiftTradeSwapDetails;
			var lightResult = validateLightSpecs(tradeSwapDetails);
			return lightResult.IsOk ? Validate(tradeSwapDetails) : lightResult;
		}

		public ShiftTradeRequestValidationResult Validate(IList<IShiftTradeSwapDetail> shiftTradeDetails)
		{
			foreach (var specification in _shiftTradeSpecifications)
			{
				var result = specification.Validate(shiftTradeDetails);
				if (!result.IsOk)
				{
					return result;
				}
			}
			return new ShiftTradeRequestValidationResult(true);
		}

		private ShiftTradeRequestValidationResult validateLightSpecs(IEnumerable<IShiftTradeSwapDetail> shiftTradeRequestDetails)
		{
			foreach (var swapDetail in shiftTradeRequestDetails)
			{
				var checkItem = new ShiftTradeAvailableCheckItem(swapDetail.DateFrom, swapDetail.PersonFrom, swapDetail.PersonTo);
				var checkResult = _shiftTradeLightValidator.Validate(checkItem);
				if (!checkResult.IsOk) return checkResult;
			}

			return new ShiftTradeRequestValidationResult(true);
		}
	}
}