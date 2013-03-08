

using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface ITeamBlockDayOffOptimizerService
	{
	}

	public class TeamBlockDayOffOptimizerService : ITeamBlockDayOffOptimizerService
	{

		public void OptimizeDaysOff(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons)
		{
			foreach (var datePointer in selectedPeriod.DayCollection())
			{
				var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
				foreach (var selectedPerson in selectedPersons)
				{
					//allTeamInfoListOnStartDate.Add(_teamInfoFactory.CreateTeamInfo(selectedPerson, datePointer, allPersonMatrixList));
				}
			}
		}
		// create a list of all teamInfos
		// find a random selected TeamInfo/matrix
 		// find days off to move within the common matrix period
		// execute do moves
		// ev back to legal state?
		// if possible reschedule block without clearing
		// else
		//	clear involved teamblocks
		//	reschedule involved teamblocks
		// rollback id failed or not good
		// remember not to break anything in shifts or restrictions
	}
}