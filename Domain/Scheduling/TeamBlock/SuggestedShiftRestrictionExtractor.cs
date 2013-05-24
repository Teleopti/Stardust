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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public IEffectiveRestriction Extract(IShiftProjectionCache shift, ISchedulingOptions schedulingOptions)
		{
			var startTimeLimitation = new StartTimeLimitation();
			var endTimeLimitation = new EndTimeLimitation();
			var workTimeLimitation = new WorkTimeLimitation();
			IEditorShift commonMainShift = null;
			IShiftCategory shiftCategory = null;
			if (schedulingOptions.UseTeamBlockSameStartTime)
			{
				var startTime = shift.WorkShiftStartTime;
				startTimeLimitation = new StartTimeLimitation(startTime, startTime);
			}
				
			if (schedulingOptions.UseTeamBlockSameEndTime)
			{
				var endTime = shift.WorkShiftEndTime;
				endTimeLimitation = new EndTimeLimitation(endTime, endTime);
			}

			if (schedulingOptions.UseTeamBlockSameShift)
			{
				commonMainShift = shift.TheMainShift;
			}

			if (schedulingOptions.UseTeamBlockSameShiftCategory)
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