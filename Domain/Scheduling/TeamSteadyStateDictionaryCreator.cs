using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class TeamSteadyStateDictionaryCreator
	{
		private readonly IList<IGroupPerson> _groupPersons;
		private readonly ISchedulePeriodTargetTimeCalculator _schedulePeriodTargetTimeCalculator;
		private readonly IList<IScheduleMatrixPro> _matrixList;

		private delegate KeyValuePair<string, bool> TeamSteadyStateDelegate(IGroupPerson groupPerson, DateOnly dateOnly);

		public TeamSteadyStateDictionaryCreator(IList<IGroupPerson> groupPersons, ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator, IList<IScheduleMatrixPro> matrixList)
		{
			_groupPersons = groupPersons;
			_schedulePeriodTargetTimeCalculator = schedulePeriodTargetTimeCalculator;
			_matrixList = matrixList;
		}

		public IDictionary<string, bool> Create(DateOnly dateOnly)
		{
			var runnableList = new Dictionary<TeamSteadyStateDelegate, IAsyncResult>();

			foreach (var groupPerson in _groupPersons)
			{
				var runner = new TeamSteadyStateRunner(_matrixList, _schedulePeriodTargetTimeCalculator);
				TeamSteadyStateDelegate toRun = runner.Run;
				var result = toRun.BeginInvoke(groupPerson, dateOnly, null, null);
				runnableList.Add(toRun,result);
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

			return results.ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value);
		}
	}
}
