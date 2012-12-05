using System.Collections.Generic;
using System.Linq;
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
			if (new IsShiftTradeRequestNotNullSpecification().IsSatisfiedBy(shiftTradeRequest))
			{
				var tradeSwapDetails = shiftTradeRequest.ShiftTradeSwapDetails;

				var lightResult = validateLightSpecs(tradeSwapDetails);
				return lightResult.Value ? Validate(tradeSwapDetails) : lightResult;
			}
			return new ShiftTradeRequestValidationResult(false);
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

		private ShiftTradeRequestValidationResult validateLightSpecs(IEnumerable<IShiftTradeSwapDetail> shiftTradeRequest)
		{
			foreach (var result in shiftTradeRequest.Select(swapDetail => new ShiftTradeAvailableCheckItem
			                                                              	{
			                                                              		DateOnly = swapDetail.DateFrom,
			                                                              		PersonFrom = swapDetail.PersonFrom,
			                                                              		PersonTo = swapDetail.PersonTo
			                                                              	})
						.Select(checkItem => _shiftTradeLightValidator.Validate(checkItem)).Where(result => !result.Value))
			{
				return result;
			}
			return new ShiftTradeRequestValidationResult(true);
		}
	}
}
