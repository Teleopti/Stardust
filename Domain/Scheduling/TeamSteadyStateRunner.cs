using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ITeamSteadyStateRunner
	{
		KeyValuePair<Guid, bool> Run(IGroupPerson groupPerson, DateOnly dateOnly);
	}

	public class TeamSteadyStateRunner : ITeamSteadyStateRunner
	{
		private readonly IList<IScheduleMatrixPro> _matrixList;
		private readonly ISchedulePeriodTargetTimeCalculator _schedulePeriodTargetTimeCalculator;
		private DateOnly _startDate = new DateOnly(DateTime.MinValue);
		private DateOnly _endDate = new DateOnly(DateTime.MinValue);

		public TeamSteadyStateRunner(IList<IScheduleMatrixPro> matrixList, ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator)
		{
			_matrixList = matrixList;
			_schedulePeriodTargetTimeCalculator = schedulePeriodTargetTimeCalculator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public KeyValuePair<Guid, bool> Run(IGroupPerson groupPerson, DateOnly dateOnly)	
		{
				if(!groupPerson.Id.HasValue) throw new ArgumentNullException("groupPerson");
				var steadyState = true;
				var firstPerson = groupPerson.GroupMembers.First();
				var firstPersonvirtualSchedulePeriod = firstPerson.VirtualSchedulePeriod(dateOnly);
				var firstPersonScheduleMatrixPro = GetScheduleMatrixPro(firstPersonvirtualSchedulePeriod);

				if (firstPersonScheduleMatrixPro == null)
					return new KeyValuePair<Guid, bool>(groupPerson.Id.Value, false);
			
				_startDate = firstPersonScheduleMatrixPro.EffectivePeriodDays[0].Day;
				_endDate = firstPersonScheduleMatrixPro.EffectivePeriodDays[firstPersonScheduleMatrixPro.EffectivePeriodDays.Count - 1].Day;
				var teamSteadyState = CreateTeamSteadyState(firstPerson, dateOnly, firstPersonScheduleMatrixPro, firstPersonvirtualSchedulePeriod);
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

						if(!MatrixIsSteady(scheduleMatrixPro))
						{
							steadyState = false;
							break;
						}

						var personPeriod = person.Period(dateOnly);

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

			return new KeyValuePair<Guid, bool>(groupPerson.Id.Value, steadyState);
		}

		private bool MatrixIsSteady(IScheduleMatrixPro matrix)
		{
			var matrixStart = matrix.EffectivePeriodDays[0].Day;
			var matrixEnd = matrix.EffectivePeriodDays[matrix.EffectivePeriodDays.Count - 1].Day;

			if (matrixStart != _startDate || matrixEnd != _endDate)
			{
				return false;
			}
		
			if (matrix.Person.Period(matrixStart) != matrix.Person.Period(matrixEnd))
			{
				return false;
			}

			return true;
		}

		private ITeamSteadyState CreateTeamSteadyState(IPerson firstPerson, DateOnly dateOnly, IScheduleMatrixPro firstPersonScheduleMatrixPro, IVirtualSchedulePeriod firstPersonvirtualSchedulePeriod)
		{
			if (firstPersonScheduleMatrixPro == null) return null;
			if (firstPersonScheduleMatrixPro.Person.Period(_startDate) != firstPersonScheduleMatrixPro.Person.Period(_endDate)) return null;
			var firstPersonpersonPeriod = firstPerson.Period(dateOnly);
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
