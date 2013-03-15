using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface ITeamBlockIntradayOptimizationService
	{
		void Optimize(IList<IScheduleMatrixPro> allPersonMatrixList,
					  DateOnlyPeriod selectedPeriod,
					  IList<IPerson> selectedPersons,
					  IOptimizationPreferences optimizationPreferences,
					  ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService);
	}

	public class TeamBlockIntradayOptimizationService : ITeamBlockIntradayOptimizationService
	{
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ILockableBitArrayFactory _lockableBitArrayFactory;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly ISchedulingResultStateHolder _stateHolder;
		private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;

		public TeamBlockIntradayOptimizationService(ITeamInfoFactory teamInfoFactory,
													ITeamBlockInfoFactory teamBlockInfoFactory, ITeamBlockScheduler teamBlockScheduler,
													ILockableBitArrayFactory lockableBitArrayFactory,
													ISchedulingOptionsCreator schedulingOptionsCreator,
													ISchedulingResultStateHolder stateHolder,
													IDeleteAndResourceCalculateService deleteAndResourceCalculateService)
		{
			_teamInfoFactory = teamInfoFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamBlockScheduler = teamBlockScheduler;
			_lockableBitArrayFactory = lockableBitArrayFactory;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_stateHolder = stateHolder;
			_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
		}

		public void Optimize(IList<IScheduleMatrixPro> allPersonMatrixList,
							 DateOnlyPeriod selectedPeriod,
							 IList<IPerson> selectedPersons,
							 IOptimizationPreferences optimizationPreferences,
							 ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
		{
			ISchedulingOptions schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);

			var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			foreach (var selectedPerson in selectedPersons)
			{
				allTeamInfoListOnStartDate.Add(_teamInfoFactory.CreateTeamInfo(selectedPerson, selectedPeriod,
																			   allPersonMatrixList));
			}
			var allTeamBlocksInHashSet = new HashSet<ITeamBlockInfo>();
			foreach (var teamInfo in allTeamInfoListOnStartDate)
			{
				foreach (var day in selectedPeriod.DayCollection())
				{
					var teamBlock = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, day,
																			  schedulingOptions.BlockFinderTypeForAdvanceScheduling);
					allTeamBlocksInHashSet.Add(teamBlock);
				}
			}
			var allTeamBlocks = new List<ITeamBlockInfo>();
			allTeamBlocks.AddRange(allTeamBlocksInHashSet);

			var standardDeviationData = new StandardDeviationData();
			foreach (var matrixPro in allPersonMatrixList)
			{
				var scheduleResultDataExtractor = new RelativeDailyStandardDeviationsByAllSkillsExtractor(matrixPro, schedulingOptions);
				var values = scheduleResultDataExtractor.Values();
				var periodDays = matrixPro.EffectivePeriodDays;
				for (var i = 0; i < periodDays.Count; i++)
				{
					ILockableBitArray originalArray =
						_lockableBitArrayFactory.ConvertFromMatrix(optimizationPreferences.DaysOff.ConsiderWeekBefore,
																   optimizationPreferences.DaysOff.ConsiderWeekAfter,
																   matrixPro);
					if (originalArray.UnlockedIndexes.Contains(i) && !originalArray.DaysOffBitArray[i])
						if (!standardDeviationData.Data.ContainsKey(periodDays[i].Day))
							standardDeviationData.Add(periodDays[i].Day, values[i]);
				}
			}
			foreach (var teamBlockInfo in allTeamBlocks)
			{
				var valuesOfOneBlock = new List<double?>();
				foreach (var blockDay in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
				{
					if (!standardDeviationData.Data.ContainsKey(blockDay)) continue;
					var value = standardDeviationData.Data[blockDay];
					valuesOfOneBlock.Add(value);
				}
				teamBlockInfo.BlockInfo.StandardDeviations = valuesOfOneBlock;
			}

			var sortedTeamBlocks = allTeamBlocks.OrderByDescending(x => x.BlockInfo.AverageStandardDeviation);

			foreach (var teamBlock in sortedTeamBlocks)
			{
				//clear block
				foreach (var dateOnly in teamBlock.BlockInfo.BlockPeriod.DayCollection())
				{
					IList<IScheduleDay> toRemove = new List<IScheduleDay>();
					foreach (var person in teamBlock.TeamInfo.GroupPerson.GroupMembers)
					{
						IScheduleDay scheduleDay = _stateHolder.Schedules[person].ScheduledDay(dateOnly);
						SchedulePartView significant = scheduleDay.SignificantPart();
						if (significant != SchedulePartView.FullDayAbsence && significant != SchedulePartView.DayOff &&
							significant != SchedulePartView.ContractDayOff)
							toRemove.Add(scheduleDay);
					}
					_deleteAndResourceCalculateService.DeleteWithResourceCalculation(toRemove,
																					 schedulePartModifyAndRollbackService,
																					 schedulingOptions
																						 .ConsiderShortBreaks);
				}

				var datePoint = teamBlock.BlockInfo.BlockPeriod.DayCollection().First();
				_teamBlockScheduler.ScheduleTeamBlock(teamBlock, datePoint, schedulingOptions);
			}
		}
	}
}
