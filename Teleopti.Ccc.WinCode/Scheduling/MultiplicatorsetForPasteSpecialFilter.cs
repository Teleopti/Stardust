using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IMultiplicatorsetForPasteSpecialFilter
	{
		IEnumerable<IMultiplicatorDefinitionSet> FilterAvailableMultiplicatorSet(IEnumerable<IMultiplicatorDefinitionSet> multiplicatorDefinitionSets, IEnumerable<IScheduleDay> scheduleDays);
	}

	public class MultiplicatorsetForPasteSpecialFilter : IMultiplicatorsetForPasteSpecialFilter
	{

		public IEnumerable<IMultiplicatorDefinitionSet> FilterAvailableMultiplicatorSet(IEnumerable<IMultiplicatorDefinitionSet> multiplicatorDefinitionSets, IEnumerable<IScheduleDay> scheduleDays)
		{

			var workingSets = from set in multiplicatorDefinitionSets
							  where set.MultiplicatorType == MultiplicatorType.Overtime
							  select set;
						


			foreach (var scheduleDay in scheduleDays)
			{
				var multiplicatorForScheduleDay = multiplicatorDefinitionSetsForScheduleDay(scheduleDay);
				// remove items from workingSets which are not in ALL multiplicatorForScheduleDay
				var except = workingSets.Except(multiplicatorForScheduleDay);
				workingSets = workingSets.Except(except);
				if (!workingSets.Any())
					return workingSets;
			}
			return workingSets;
		}

		private IEnumerable<IMultiplicatorDefinitionSet> multiplicatorDefinitionSetsForScheduleDay(IScheduleDay scheduleDay)
		{
			// union of all person period's multiplicator definition set within a day

			var result = new List<IMultiplicatorDefinitionSet>();
			var person = scheduleDay.Person;
			var personPeriods = person.PersonPeriods(new DateOnlyPeriod(scheduleDay.DateOnlyAsPeriod.DateOnly,
					scheduleDay.DateOnlyAsPeriod.DateOnly));
			if (!personPeriods.Any())
				return result;
			foreach (var personPeriod in personPeriods)
			{
				var personContract = personPeriod.PersonContract;
				if (personContract == null)
					continue;
				var contract = personContract.Contract;
				if (contract == null)
					continue;
				var multiplicatorDefinitionSetCollection = contract.MultiplicatorDefinitionSetCollection;
				if (multiplicatorDefinitionSetCollection == null
				    || !multiplicatorDefinitionSetCollection.Any())
					continue;
				result.AddRange(multiplicatorDefinitionSetCollection);
			}

			return result;
		}

	}
}
