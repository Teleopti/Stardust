using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockSchedulingCompletionChecker
	{
		bool IsDayScheduledInTeamBlock(ITeamBlockInfo teamBlockInfo, DateOnly dateOnly);

		bool IsDayScheduledInTeamBlockForSelectedPersons(ITeamBlockInfo teamBlockInfo, DateOnly dateOnly,
																		 IList<IPerson> selectedPersons);

		bool IsTeamBlockScheduledForSelectedPersons(ITeamBlockInfo teamBlockInfo, IList<IPerson> selectedPersons);
	}

	public class TeamBlockSchedulingCompletionChecker : ITeamBlockSchedulingCompletionChecker
	{
		public bool IsDayScheduledInTeamBlock(ITeamBlockInfo teamBlockInfo, DateOnly dateOnly)
		{
			if (teamBlockInfo == null) return false;

			foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dateOnly))
			{
				if (!checkScheduleStatus(dateOnly, matrix)) return false;
			}

			return true;
		}

	    private bool checkScheduleStatus(DateOnly dateOnly, IScheduleMatrixPro matrix)
	    {
	        IScheduleRange rangeForPerson = matrix.SchedulingStateHolder.Schedules[matrix.Person];
	        IScheduleDay scheduleDay = rangeForPerson.ScheduledDay(dateOnly);
	        if (!scheduleDay.IsScheduled())
	        {
	            return false;
	        }
	        return true;
	    }

	    public bool IsDayScheduledInTeamBlockForSelectedPersons(ITeamBlockInfo teamBlockInfo, DateOnly dateOnly,
		                                                               IList<IPerson> selectedPersons)
		{
			if (teamBlockInfo == null) return false;

			foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dateOnly))
			{
				if (!selectedPersons.Contains(matrix.Person)) continue;
                if (!checkScheduleStatus(dateOnly, matrix)) 
					return false;
			}

			return true;

		}

		public bool IsTeamBlockScheduledForSelectedPersons(ITeamBlockInfo teamBlockInfo, IList<IPerson> selectedPersons)
		{
			if (teamBlockInfo == null) return false;

			foreach (var dateOnly in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
			{
				foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dateOnly))
				{
					if (!selectedPersons.Contains(matrix.Person)) continue;
                    if (!checkScheduleStatus(dateOnly, matrix)) return false;
				}
			}
			return true;
		}
	}
}