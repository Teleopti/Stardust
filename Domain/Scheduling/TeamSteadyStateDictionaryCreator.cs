using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class TeamSteadyStateDictionaryCreator
	{
		private readonly IList<IGroupPerson> _groupPersons;
		private readonly ISchedulePeriodTargetTimeCalculator _schedulePeriodTargetTimeCalculator;
		private readonly IList<IScheduleMatrixPro> _matrixList;

		public TeamSteadyStateDictionaryCreator(IList<IGroupPerson> groupPersons, ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator, IList<IScheduleMatrixPro> matrixList)
		{
			_groupPersons = groupPersons;
			_schedulePeriodTargetTimeCalculator = schedulePeriodTargetTimeCalculator;
			_matrixList = matrixList;
		}

		public IDictionary<string, bool> Create(DateOnly dateOnly)
		{
			var dictionary = new Dictionary<string, bool>();

			foreach (var groupPerson in _groupPersons)
			{
				var firstPerson = groupPerson.GroupMembers[0];
				if (groupPerson.GroupMembers.Count == 0) continue;
				var teamSteadyState = CreateTeamSteadyState(firstPerson, dateOnly);
				if (teamSteadyState == null) continue;
				var steadyState = true;

				foreach (var person in groupPerson.GroupMembers)
				{
					if(!person.Equals(firstPerson))
					{
						var virtualSchedulePeriod = person.VirtualSchedulePeriod(dateOnly);
						var scheduleMatrixPro = GetScheduleMatrixPro(virtualSchedulePeriod);
						if (scheduleMatrixPro == null) continue;
						var personPeriod = person.PersonPeriods(new DateOnlyPeriod(dateOnly, dateOnly)).SingleOrDefault();
						if (personPeriod == null) continue;
						
						if(!teamSteadyState.SteadyState(virtualSchedulePeriod, scheduleMatrixPro, personPeriod))
						{
							steadyState = false;
							break;
						}
					}
				}

				dictionary.Add(groupPerson.Name.FirstName, steadyState);
			}

			return dictionary;
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
