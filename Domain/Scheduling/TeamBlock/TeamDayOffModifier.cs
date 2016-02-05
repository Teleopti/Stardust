using System;
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
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly Func<IScheduleDayForPerson> _scheduleDayForPerson;

		public TeamDayOffModifier(IResourceOptimizationHelper resourceOptimizationHelper, Func<IScheduleDayForPerson> scheduleDayForPerson)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_scheduleDayForPerson = scheduleDayForPerson;
		}

		public void AddDayOffForTeamAndResourceCalculate(
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			ITeamInfo teamInfo, DateOnly dateOnly, IDayOffTemplate dayOffTemplate)
		{
			foreach (var person in teamInfo.GroupMembers)
			{
				AddDayOffForMember(schedulePartModifyAndRollbackService, person, dateOnly, dayOffTemplate, false);
			}

			_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, false);
		}

		public void AddDayOffForMember(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                               IPerson person, DateOnly dateOnly, IDayOffTemplate dayOffTemplate,
		                               bool resourceCalculate)
		{
			IScheduleDay scheduleDay = _scheduleDayForPerson().ForPerson(person, dateOnly);
			if (scheduleDay.SignificantPart() == SchedulePartView.FullDayAbsence || scheduleDay.SignificantPart() == SchedulePartView.ContractDayOff)
				return;

			scheduleDay.DeleteMainShift(scheduleDay);
			scheduleDay.CreateAndAddDayOff(dayOffTemplate);
			schedulePartModifyAndRollbackService.Modify(scheduleDay);

			if (resourceCalculate)
				_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, false);
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