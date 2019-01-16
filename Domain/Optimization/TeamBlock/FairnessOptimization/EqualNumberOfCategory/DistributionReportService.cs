using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

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

		public DistributionReportService(IDistributionForPersons distributionForPersons, 
										 IGroupCreator groupCreator)
		{
			_distributionForPersons = distributionForPersons;
			_groupCreator = groupCreator;
		}

		public DistributionReportData CreateReport(IPerson person, IGroupPage groupPageForDate, IList<IPerson> allFilteredPersons, IScheduleDictionary scheduleDictionary)
		{
			var report = new DistributionReportData();
			var totalDistribution =
				_distributionForPersons.CreateSummary(
					filterOnEqualNumberOfCategorySetting(allFilteredPersons), scheduleDictionary);
			var personDistribution = _distributionForPersons.CreateSummary(new List<IPerson> {person}, scheduleDictionary);
			var myTeam = _groupCreator.CreateGroupForPerson(person, groupPageForDate, scheduleDictionary.Keys.ToHashSet());
			var teamDistribution =
				_distributionForPersons.CreateSummary(filterOnEqualNumberOfCategorySetting(myTeam.GroupMembers), scheduleDictionary);

			var involvedCategories = new SortedSet<IShiftCategory>(totalDistribution.PercentDicionary.Keys,
				new ShiftCategorySorter());
			foreach (var shiftCategory in involvedCategories)
			{
				var values = new DistributionReportDataValues();
				values.All = totalDistribution.PercentDicionary[shiftCategory];

				double percentage;
				if (personDistribution.PercentDicionary.TryGetValue(shiftCategory, out percentage))
					values.Agent = percentage;

				if (teamDistribution.PercentDicionary.TryGetValue(shiftCategory, out percentage))
					values.Team = percentage;

				report.DistributionDictionary.Add(shiftCategory, values);
			}

			return report;
		}

		private IEnumerable<IPerson> filterOnEqualNumberOfCategorySetting(IEnumerable<IPerson> personList)
		{
			return personList.Where(p => p.WorkflowControlSet!=null).GroupBy(p => p.WorkflowControlSet)
				.Where(k => k.Key?.GetFairnessType() == FairnessType.EqualNumberOfShiftCategory).SelectMany(p => p).ToArray();
		}
	}
}