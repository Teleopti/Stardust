using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private readonly ISchedulePeriodTargetTimeCalculator _schedulePeriodTargetTimeCalculator;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private readonly IList<IScheduleMatrixPro> _matrixList;
		private readonly IGroupPersonsBuilder _groupPersonsBuilder;
		private readonly IList<IPerson> _persons;
		private readonly ISchedulingOptions _schedulingOptions;

		private delegate KeyValuePair<Guid, bool> TeamSteadyStateDelegate(IGroupPerson groupPerson, DateOnly dateOnly);

		public TeamSteadyStateDictionaryCreator(IList<IPerson> persons, ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator, IList<IScheduleMatrixPro> matrixList, IGroupPersonsBuilder groupPersonsBuilder, ISchedulingOptions schedulingOptions)
		{
			_persons = persons;
			_schedulePeriodTargetTimeCalculator = schedulePeriodTargetTimeCalculator;
			_matrixList = matrixList;
			_groupPersonsBuilder = groupPersonsBuilder;
			_schedulingOptions = schedulingOptions;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "runnableList")]
		public IDictionary<Guid, bool> Create(DateOnlyPeriod dateOnlyPeriod)	
		{
			var dictionary = new Dictionary<Guid, bool>();
			IList<KeyValuePair<Guid, bool>> results = new List<KeyValuePair<Guid, bool>>();

			foreach (var dateOnly in dateOnlyPeriod.DayCollection())
			{
				var runnableList = new Dictionary<TeamSteadyStateDelegate, IAsyncResult>();
				var groupPersons = _groupPersonsBuilder.BuildListOfGroupPersons(dateOnly, _persons, true, _schedulingOptions);

				foreach (var groupPerson in groupPersons)
				{
					//var runner = new TeamSteadyStateRunner(_matrixList, _schedulePeriodTargetTimeCalculator);
					//TeamSteadyStateDelegate toRun = runner.Run;
					//var result = toRun.BeginInvoke(groupPerson, dateOnly, null, null);
					//runnableList.Add(toRun, result);

					results.Add(new KeyValuePair<Guid, bool>(groupPerson.Id.Value, false));
				}

				////Sync all threads
				//IList<KeyValuePair<Guid, bool>> results = new List<KeyValuePair<Guid, bool>>();
				//try
				//{
				//    foreach (var thread in runnableList)
				//    {
				//        results.Add(thread.Key.EndInvoke(thread.Value));
				//    }
				//}
				//catch (Exception e)
				//{
				//    Trace.WriteLine(e.Message);
				//    throw;
				//}

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
	}
}
