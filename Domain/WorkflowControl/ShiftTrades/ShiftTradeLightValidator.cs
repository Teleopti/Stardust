using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class ShiftTradeLightValidator : IShiftTradeLightValidator
	{
		private readonly IEnumerable<ISpecification<ShiftTradeAvailableCheckItem>> _specifications;

		public ShiftTradeLightValidator(IEnumerable<ISpecification<ShiftTradeAvailableCheckItem>> specifications)
		{
			_specifications = specifications;
		}

		public bool Validate(ShiftTradeAvailableCheckItem checkItem)
		{
			return _specifications.All(specification => specification.IsSatisfiedBy(checkItem));
		}
	}
}