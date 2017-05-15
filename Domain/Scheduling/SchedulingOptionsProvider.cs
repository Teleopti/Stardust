using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingOptionsProvider : ISchedulingOptionsProvider
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private SchedulingOptions _setFromTest;

		public SchedulingOptionsProvider(Func<ISchedulerStateHolder> schedulerStateHolder)
		{
			_schedulerStateHolder = schedulerStateHolder;
		}

		public SchedulingOptions Fetch()
		{
			if (_setFromTest != null)
				return _setFromTest;

			return new SchedulingOptions
			{
				UseAvailability = true,
				UsePreferences = true,
				UseRotations = true,
				UseStudentAvailability = false,
				DayOffTemplate = _schedulerStateHolder().CommonStateHolder.DefaultDayOffTemplate,
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