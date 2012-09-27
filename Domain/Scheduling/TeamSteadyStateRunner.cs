using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class TeamSteadyStateRunner
	{
		private readonly IList<IScheduleMatrixPro> _matrixList;
		private readonly ISchedulePeriodTargetTimeCalculator _schedulePeriodTargetTimeCalculator;

		public TeamSteadyStateRunner(IList<IScheduleMatrixPro> matrixList, ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator)
		{
			_matrixList = matrixList;
			_schedulePeriodTargetTimeCalculator = schedulePeriodTargetTimeCalculator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public KeyValuePair<string, bool> Run(IGroupPerson groupPerson, DateOnly dateOnly)
		{
				var steadyState = true;
				var firstPerson = groupPerson.GroupMembers[0];
				var teamSteadyState = CreateTeamSteadyState(firstPerson, dateOnly);
				if (teamSteadyState == null)
					steadyState = false;

				foreach (var person in groupPerson.GroupMembers)
				{
					if (steadyState == false) break;
					if (!person.Equals(firstPerson))
					{
						var virtualSchedulePeriod = person.VirtualSchedulePeriod(dateOnly);
						var scheduleMatrixPro = GetScheduleMatrixPro(virtualSchedulePeriod);
						if (scheduleMatrixPro == null)
						{
							steadyState = false;
							break;	
						}

						var personPeriod = person.PersonPeriods(new DateOnlyPeriod(dateOnly, dateOnly)).SingleOrDefault();

						if (personPeriod == null)
						{
							steadyState = false;
							break;	
						}

						if (!teamSteadyState.SteadyState(virtualSchedulePeriod, scheduleMatrixPro, personPeriod))
						{
							steadyState = false;
							break;
						}
					}
				}

			return new KeyValuePair<string, bool>(groupPerson.Name.FirstName, steadyState);
		}

		private ITeamSteadyState CreateTeamSteadyState(IPerson firstPerson, DateOnly dateOnly)
		{
			var firstPersonvirtualSchedulePeriod = firstPerson.VirtualSchedulePeriod(dateOnly);
			var firstPersonScheduleMatrixPro = GetScheduleMatrixPro(firstPersonvirtualSchedulePeriod);
			if (firstPersonScheduleMatrixPro == null) return null;
			var firstPersonpersonPeriod = firstPerson.PersonPeriods(new DateOnlyPeriod(dateOnly, dateOnly)).SingleOrDefault();
			if (firstPersonpersonPeriod == null) return null;
			var firstPersonteamSteadyStateSchedulePeriod = new TeamSteadyStateSchedulePeriod(firstPersonvirtualSchedulePeriod, _schedulePeriodTargetTimeCalculator, firstPersonScheduleMatrixPro);
			var firstPersonteamSteadyStatePersonPeriod = new TeamSteadyStatePersonPeriod(firstPersonpersonPeriod);
			return new TeamSteadyState(firstPersonteamSteadyStatePersonPeriod, firstPersonteamSteadyStateSchedulePeriod);
		}

		private IScheduleMatrixPro GetScheduleMatrixPro(IVirtualSchedulePeriod virtualSchedulePeriod)
		{
			foreach (var scheduleMatrixPro in _matrixList)
			{
				if (scheduleMatrixPro.Person.Equals(virtualSchedulePeriod.Person) && scheduleMatrixPro.SchedulePeriod.Equals(virtualSchedulePeriod)) return scheduleMatrixPro;
			}

			return null;
		}
	}
}
