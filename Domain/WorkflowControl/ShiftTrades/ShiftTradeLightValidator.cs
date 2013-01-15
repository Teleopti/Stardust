using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class ShiftTradeLightValidator : IShiftTradeLightValidator
	{
		private readonly IEnumerable<IShiftTradeLightSpecification> _specifications;

		public ShiftTradeLightValidator(IEnumerable<IShiftTradeLightSpecification> specifications)
		{
			_specifications = specifications;
		}

		public ShiftTradeRequestValidationResult Validate(ShiftTradeAvailableCheckItem checkItem)
		{
			foreach (var specification in _specifications.Where(specification => !specification.IsSatisfiedBy(checkItem)))
			{
				return new ShiftTradeRequestValidationResult(false, specification.DenyReason);
			}
			return new ShiftTradeRequestValidationResult(true);
		}
	}
}