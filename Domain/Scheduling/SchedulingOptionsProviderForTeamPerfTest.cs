using System;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.Domain.Scheduling
{
	[RemoveMeWithToggle("Remove when web supports team/block out of the box", Toggles.ResourcePlanner_RunPerfTestAsTeam_43537)]
	public class SchedulingOptionsProviderForTeamPerfTest : ISchedulingOptionsProvider
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;

		public SchedulingOptionsProviderForTeamPerfTest(Func<ISchedulerStateHolder> schedulerStateHolder)
		{
			_schedulerStateHolder = schedulerStateHolder;
		}

		public SchedulingOptions Fetch(IDayOffTemplate defaultDayOffTemplate)
		{
			return new SchedulingOptions
			{
				UseAvailability = false,
				UsePreferences = false,
				UseRotations = false,
				UseStudentAvailability = false,
				DayOffTemplate = _schedulerStateHolder().CommonStateHolder.DefaultDayOffTemplate,
				ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
				TagToUseOnScheduling = NullScheduleTag.Instance,
				UseTeam = true,
				TeamSameShiftCategory = true
			};
		}
	}
}