using System;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.Domain.Scheduling
{

	public class SchedulingUseTeamProvider : ISchedulingUseTeamProvider
	{
		private readonly IPlanningGroupSettingsProvider _planningGroupSettingsProvider;
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;

		public SchedulingUseTeamProvider(IPlanningGroupSettingsProvider planningGroupSettingsProvider, ISchedulingOptionsProvider schedulingOptionsProvider)
		{
			_planningGroupSettingsProvider = planningGroupSettingsProvider;
			_schedulingOptionsProvider = schedulingOptionsProvider;
		}

		public bool Fetch(Guid planningPeriodId)
		{
			var allSettingsForPlanningGroup = _planningGroupSettingsProvider.Execute(planningPeriodId);
			return allSettingsForPlanningGroup != null
				? allSettingsForPlanningGroup.TeamSettings.GroupPageType != GroupPageType.SingleAgent
				: _schedulingOptionsProvider.Fetch(null).UseTeam;
		}
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_TeamSchedulingInPlans_79283)]
	public class SchedulingUseTeamProviderOld : ISchedulingUseTeamProvider
	{
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;

		public SchedulingUseTeamProviderOld(ISchedulingOptionsProvider schedulingOptionsProvider)
		{
			_schedulingOptionsProvider = schedulingOptionsProvider;
		}

		public bool Fetch(Guid planningPeriodId)
		{
			return _schedulingOptionsProvider.Fetch(null).UseTeam;
		}
	}

	public interface ISchedulingUseTeamProvider
	{
		bool Fetch(Guid planningPeriodId);
	}

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

		public void SetFromTest_LegacyDONOTUSE(PlanningPeriod planningPeriod, SchedulingOptions schedulingOptions)
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