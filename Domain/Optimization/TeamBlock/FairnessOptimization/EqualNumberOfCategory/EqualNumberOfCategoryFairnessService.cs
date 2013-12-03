using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IEqualNumberOfCategoryFairnessService
	{
		void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
		                             IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions, 
		                             IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService);
	}

	public class EqualNumberOfCategoryFairnessService : IEqualNumberOfCategoryFairnessService
	{
		private readonly IConstructTeamBlock _constructTeamBlock;
		private readonly IDistributionForPersons _distributionForPersons;
		private readonly IFilterForEqualNumberOfCategoryFairness _filterForEqualNumberOfCategoryFairness;
		private readonly IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;

		public EqualNumberOfCategoryFairnessService(IConstructTeamBlock constructTeamBlock, IDistributionForPersons distributionForPersons, IFilterForEqualNumberOfCategoryFairness filterForEqualNumberOfCategoryFairness, IFilterForTeamBlockInSelection filterForTeamBlockInSelection)
		{
			_constructTeamBlock = constructTeamBlock;
			_distributionForPersons = distributionForPersons;
			_filterForEqualNumberOfCategoryFairness = filterForEqualNumberOfCategoryFairness;
			_filterForTeamBlockInSelection = filterForTeamBlockInSelection;
		}

		public void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
		                    IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions, 
							IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService)
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
			
			var totalDistribution = _distributionForPersons.CreateSummary(personListForTotalDistribution, scheduleDictionary);
			var teamBlockInfoList = _filterForTeamBlockInSelection.Filter(teamBlockListWithCorrectWorkFlowControlSet,
			                                                              selectedPersons, selectedPeriod);

			while (teamBlockInfoList.Count > 0)
			{

				//to standalone class
				ITeamBlockInfo teamBlockInfoToWorkWith = null;
				var teamBlockInfoDistributionValue = 0d;
				foreach (var teamBlockInfo in teamBlockInfoList)
				{
					var distribution = _distributionForPersons.CreateSummary(teamBlockInfo.TeamInfo.GroupPerson.GroupMembers,
					                                                         scheduleDictionary);
					var absDiff = distributionDiff(totalDistribution, distribution);
					if (absDiff > teamBlockInfoDistributionValue)
					{
						teamBlockInfoDistributionValue = absDiff;
						teamBlockInfoToWorkWith = teamBlockInfo;
					}
				}

				//to standalone class
				var possibleTeamBlocksToSwapWith = new List<ITeamBlockInfo>();
				foreach (var teamBlockInfo in teamBlockInfoList)
				{
					if (!new TeamBlockPeriodValidator().ValidatePeriod(teamBlockInfo, teamBlockInfoToWorkWith))
						continue;

					if (!new TeamMemberCountValidator().ValidateMemberCount(teamBlockInfo, teamBlockInfoToWorkWith))
						continue;

					if (!new TeamBlockContractTimeValidator().ValidateContractTime(teamBlockInfo, teamBlockInfoToWorkWith))
						continue;

					//Kolla att skillen stämmer
					//inga lås i blocken

					if (teamBlockInfo.Equals(teamBlockInfoToWorkWith))
						continue;

					possibleTeamBlocksToSwapWith.Add(teamBlockInfo);
				}


				//to standalone class
				ISwapServiceNew swapService = new SwapServiceNew(); //problem med frånvaro
				List<IScheduleDay> totalModifyList = new List<IScheduleDay>();
				for (int i = 0; i < teamBlockInfoToWorkWith.TeamInfo.GroupPerson.GroupMembers.Count(); i++)
				{
					foreach (var dateOnly in teamBlockInfoToWorkWith.BlockInfo.BlockPeriod.DayCollection())
					{
						var person1 = teamBlockInfoToWorkWith.TeamInfo.GroupPerson.GroupMembers.ToList()[i];
						var person2 = possibleTeamBlocksToSwapWith[0].TeamInfo.GroupPerson.GroupMembers.ToList()[i];
						var day1 = scheduleDictionary[person1].ScheduledDay(dateOnly);
						var day2 = scheduleDictionary[person2].ScheduledDay(dateOnly);
						totalModifyList.AddRange(swapService.Swap(new List<IScheduleDay> {day1, day2}, scheduleDictionary));
					}
				}

				rollbackService.ClearModificationCollection();
				rollbackService.ModifyParts(totalModifyList);
				rollbackService.ClearModificationCollection();

				//loop
				//get swap candidate
				//get swap teamblock from candidate
				//find swappable pair
				//swap

				teamBlockInfoList.Remove(teamBlockInfoToWorkWith);
			}
		}

		private double distributionDiff(IDistributionSummary totalDistribution,
									 IDistributionSummary distributionToCalculate)
		{
			var absDiff = 0d;
			foreach (var i in distributionToCalculate.PercentDicionary)
			{
				absDiff += Math.Abs(i.Value - totalDistribution.PercentDicionary[i.Key]);
			}

			return absDiff;
		}

	}
}