using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class IntervalHasAboveMaxSeats : Specification<ISkillStaffPeriod>
	{
		public override bool IsSatisfiedBy(ISkillStaffPeriod obj)
		{
			return obj.Payload.CalculatedUsedSeats > obj.Payload.MaxSeats;
		}
	}
}