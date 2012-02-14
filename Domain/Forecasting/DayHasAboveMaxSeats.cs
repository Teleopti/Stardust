using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class DayHasAboveMaxSeats : Specification<IEnumerable<ISkillStaffPeriod>>
	{
		private readonly IntervalHasAboveMaxSeats _intervalHasAboveMaxSeats = new IntervalHasAboveMaxSeats();

		public override bool IsSatisfiedBy(IEnumerable<ISkillStaffPeriod> obj)
		{
			return obj.Any(s => _intervalHasAboveMaxSeats.IsSatisfiedBy(s));
		}
	}
}