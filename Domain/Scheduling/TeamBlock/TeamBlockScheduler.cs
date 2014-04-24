using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockScheduler
	{
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		bool ScheduleTeamBlockDay(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions,
								  DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, 
								ISchedulePartModifyAndRollbackService rollbackService, 
								IResourceCalculateDelayer resourceCalculateDelayer,
								ISchedulingResultStateHolder schedulingResultStateHolder,
								ShiftNudgeDirective shiftNudgeDirective);

		void OnDayScheduled(object sender, SchedulingServiceBaseEventArgs e);
	}

	public class TeamBlockScheduler : ITeamBlockScheduler
	{
		private readonly ITeamBlockSingleDayScheduler _singleDayScheduler;
		private readonly ITeamBlockRoleModelSelector _roleModelSelector;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private bool _cancelMe;

		public TeamBlockScheduler(ITeamBlockSingleDayScheduler singleDayScheduler,
		                          ITeamBlockRoleModelSelector roleModelSelector,
									ITeamBlockClearer teamBlockClearer, ITeamBlockSchedulingOptions teamBlockSchedulingOptions)
		{
			_singleDayScheduler = singleDayScheduler;
			_roleModelSelector = roleModelSelector;
			_teamBlockClearer = teamBlockClearer;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
		}

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public bool ScheduleTeamBlockDay(ITeamBlockInfo teamBlockInfo, DateOnly datePointer,
			ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
			ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			ShiftNudgeDirective shiftNudgeDirective)

		{
			var selectedTeamMembers = teamBlockInfo.TeamInfo.GroupMembers.Intersect(selectedPersons).ToList();
			if (selectedTeamMembers.IsEmpty())
				return true;

			IShiftProjectionCache roleModelShift = _roleModelSelector.Select(teamBlockInfo, datePointer, selectedTeamMembers.First(),
				schedulingOptions, shiftNudgeDirective.EffectiveRestriction);

			if (roleModelShift == null)
			{
				OnDayScheduledFailed();
				return false;
			}

			var selectedBlockDays =
				teamBlockInfo.BlockInfo.BlockPeriod.DayCollection().Where(x => selectedPeriod.DayCollection().Contains(x)).ToList();

			bool success = tryScheduleBlock(teamBlockInfo, schedulingOptions, selectedPeriod, selectedPersons, selectedBlockDays,
				roleModelShift, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, shiftNudgeDirective);

			if (!success && _teamBlockSchedulingOptions.IsBlockWithSameShiftCategoryInvolved(schedulingOptions))
			{
				schedulingOptions.NotAllowedShiftCategories.Clear();
				while (roleModelShift != null && !success)
				{
					_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
					schedulingOptions.NotAllowedShiftCategories.Add(roleModelShift.TheMainShift.ShiftCategory);
					roleModelShift = _roleModelSelector.Select(teamBlockInfo, datePointer, selectedTeamMembers.First(),
						schedulingOptions, shiftNudgeDirective.EffectiveRestriction);
					success = tryScheduleBlock(teamBlockInfo, schedulingOptions, selectedPeriod, selectedPersons, selectedBlockDays,
						roleModelShift, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, shiftNudgeDirective);
				}
				schedulingOptions.NotAllowedShiftCategories.Clear();	
			}

			return success;


		}

		private bool tryScheduleBlock(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions,
			DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, IList<DateOnly> selectedBlockDays,
			IShiftProjectionCache roleModelShift, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder,
			ShiftNudgeDirective shiftNudgeDirective)
		{
			var lastIndex = selectedBlockDays.Count - 1;
			IEffectiveRestriction shiftNudgeRestriction = new EffectiveRestriction();
			if (shiftNudgeDirective.Direction == ShiftNudgeDirective.NudgeDirection.Right)
				shiftNudgeRestriction = shiftNudgeDirective.EffectiveRestriction;

			for (int dayIndex = 0; dayIndex <= lastIndex; dayIndex++)
			{
				var day = selectedBlockDays[dayIndex];
				if (_cancelMe)
					return false;

				if (shiftNudgeDirective.Direction == ShiftNudgeDirective.NudgeDirection.Left && dayIndex == lastIndex)
				
				_singleDayScheduler.DayScheduled += OnDayScheduled;
				bool successful = _singleDayScheduler.ScheduleSingleDay(teamBlockInfo, schedulingOptions, selectedPersons, day,
					roleModelShift, selectedPeriod, rollbackService,
					resourceCalculateDelayer, schedulingResultStateHolder, shiftNudgeRestriction);
				_singleDayScheduler.DayScheduled -= OnDayScheduled;

				if(shiftNudgeDirective.Direction == ShiftNudgeDirective.NudgeDirection.Right && dayIndex == 0)
					shiftNudgeRestriction = new EffectiveRestriction();

				if (!successful)
				{
					OnDayScheduledFailed();
					return false;
				}
			}
			
			return true;
		}

		public void OnDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
			if (temp != null)
			{
				temp(this, e);
			}
			_cancelMe = e.Cancel;
		}

		public void OnDayScheduledFailed()
		{
			var temp = DayScheduled;
			if (temp != null)
			{
				temp(this, new SchedulingServiceFailedEventArgs());
			}
		}
	}
}