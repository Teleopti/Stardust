using System;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingOptionsProvider : ISchedulingOptionsProvider
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private ISchedulingOptions _setFromTest;

		public SchedulingOptionsProvider(Func<ISchedulerStateHolder> schedulerStateHolder)
		{
			_schedulerStateHolder = schedulerStateHolder;
		}

		public ISchedulingOptions Fetch()
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
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
				TagToUseOnScheduling = NullScheduleTag.Instance
			};
		}

		public void SetFromTest(ISchedulingOptions schedulingOptions)
		{
			_setFromTest = schedulingOptions;
		}
	}
}