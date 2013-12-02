

using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IEqualNumberOfCategoryFairnessService
	{
	}

	public class EqualNumberOfCategoryFairnessService : IEqualNumberOfCategoryFairnessService
	{
		private readonly IConstructTeamBlock _constructTeamBlock;
		private readonly IDistributionForPersons _distributionForPersons;
		private readonly IFilterForEqualNumberOfCategoryFairness _filterForEqualNumberOfCategoryFairness;

		public EqualNumberOfCategoryFairnessService(IConstructTeamBlock constructTeamBlock, IDistributionForPersons distributionForPersons, IFilterForEqualNumberOfCategoryFairness filterForEqualNumberOfCategoryFairness)
		{
			_constructTeamBlock = constructTeamBlock;
			_distributionForPersons = distributionForPersons;
			_filterForEqualNumberOfCategoryFairness = filterForEqualNumberOfCategoryFairness;
		}

		public void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
		                    IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions, 
							IScheduleDictionary scheduleDictionary)
		{
			var teamBlockListRaw = _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod, selectedPersons,
			                                                  schedulingOptions);

			var teamBlockListWithCorrectWorkFlowControlSet = _filterForEqualNumberOfCategoryFairness.Filter(teamBlockListRaw);
			
			var personListForTotalDistribution = new HashSet<IPerson>();
			foreach (var teamBlockInfo in teamBlockListWithCorrectWorkFlowControlSet)
			{
				foreach (var groupMember in teamBlockInfo.TeamInfo.GroupPerson.GroupMembers)
				{
					personListForTotalDistribution.Add(groupMember);
				}
			}
			
			//get total distr
			var toalDistribution = _distributionForPersons.CreateSummary(personListForTotalDistribution, scheduleDictionary);
			//loop
			//get swap candidate
			//get swap teamblock from candidate
			//find swappable pair
			//swap
		}
	}
}