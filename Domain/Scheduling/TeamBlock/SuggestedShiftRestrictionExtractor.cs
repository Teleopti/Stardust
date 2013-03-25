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
			IMainShift commonMainShift = null;
			IShiftCategory shiftCategory = null;
			if (schedulingOptions.UseLevellingSameStartTime)
			{
				var startTime = shift.WorkShiftStartTime;
				startTimeLimitation = new StartTimeLimitation(startTime, startTime);
			}
				
			if (schedulingOptions.UseLevellingSameEndTime)
			{
				var endTime = shift.WorkShiftEndTime;
				endTimeLimitation = new EndTimeLimitation(endTime, endTime);
			}

			if (schedulingOptions.UseLevellingSameShift)
			{
				commonMainShift = shift.TheMainShift;
			}

			if (schedulingOptions.UseLevellingSameShiftCategory)
			{
				shiftCategory = shift.TheWorkShift.ShiftCategory;
			}

			var restriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation,
			                                           workTimeLimitation, null, null, null,
			                                           new List<IActivityRestriction>())
				{
					CommonMainShift = commonMainShift,
					ShiftCategory = shiftCategory
				};

			return restriction;
		}
	}
}