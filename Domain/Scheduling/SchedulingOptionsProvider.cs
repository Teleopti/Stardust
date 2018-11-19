using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingOptionsProvider : ISchedulingOptionsProvider
	{
		private SchedulingOptions _setFromTest;

		public SchedulingOptions Fetch(IDayOffTemplate defaultDayOffTemplate)
		{
			if (_setFromTest != null)
				return _setFromTest.Clone();

			return new SchedulingOptions
			{
				UseAvailability = true,
				UsePreferences = true,
				UseRotations = true,
				UseStudentAvailability = false,
				DayOffTemplate = defaultDayOffTemplate,
				ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
				TagToUseOnScheduling = NullScheduleTag.Instance
			};
		}

		public void SetFromTest(PlanningPeriod planningPeriod, SchedulingOptions schedulingOptions)
		{
			_setFromTest = schedulingOptions;
			
			//hack for now - to be removed when we have one/fewer "settings objects" sent around
			if (schedulingOptions.UseBlock)
			{
				planningPeriod.PlanningGroup.ModifyDefault(x =>
				{
					x.BlockFinderType = schedulingOptions.BlockFinderTypeForAdvanceScheduling;
					x.BlockSameShiftCategory = schedulingOptions.BlockSameShiftCategory;
					x.BlockSameStartTime = schedulingOptions.BlockSameStartTime;
					x.BlockSameShift = schedulingOptions.BlockSameShift;				
				});
			}
		}
	}
}