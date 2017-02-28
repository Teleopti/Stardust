using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class ShiftTradeValidator : IShiftTradeValidator
	{
		private readonly IShiftTradeLightValidator _shiftTradeLightValidator;
		private readonly ISpecificationChecker _specificationChecker;

		public ShiftTradeValidator(IShiftTradeLightValidator shiftTradeLightValidator,
			ISpecificationChecker specificationChecker)
		{
			_shiftTradeLightValidator = shiftTradeLightValidator;
			_specificationChecker = specificationChecker;
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
			return _specificationChecker.Check(shiftTradeDetails);
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