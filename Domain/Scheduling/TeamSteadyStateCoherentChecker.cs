using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ITeamSteadyStateCoherentChecker
	{
		IScheduleDay CheckCoherent(IEnumerable<IScheduleMatrixPro> matrixes, DateOnly dateOnly, IScheduleDictionary scheduleDictionary, IScheduleDay scheduleDay, IEnumerable<IPerson> groupMembers);
	}

	public class TeamSteadyStateCoherentChecker : ITeamSteadyStateCoherentChecker
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IScheduleDay CheckCoherent(IEnumerable<IScheduleMatrixPro> matrixes, DateOnly dateOnly, IScheduleDictionary scheduleDictionary, IScheduleDay scheduleDay, IEnumerable<IPerson> groupMembers)
		{
			foreach (var scheduleMatrixPro in matrixes)
			{
				if (!groupMembers.Contains(scheduleMatrixPro.Person)) continue;
				var scheduleRangeSource = scheduleDictionary[scheduleMatrixPro.Person];
				var scheduleDaySource = scheduleRangeSource.ScheduledDay(dateOnly);
				var schedulePartViewSource = scheduleDaySource.SignificantPart();

				if (schedulePartViewSource == SchedulePartView.MainShift)
				{
					if (scheduleDay.SignificantPart() == SchedulePartView.MainShift)
					{
						if (!scheduleDay.AssignmentHighZOrder().Period.Equals(scheduleDaySource.AssignmentHighZOrder().Period))
							return null;
					}
					else
					{
						scheduleDay = scheduleDaySource;
					}
				}
			}

			return scheduleDay;
		}	
	}
}
