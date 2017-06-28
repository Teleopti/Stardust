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
				return _setFromTest;

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

		public void SetFromTest(SchedulingOptions schedulingOptions)
		{
			_setFromTest = schedulingOptions;
		}
	}
}