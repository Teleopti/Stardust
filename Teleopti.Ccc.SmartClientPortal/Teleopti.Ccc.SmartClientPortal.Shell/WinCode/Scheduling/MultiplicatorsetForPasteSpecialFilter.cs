using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	/// <summary>
	/// Remove items from the give IMultiplicatorDefinitionSet workingSets which are not in ALL scheduleDay
	/// </summary>
	public class MultiplicatorsetForPasteSpecialFilter
	{

		public IEnumerable<IMultiplicatorDefinitionSet> FilterAvailableMultiplicatorSet(IEnumerable<IScheduleDay> scheduleDays)
		{
			var contractsInScheduleDays = extractContracts(scheduleDays);
			var multiplicatorDefinitionSetsInScheduleDays = multiplicatorDefinitionSetsInContracts(contractsInScheduleDays);
			return multiplicatorDefinitionSetsInScheduleDays;
		}

		private IEnumerable<IMultiplicatorDefinitionSet> multiplicatorDefinitionSetsInContracts(IEnumerable<IContract> contracts)
		{
			//  of all person period's multiplicator definition set within a day

			IEnumerable<IMultiplicatorDefinitionSet> result = new List<IMultiplicatorDefinitionSet>();

			var contractList = contracts.ToList();

			if (!contractList.Any())
				return result;

			result = contractList[0].MultiplicatorDefinitionSetCollection.Where(m => m.MultiplicatorType == MultiplicatorType.Overtime && m.IsDeleted == false);

			for (int i = 1; i < contractList.Count; i++)
			{
				var current = contractList[i];
				var multiplicatorDefinitionSetCollection =
					current.MultiplicatorDefinitionSetCollection.Where(m => m.MultiplicatorType == MultiplicatorType.Overtime && m.IsDeleted == false)
			;
				result = result.Intersect(multiplicatorDefinitionSetCollection);
				if (!result.Any())
					break;
			}
			return result;
		}

		private IEnumerable<IContract> extractContracts(IEnumerable<IScheduleDay> scheduleDays)
		{
			// union of contract in all schedule days

			var result = new HashSet<IContract>();
			foreach (var scheduleDay in scheduleDays)
			{
				var person = scheduleDay.Person;
				var personPeriod = person.Period(scheduleDay.DateOnlyAsPeriod.DateOnly);
				if (personPeriod == null)
					continue;
				var personContract = personPeriod.PersonContract;
				var contract = personContract.Contract;
				result.Add(contract);
			}
			return result;
		}
	}
}
