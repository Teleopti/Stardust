using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ITeamSteadyStateScheduleMatrixProFinder 
	{
		IScheduleMatrixPro MatrixPro(IEnumerable<IScheduleMatrixPro> matrixes, IScheduleDay scheduleDay);
	}

	public class TeamSteadyStateScheduleMatrixProFinder : ITeamSteadyStateScheduleMatrixProFinder
	{
		public IScheduleMatrixPro MatrixPro(IEnumerable<IScheduleMatrixPro> matrixes, IScheduleDay scheduleDay)
		{
			if(matrixes == null) throw new ArgumentNullException("matrixes");
			if(scheduleDay == null) throw new ArgumentNullException("scheduleDay");

			foreach (var scheduleMatrixPro in matrixes)
			{
				if (scheduleMatrixPro.Person != scheduleDay.Person) continue;
				if (scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.Contains(scheduleDay.DateOnlyAsPeriod.DateOnly))
					return scheduleMatrixPro;
			}

			return null;
		}
	}
}
