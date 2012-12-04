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

		public bool Validate(ShiftTradeAvailableCheckItem checkItem)
		{
			return _specifications.All(specification => specification.IsSatisfiedBy(checkItem));
		}
	}
}