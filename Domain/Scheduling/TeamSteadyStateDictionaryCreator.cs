using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ITeamSteadyStateDictionaryCreator
	{
		IDictionary<Guid, bool> Create(DateOnlyPeriod dateOnlyPeriod);
	}

	public class TeamSteadyStateDictionaryCreator : ITeamSteadyStateDictionaryCreator
	{
		private readonly IList<IScheduleMatrixPro> _matrixList;
		private readonly IGroupPersonsBuilder _groupPersonsBuilder;
		private readonly ISchedulingOptions _schedulingOptions;
		private readonly ITeamSteadyStateRunner _teamSteadyStateRunner;

		public TeamSteadyStateDictionaryCreator(ITeamSteadyStateRunner teamSteadyStateRunner, IList<IScheduleMatrixPro> matrixList, IGroupPersonsBuilder groupPersonsBuilder, ISchedulingOptions schedulingOptions)
		{
			_matrixList = matrixList;
			_groupPersonsBuilder = groupPersonsBuilder;
			_schedulingOptions = schedulingOptions;
			_teamSteadyStateRunner = teamSteadyStateRunner;
		}

		public IDictionary<Guid, bool> Create(DateOnlyPeriod dateOnlyPeriod)
		{
		    var dictionary = new Dictionary<Guid, bool>();
			//return dictionary;

		    if (_matrixList.Count == 0) return dictionary;
			var groupPersonDic = new Dictionary<IGroupPerson, DateOnly>();

			IList<IPerson> persons = new List<IPerson>();

			foreach (var scheduleMatrixPro in _matrixList)
			{
				if(!persons.Contains(scheduleMatrixPro.Person))
					persons.Add(scheduleMatrixPro.Person);
			}

			foreach (var scheduleMatrixPro in _matrixList)
			{
				var day = scheduleMatrixPro.EffectivePeriodDays[0].Day;
				var groupPersons = _groupPersonsBuilder.BuildListOfGroupPersons(day, persons, true, _schedulingOptions);
				foreach (var groupPerson in groupPersons)
				{
					if (!groupPersonDic.ContainsKey(groupPerson))
						groupPersonDic.Add(groupPerson, day);
				}
			}

			foreach (var kvp in groupPersonDic)
			{
				var groupPerson = kvp.Key;
				var date = kvp.Value;
				if (groupPerson.Id.HasValue && !dictionary.ContainsKey(groupPerson.Id.Value))
				{
					var res = _teamSteadyStateRunner.Run(groupPerson, date);
					dictionary.Add(res.Key, res.Value);
				}
			}

		    return dictionary;
		}
	}
}
