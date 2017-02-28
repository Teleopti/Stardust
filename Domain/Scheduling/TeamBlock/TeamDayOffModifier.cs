﻿using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamDayOffModifier
	{
		void AddDayOffForTeamAndResourceCalculate(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
														   ITeamInfo teamInfo, DateOnly dateOnly, IDayOffTemplate dayOffTemplate);

		void RemoveDayOffForTeam(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                                                      ITeamInfo teamInfo, DateOnly dateOnly);

		void RemoveDayOffForMember(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                                           IPerson person, DateOnly dateOnly);

		void AddDayOffForMember(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, IPerson person,
		                        DateOnly dateOnly, IDayOffTemplate dayOffTemplate, bool resourceCalculate);
	}

	public class TeamDayOffModifier : ITeamDayOffModifier
	{
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly Func<IScheduleDayForPerson> _scheduleDayForPerson;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;

		public TeamDayOffModifier(IResourceCalculation resourceOptimizationHelper, Func<IScheduleDayForPerson> scheduleDayForPerson, Func<ISchedulingResultStateHolder> schedulingResultStateHolder)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_scheduleDayForPerson = scheduleDayForPerson;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public void AddDayOffForTeamAndResourceCalculate(
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			ITeamInfo teamInfo, DateOnly dateOnly, IDayOffTemplate dayOffTemplate)
		{
			var resCalcData = _schedulingResultStateHolder().ToResourceOptimizationData(true, false);
			foreach (var person in teamInfo.GroupMembers)
			{
				AddDayOffForMember(schedulePartModifyAndRollbackService, person, dateOnly, dayOffTemplate, false);
			}

			_resourceOptimizationHelper.ResourceCalculate(dateOnly, resCalcData);
		}

		public void AddDayOffForMember(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                               IPerson person, DateOnly dateOnly, IDayOffTemplate dayOffTemplate,
		                               bool resourceCalculate)
		{
			IScheduleDay scheduleDay = _scheduleDayForPerson().ForPerson(person, dateOnly);
			if (scheduleDay.SignificantPart() == SchedulePartView.FullDayAbsence || scheduleDay.SignificantPart() == SchedulePartView.ContractDayOff)
				return;

			scheduleDay.DeleteMainShift();
			scheduleDay.CreateAndAddDayOff(dayOffTemplate);
			schedulePartModifyAndRollbackService.Modify(scheduleDay);

			if (resourceCalculate)
				_resourceOptimizationHelper.ResourceCalculate(dateOnly, _schedulingResultStateHolder().ToResourceOptimizationData(true, false));
		}

		public void RemoveDayOffForTeam(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                                ITeamInfo teamInfo, DateOnly dateOnly)
		{
			foreach (var person in teamInfo.GroupMembers)
			{
				RemoveDayOffForMember(schedulePartModifyAndRollbackService, person, dateOnly);
			}

		}

		public void RemoveDayOffForMember(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
										  IPerson person, DateOnly dateOnly)
		{
			IScheduleDay scheduleDay = _scheduleDayForPerson().ForPerson(person, dateOnly);
			if (scheduleDay.SignificantPartForDisplay() == SchedulePartView.ContractDayOff)
				return;

			scheduleDay.DeleteDayOff();
			schedulePartModifyAndRollbackService.Modify(scheduleDay);
		}
	}
}