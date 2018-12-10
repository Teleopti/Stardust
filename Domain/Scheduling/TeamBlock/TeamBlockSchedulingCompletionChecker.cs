using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockSchedulingCompletionChecker
	{
		bool IsDayScheduledInTeamBlock(ITeamBlockInfo teamBlockInfo, DateOnly dateOnly, SchedulingOptions schedulingOptions);

		bool IsDayScheduledInTeamBlockForSelectedPersons(ITeamBlockInfo teamBlockInfo, DateOnly dateOnly,
																		 IList<IPerson> selectedPersons, SchedulingOptions schedulingOptions);
	}

	public class TeamBlockSchedulingCompletionChecker : ITeamBlockSchedulingCompletionChecker
	{
		public bool IsDayScheduledInTeamBlock(ITeamBlockInfo teamBlockInfo, DateOnly dateOnly, SchedulingOptions schedulingOptions)
		{
			if (teamBlockInfo == null) return false;

			foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dateOnly))
			{
				if (!checkScheduleStatus(dateOnly, matrix, schedulingOptions)) return false;
			}

			return true;
		}

	    private bool checkScheduleStatus(DateOnly dateOnly, IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions)
	    {
	        IScheduleRange rangeForPerson = matrix.ActiveScheduleRange;
	        IScheduleDay scheduleDay = rangeForPerson.ScheduledDay(dateOnly);
	        if (!schedulingOptions.IsDayScheduled(scheduleDay))
	        {
	            return false;
	        }
	        return true;
	    }

	    public bool IsDayScheduledInTeamBlockForSelectedPersons(ITeamBlockInfo teamBlockInfo, DateOnly dateOnly,
		                                                               IList<IPerson> selectedPersons, SchedulingOptions schedulingOptions)
		{
			if (teamBlockInfo == null) return false;

			foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dateOnly))
			{
				if (!selectedPersons.Contains(matrix.Person)) continue;
                if (!checkScheduleStatus(dateOnly, matrix, schedulingOptions)) 
					return false;
			}

			return true;

		}
	}
}