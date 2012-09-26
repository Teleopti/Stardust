using System;
using System.Collections.Generic;
using System.Diagnostics;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ITeamSteadyStateDictionaryCreator
	{
		IDictionary<string, bool> Create(DateOnlyPeriod dateOnlyPeriod);
	}

	public class TeamSteadyStateDictionaryCreator : ITeamSteadyStateDictionaryCreator
	{
		//private readonly IList<IGroupPerson> _groupPersons;
		private readonly ISchedulePeriodTargetTimeCalculator _schedulePeriodTargetTimeCalculator;
		private readonly IList<IScheduleMatrixPro> _matrixList;
		private readonly IGroupPersonsBuilder _groupPersonsBuilder;
		private readonly IList<IPerson> _persons;
		private readonly ISchedulingOptions _schedulingOptions;

		private delegate KeyValuePair<string, bool> TeamSteadyStateDelegate(IGroupPerson groupPerson, DateOnly dateOnly);

		//public TeamSteadyStateDictionaryCreator(IList<IGroupPerson> groupPersons, ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator, IList<IScheduleMatrixPro> matrixList)
		//{
		//    _groupPersons = groupPersons;
		//    _schedulePeriodTargetTimeCalculator = schedulePeriodTargetTimeCalculator;
		//    _matrixList = matrixList;
		//}

		public TeamSteadyStateDictionaryCreator(IList<IPerson> persons, ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator, IList<IScheduleMatrixPro> matrixList, IGroupPersonsBuilder groupPersonsBuilder, ISchedulingOptions schedulingOptions)
		{
			_persons = persons;
			_schedulePeriodTargetTimeCalculator = schedulePeriodTargetTimeCalculator;
			_matrixList = matrixList;
			_groupPersonsBuilder = groupPersonsBuilder;
			_schedulingOptions = schedulingOptions;
		}

		public IDictionary<string, bool> Create(DateOnlyPeriod dateOnlyPeriod)	
		{
			var dictionary = new Dictionary<string, bool>();
			
			foreach (var dateOnly in dateOnlyPeriod.DayCollection())
			{
				var runnableList = new Dictionary<TeamSteadyStateDelegate, IAsyncResult>();
				var groupPersons = _groupPersonsBuilder.BuildListOfGroupPersons(dateOnly, _persons, true, _schedulingOptions);

				foreach (var groupPerson in groupPersons)
				{
					var runner = new TeamSteadyStateRunner(_matrixList, _schedulePeriodTargetTimeCalculator);
					TeamSteadyStateDelegate toRun = runner.Run;
					var result = toRun.BeginInvoke(groupPerson, dateOnly, null, null);
					runnableList.Add(toRun, result);
				}

				//Sync all threads
				IList<KeyValuePair<string, bool>> results = new List<KeyValuePair<string, bool>>();
				try
				{
					foreach (var thread in runnableList)
					{
						results.Add(thread.Key.EndInvoke(thread.Value));
					}
				}
				catch (Exception e)
				{
					Trace.WriteLine(e.Message);
					throw;
				}

				foreach (var keyValuePair in results)
				{
					if(dictionary.ContainsKey(keyValuePair.Key))
					{
						if(keyValuePair.Value == false && dictionary[keyValuePair.Key])
						{
							dictionary.Remove(keyValuePair.Key);
							dictionary.Add(keyValuePair.Key, keyValuePair.Value);
						}
					}

					else dictionary.Add(keyValuePair.Key, keyValuePair.Value);
				}
			}

			return dictionary;
		}

		//public IDictionary<string, bool> Create(DateOnly dateOnly)
		//{
		//    var dictionary = new Dictionary<string, bool>();
		//    var runnableList = new Dictionary<TeamSteadyStateDelegate, IAsyncResult>();

		//    foreach (var groupPerson in _groupPersons)
		//    {
		//        var runner = new TeamSteadyStateRunner(_matrixList, _schedulePeriodTargetTimeCalculator);
		//        TeamSteadyStateDelegate toRun = runner.Run;
		//        var result = toRun.BeginInvoke(groupPerson, dateOnly, null, null);
		//        runnableList.Add(toRun,result);
		//    }

		//    //Sync all threads
		//    IList<KeyValuePair<string, bool>> results = new List<KeyValuePair<string, bool>>();
		//    try
		//    {
		//        foreach (var thread in runnableList)
		//        {
		//            results.Add(thread.Key.EndInvoke(thread.Value));
		//        }
		//    }
		//    catch (Exception e)
		//    {
		//        Trace.WriteLine(e.Message);
		//        throw;
		//    }

		//    foreach (var keyValuePair in results)
		//    {
		//        dictionary.Add(keyValuePair.Key, keyValuePair.Value);
		//    }

		//    return dictionary;
		//}
	}
}
