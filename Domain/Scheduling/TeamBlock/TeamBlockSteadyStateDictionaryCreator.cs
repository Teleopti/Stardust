using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockSteadyStateDictionaryCreator
	{
		IDictionary<Guid, bool> Create(DateOnlyPeriod dateOnlyPeriod);
	}

	public class TeamBlockSteadyStateDictionaryCreator : ITeamBlockSteadyStateDictionaryCreator
	{
		private readonly IList<IScheduleMatrixPro> _matrixList;
		private readonly IGroupPersonsBuilder _groupPersonsBuilder;
		private readonly ISchedulingOptions _schedulingOptions;
		private readonly ITeamSteadyStateRunner _teamSteadyStateRunner;

		public TeamBlockSteadyStateDictionaryCreator(ITeamSteadyStateRunner teamSteadyStateRunner, IList<IScheduleMatrixPro> matrixList, IGroupPersonsBuilder groupPersonsBuilder, ISchedulingOptions schedulingOptions)
		{
			_matrixList = matrixList;
			_groupPersonsBuilder = groupPersonsBuilder;
			_schedulingOptions = schedulingOptions;
			_teamSteadyStateRunner = teamSteadyStateRunner;
		}

		public IDictionary<Guid, bool> Create(DateOnlyPeriod dateOnlyPeriod)
		{
		    var dictionary = new Dictionary<Guid, bool>();

		    if (_matrixList.Count == 0) return dictionary;
			var groupPersonDic = new Dictionary<IGroupPerson, IList<DateOnly>>();

			IList<IPerson> persons = new List<IPerson>();

			foreach (var scheduleMatrixPro in _matrixList)
			{
				if(!persons.Contains(scheduleMatrixPro.Person))
					persons.Add(scheduleMatrixPro.Person);
			}

			foreach (var scheduleMatrixPro in _matrixList)
			{
				var effectivePeriodDays = scheduleMatrixPro.EffectivePeriodDays;
				var firstEffectiveDay = effectivePeriodDays[0].Day;
				var lastEffectiveDay = effectivePeriodDays[effectivePeriodDays.Count - 1].Day;
				var effectivePeriod = new DateOnlyPeriod(firstEffectiveDay, lastEffectiveDay);
				var day = DateOnly.MinValue;

				//check if matrix intersect with selection
				if (effectivePeriod.Contains(dateOnlyPeriod.StartDate)) day = dateOnlyPeriod.StartDate;
				else if (effectivePeriod.Contains(dateOnlyPeriod.EndDate)) day = dateOnlyPeriod.EndDate;

				//continue if we dont have an intersection
				if(day == DateOnly.MinValue) continue;

				//add groupPerson + firstday on matrix to dictionary
				var groupPersons = _groupPersonsBuilder.BuildListOfGroupPersons(day, new List<IPerson> {scheduleMatrixPro.Person},
				                                                                true, _schedulingOptions);
				foreach (var groupPerson in groupPersons)
				{
					
					if (!groupPersonDic.ContainsKey(groupPerson))
						groupPersonDic.Add(groupPerson, new List<DateOnly>{firstEffectiveDay});
					else
					{
						if(!groupPersonDic[groupPerson].Contains(firstEffectiveDay))
							groupPersonDic[groupPerson].Add(firstEffectiveDay);
					}
				}
			}

			//check steady state for each date on groupPersons. Set state to false if any of the periods are false.
			foreach (var kvp in groupPersonDic)
			{
				var groupPerson = kvp.Key;
				var dates = kvp.Value;
				if (groupPerson.Id.HasValue && !dictionary.ContainsKey(groupPerson.Id.Value) && dates.Count > 0)
				{
					var res = new KeyValuePair<Guid, bool>();

					foreach (var dateOnly in dates)
					{
						res = _teamSteadyStateRunner.Run(groupPerson, dateOnly);
						if (!res.Value) break;
					}

					dictionary.Add(res.Key, res.Value);		
				}
			}

		    return dictionary;
		}
	}
}
