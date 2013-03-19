using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ISuggestedShiftRestrictionExtractor
	{
		IEffectiveRestriction Extract(IShiftProjectionCache shift, ISchedulingOptions schedulingOptions);
	}

	public class SuggestedShiftRestrictionExtractor : ISuggestedShiftRestrictionExtractor
	{
		public IEffectiveRestriction Extract(IShiftProjectionCache shift, ISchedulingOptions schedulingOptions)
		{
			var startTimeLimitation = new StartTimeLimitation();
			var endTimeLimitation = new EndTimeLimitation();
			var workTimeLimitation = new WorkTimeLimitation();

			if (schedulingOptions.UseLevellingSameStartTime)
				startTimeLimitation = new StartTimeLimitation(shift.WorkShiftStartTime, null);
			if (schedulingOptions.UseLevellingSameEndTime)
				endTimeLimitation = new EndTimeLimitation(null, shift.WorkShiftEndTime);

			var restriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation,
			                                           workTimeLimitation, null, null, null,
			                                           new List<IActivityRestriction>());
			return restriction;
		}
	}
}