using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SkillCombinationResource
	{
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public double Resource { get; set; }
		public IEnumerable<Guid> SkillCombination { get; set; }

		public TimeSpan GetTimeSpan()
		{
			return EndDateTime.Subtract(StartDateTime);
		}

		public DateTimePeriod Period()
		{
			return new DateTimePeriod(StartDateTime, EndDateTime);
		}
	}
}