using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
	public class ResctrictionFromRoleModelRestriction : IScheduleRestrictionStrategy
	{
		private readonly IShiftProjectionCache _shift;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private readonly ISchedulingOptions _schedulingOptions;

		public ResctrictionFromRoleModelRestriction(IShiftProjectionCache shift, ITeamBlockSchedulingOptions teamBlockSchedulingOptions, ISchedulingOptions schedulingOptions)
		{
			_shift = shift;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_schedulingOptions = schedulingOptions;
		}

		public IEffectiveRestriction ExtractRestriction(IList<DateOnly> dateOnlyList, IList<IScheduleMatrixPro> matrixList)
		{
			var startTimeLimitation = new StartTimeLimitation();
			var endTimeLimitation = new EndTimeLimitation();
			var workTimeLimitation = new WorkTimeLimitation();
			IShiftCategory shiftCategory = null;
			if (_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions))
			{
				TimeSpan startTime = _shift.WorkShiftStartTime;
				startTimeLimitation = new StartTimeLimitation(startTime, startTime);
			}

			if (_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions))
			{
				TimeSpan endTime = _shift.WorkShiftEndTime;
				endTimeLimitation = new EndTimeLimitation(endTime, endTime);
			}

			if (_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions))
			{
				shiftCategory = _shift.TheWorkShift.ShiftCategory;
			}

			var restriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation,
													   workTimeLimitation, null, null, null,
													   new List<IActivityRestriction>())
			{
				ShiftCategory = shiftCategory
			};

			return restriction;
		}
	}
}
