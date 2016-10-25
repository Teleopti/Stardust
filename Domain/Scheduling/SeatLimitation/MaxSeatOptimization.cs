using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class MaxSeatOptimization
	{
		private readonly MaxSeatSkillDataFactory _maxSeatSkillDataFactory;
		private readonly ResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly IMatrixUserLockLocker _matrixUserLockLocker;
		private readonly IMatrixNotPermittedLocker _matrixNotPermittedLocker;
		private readonly ITeamBlockGenerator _teamBlockGenerator;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly WorkShiftSelectorForMaxSeat _workShiftSelectorForMaxSeat;
		private readonly IGroupPersonBuilderForOptimizationFactory _groupPersonBuilderForOptimizationFactory;

		public MaxSeatOptimization(MaxSeatSkillDataFactory maxSeatSkillDataFactory,
														CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
														IScheduleDayChangeCallback scheduleDayChangeCallback,
														ISchedulingOptionsCreator schedulingOptionsCreator,
														ITeamBlockScheduler teamBlockScheduler,
														IMatrixUserLockLocker matrixUserLockLocker,
														IMatrixNotPermittedLocker matrixNotPermittedLocker,
														ITeamBlockGenerator teamBlockGenerator,
														ITeamBlockClearer teamBlockClearer,
														WorkShiftSelectorForMaxSeat workShiftSelectorForMaxSeat,
														IGroupPersonBuilderForOptimizationFactory groupPersonBuilderForOptimizationFactory)
		{
			_maxSeatSkillDataFactory = maxSeatSkillDataFactory;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_teamBlockScheduler = teamBlockScheduler;
			_matrixUserLockLocker = matrixUserLockLocker;
			_matrixNotPermittedLocker = matrixNotPermittedLocker;
			_teamBlockGenerator = teamBlockGenerator;
			_teamBlockClearer = teamBlockClearer;
			_workShiftSelectorForMaxSeat = workShiftSelectorForMaxSeat;
			_groupPersonBuilderForOptimizationFactory = groupPersonBuilderForOptimizationFactory;
		}

		//TODO: ska vi verkligen skicka in optimizationpreferences? Vi ska väl bara stödja team + GroupPageType.Hierarchy i alla fall?
		public void Optimize(DateOnlyPeriod period, IEnumerable<IPerson> agentsToOptimize, IScheduleDictionary schedules, IScenario scenario, IOptimizationPreferences optimizationPreferences)
		{
			var allAgents = schedules.Select(schedule => schedule.Key);
			var maxSeatData = _maxSeatSkillDataFactory.Create(period, agentsToOptimize, scenario, allAgents);

			var tagSetter = new ScheduleTagSetter(new NullScheduleTag()); //fix - the tag
			var rollbackService = new SchedulePartModifyAndRollbackService(null, _scheduleDayChangeCallback, tagSetter);

			var allMatrixes = createMatrixes(schedules,
				schedules.Period.LoadedPeriod().ToDateOnlyPeriod(TimeZoneInfo.Utc) //FIX
				, period, allAgents);
			var businessRules = NewBusinessRuleCollection.Minimum(); //is this enough?

			using (_resourceCalculationContextFactory.Create(schedules, maxSeatData.AllMaxSeatSkills()))
			{
				//most stuff taken from TeamBlockIntradayOptimizationService
				var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
				_groupPersonBuilderForOptimizationFactory.Create(schedules, optimizationPreferences.Extra.TeamGroupPage);
				var teamBlocks = _teamBlockGenerator.Generate(allAgents, allMatrixes, period, agentsToOptimize, schedulingOptions);
				var remainingInfoList = teamBlocks.ToList();

				while (remainingInfoList.Count > 0)
				{
					foreach (var teamBlockInfo in remainingInfoList.ToList())
					{
						var firstSelectedDay = period.DayCollection().First();
						var datePoint = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection().FirstOrDefault(x => x >= firstSelectedDay);
						_teamBlockClearer.ClearTeamBlockWithNoResourceCalculation(rollbackService, teamBlockInfo, businessRules); //TODO: check if this is enough
						_teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelectorForMaxSeat, teamBlockInfo, datePoint, schedulingOptions,
							rollbackService,
							new DoNothingResourceCalculateDelayer(), maxSeatData.AllMaxSeatSkillDaysPerSkill().ToSkillDayEnumerable(), schedules, new ShiftNudgeDirective(), businessRules);

						remainingInfoList.Remove(teamBlockInfo);
					}
				}
			}
		}

		#region _matrixListFactory.CreateMatrixListAllForLoadedPeriod(period); - it's currently depending on stateholder though
		private IEnumerable<IScheduleMatrixPro> createMatrixes(IScheduleDictionary scheduleDictionary, DateOnlyPeriod loadedPeriod, DateOnlyPeriod choosenPeriod, IEnumerable<IPerson> allAgents)
		{
			var period = loadedPeriod.Inflate(10);
			var persons = allAgents;
			var startDate = period.StartDate;
			var matrixes = new List<IScheduleMatrixPro>();
			foreach (var person in persons)
			{
				var date = startDate;
				while (date <= period.EndDate)
				{
					var matrix = createMatrixForPersonAndDate(scheduleDictionary, person, date);
					if (matrix == null)
					{
						date = date.AddDays(1);
						continue;
					}
					matrixes.Add(matrix);
					date = matrix.SchedulePeriod.DateOnlyPeriod.EndDate.AddDays(1);
				}
			}
			_matrixUserLockLocker.Execute(matrixes, choosenPeriod);
			_matrixNotPermittedLocker.Execute(matrixes);

			return matrixes;
		}
		private static IScheduleMatrixPro createMatrixForPersonAndDate(IScheduleDictionary scheduleDictionary, IPerson person, DateOnly date)
		{
			var virtualSchedulePeriod = person.VirtualSchedulePeriod(date);
			if (!virtualSchedulePeriod.IsValid)
				return null;

			IFullWeekOuterWeekPeriodCreator fullWeekOuterWeekPeriodCreator =
				new FullWeekOuterWeekPeriodCreator(virtualSchedulePeriod.DateOnlyPeriod, person);

			return new ScheduleMatrixPro(scheduleDictionary[person],
				fullWeekOuterWeekPeriodCreator,
				virtualSchedulePeriod);
		}
		#endregion
	}
}