﻿using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IScheduleDayDataMapper
	{
		IScheduleDayData Map(IScheduleDayPro scheduleDayPro, ISchedulingOptions schedulingOptions);
	}

	public class ScheduleDayDataMapper : IScheduleDayDataMapper
	{
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;

		public ScheduleDayDataMapper(IEffectiveRestrictionCreator effectiveRestrictionCreator)
		{
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IScheduleDayData Map(IScheduleDayPro scheduleDayPro, ISchedulingOptions schedulingOptions)
		{
			IScheduleDayData toAdd = new ScheduleDayData(scheduleDayPro.Day);
			IScheduleDay scheduleDay = scheduleDayPro.DaySchedulePart();
			toAdd.IsScheduled = scheduleDay.IsScheduled();
			SchedulePartView significant = scheduleDay.SignificantPart();
			toAdd.IsDayOff = significant == SchedulePartView.DayOff;
			//toAdd.IsContractDayOff = hasDayOffDefinition.IsDayOff();
			IEffectiveRestriction effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay,
			                                                                                                  schedulingOptions);
			toAdd.HaveRestriction = effectiveRestriction.IsRestriction;
			return toAdd;
		}
	}
}