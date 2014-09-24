using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IDistributionReportService
	{
		DistributionReportData CreateReport(IPerson person, IGroupPage groupPageForDate, IList<IPerson> allFilteredPersons, IScheduleDictionary scheduleDictionary);
	}

	public class DistributionReportService : IDistributionReportService
	{
		private readonly IDistributionForPersons _distributionForPersons;
		private readonly IGroupCreator _groupCreator;
		private readonly bool _schedulerHidePointsFairnessSystem28317;
		private readonly bool _schedulerSeniority11111;

		public DistributionReportService(IDistributionForPersons distributionForPersons, 
										 IGroupCreator groupCreator, 
										 bool scheduler_HidePointsFairnessSystem_28317, 
										 bool scheduler_Seniority_11111)
		{
			_distributionForPersons = distributionForPersons;
			_groupCreator = groupCreator;
			_schedulerHidePointsFairnessSystem28317 = scheduler_HidePointsFairnessSystem_28317;
			_schedulerSeniority11111 = scheduler_Seniority_11111;
		}

		public DistributionReportData CreateReport(IPerson person, IGroupPage groupPageForDate, IList<IPerson> allFilteredPersons, IScheduleDictionary scheduleDictionary)
		{
			var report = new DistributionReportData();
			var totalDistribution = _distributionForPersons.CreateSummary(filterOnEqualNumberOfCategorySetting(allFilteredPersons), scheduleDictionary);
			var personDistribution = _distributionForPersons.CreateSummary(new List<IPerson> {person}, scheduleDictionary);
			var myTeam = _groupCreator.CreateGroupForPerson(person, groupPageForDate, scheduleDictionary.Keys.ToList());
			var teamDistribution = _distributionForPersons.CreateSummary(filterOnEqualNumberOfCategorySetting(myTeam.GroupMembers), scheduleDictionary);

			var involvedCategories = new SortedSet<IShiftCategory>(totalDistribution.PercentDicionary.Keys,
			                                                       new ShiftCategorySorter());
			foreach (var shiftCategory in involvedCategories)
			{
				var values = new DistributionReportDataValues();
				values.All = totalDistribution.PercentDicionary[shiftCategory];

				if (personDistribution.PercentDicionary.ContainsKey(shiftCategory))
					values.Agent = personDistribution.PercentDicionary[shiftCategory];

				if (teamDistribution.PercentDicionary.ContainsKey(shiftCategory))
					values.Team = teamDistribution.PercentDicionary[shiftCategory];

				report.DistributionDictionary.Add(shiftCategory, values);
			}
	
			return report;
		}

		private IEnumerable<IPerson> filterOnEqualNumberOfCategorySetting(IEnumerable<IPerson> personList)
		{
			IList<IPerson> filteredList = new List<IPerson>();
			foreach (var person in personList)
			{
				var wfcs = person.WorkflowControlSet;
				if (wfcs == null) continue;
				if (wfcs.GetFairnessType(_schedulerHidePointsFairnessSystem28317, _schedulerSeniority11111) == FairnessType.EqualNumberOfShiftCategory)
					filteredList.Add(person);
			}

			return filteredList;
		}
	}
}