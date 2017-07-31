using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SkillCombinationResource : IEquatable<SkillCombinationResource>
	{
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public double Resource { get; set; }
		public IEnumerable<Guid> SkillCombination { get; set; }
		private DateTimePeriod? _period;

		public TimeSpan GetTimeSpan()
		{
			return EndDateTime.Subtract(StartDateTime);
		}

		public DateTimePeriod Period()
		{
			if (!_period.HasValue)
			_period =  new DateTimePeriod(StartDateTime, EndDateTime);

			return _period.Value;
		}

		public bool Equals(SkillCombinationResource other)
		{
			return StartDateTime == other.StartDateTime && EndDateTime == other.EndDateTime &&
				   !SkillCombination.Except(other.SkillCombination).Any();
		}
	}
}