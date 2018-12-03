using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
	public class ResctrictionFromRoleModelRestriction : IScheduleRestrictionStrategy
	{
		private readonly ShiftProjectionCache _shift;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private readonly SchedulingOptions _schedulingOptions;

		public ResctrictionFromRoleModelRestriction(ShiftProjectionCache shift, ITeamBlockSchedulingOptions teamBlockSchedulingOptions, SchedulingOptions schedulingOptions)
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
			if (_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions) ||
			    _teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(_schedulingOptions) ||
			    _teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(_schedulingOptions) ||
			    _teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(_schedulingOptions))
			{
				TimeSpan startTime = _shift.WorkShiftStartTime();
				startTimeLimitation = new StartTimeLimitation(startTime, startTime);
			}

			if (_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions) ||
			    _teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(_schedulingOptions) ||
				_teamBlockSchedulingOptions.IsBlockSchedulingWithSameEndTime(_schedulingOptions))
			{
				TimeSpan endTime = _shift.WorkShiftEndTime();
				endTimeLimitation = new EndTimeLimitation(endTime, endTime);
			}

			if (_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions) || 
				_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)||
				_teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions)||
				_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions))
			{
				shiftCategory = _shift.TheWorkShift.ShiftCategory;
			}

			var restriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation,
													   workTimeLimitation, null, null, null,
													   new List<IActivityRestriction>())
			{
				ShiftCategory = shiftCategory,
				CommonMainShift = null
			};

			return restriction;
		}
	}
}
