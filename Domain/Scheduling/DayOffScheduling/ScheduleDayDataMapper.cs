using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IScheduleDayDataMapper
	{
		IScheduleDayData Map(IScheduleDayPro scheduleDayPro, SchedulingOptions schedulingOptions);
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
		public IScheduleDayData Map(IScheduleDayPro scheduleDayPro, SchedulingOptions schedulingOptions)
		{
			IScheduleDayData toAdd = new ScheduleDayData(scheduleDayPro.Day);
			IScheduleDay scheduleDay = scheduleDayPro.DaySchedulePart();
			toAdd.IsScheduled = scheduleDay.IsScheduled();
			SchedulePartView significant = scheduleDay.SignificantPart();
			toAdd.IsDayOff = significant == SchedulePartView.DayOff;
			toAdd.IsContractDayOff = _hasContractDayOffDefinition.IsDayOff(scheduleDay);
			IEffectiveRestriction effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay,
			                                                                                                  schedulingOptions);
			if (effectiveRestriction == null)
				return toAdd;

			if(effectiveRestriction.IsAvailabilityDay && !schedulingOptions.AvailabilityDaysOnly)
				return toAdd;
				
			toAdd.HaveRestriction = effectiveRestriction.IsRestriction;
			if(toAdd.IsDayOff && !toAdd.IsScheduled)
				return null;
			return toAdd;
		}
	}
}