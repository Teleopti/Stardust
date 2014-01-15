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
		private readonly IHasContractDayOffDefinition _hasContractDayOffDefinition;

		public ScheduleDayDataMapper(IEffectiveRestrictionCreator effectiveRestrictionCreator, IHasContractDayOffDefinition hasContractDayOffDefinition)
		{
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_hasContractDayOffDefinition = hasContractDayOffDefinition;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IScheduleDayData Map(IScheduleDayPro scheduleDayPro, ISchedulingOptions schedulingOptions)
		{
			IScheduleDayData toAdd = new ScheduleDayData(scheduleDayPro.Day);
			IScheduleDay scheduleDay = scheduleDayPro.DaySchedulePart();
			toAdd.IsScheduled = scheduleDay.IsScheduled();
			SchedulePartView significant = scheduleDay.SignificantPart();
			toAdd.IsDayOff = significant == SchedulePartView.DayOff;
			toAdd.IsContractDayOff = _hasContractDayOffDefinition.IsDayOff(scheduleDay);
			IEffectiveRestriction effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay,
			                                                                                                  schedulingOptions);
			if (effectiveRestriction != null)
				toAdd.HaveRestriction = effectiveRestriction.IsRestriction;

			return toAdd;
		}
	}
}