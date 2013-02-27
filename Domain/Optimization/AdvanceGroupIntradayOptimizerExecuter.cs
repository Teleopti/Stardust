using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IAdvanceGroupIntradayOptimizerExecuter
	{
		void Rollback(DateOnly dateOnly);

		bool Execute(IList<IScheduleDay> daysToDelete,
		                             IList<IScheduleDay> daysToSave,
		                             IList<IScheduleMatrixPro> allMatrixes,
		                             IOptimizationOverLimitByRestrictionDecider optimizationOverLimitByRestrictionDecider);
	}

	public class AdvanceGroupIntradayOptimizerExecuter : IAdvanceGroupIntradayOptimizerExecuter
	{
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private readonly IDeleteAndResourceCalculateService _deleteService;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly IOptimizationPreferences _optimizerPreferences;
		private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
		private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IRestrictionAggregator _restrictionAggregator;
		private readonly IDynamicBlockFinder _dynamicBlockFinder;
		private readonly IGroupPersonBuilderBasedOnContractTime _groupPersonBuilderBasedOnContractTime;
		private readonly ISchedulingOptions _schedulingOptions;
		private readonly ISkillDayPeriodIntervalDataGenerator _skillDayPeriodIntervalDataGenerator;
		private readonly IWorkShiftFilterService _workShiftFilterService;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly ITeamScheduling _teamScheduling;

		public AdvanceGroupIntradayOptimizerExecuter(
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IDeleteAndResourceCalculateService deleteService,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			IOptimizationPreferences optimizerPreferences,
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter,
			IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization,
			IResourceOptimizationHelper resourceOptimizationHelper,
			IRestrictionAggregator restrictionAggregator,
			IDynamicBlockFinder dynamicBlockFinder,
			IGroupPersonBuilderBasedOnContractTime groupPersonBuilderBasedOnContractTime,
			ISchedulingOptions schedulingOptions,
			ISkillDayPeriodIntervalDataGenerator skillDayPeriodIntervalDataGenerator,
			IWorkShiftFilterService workShiftFilterService,
			IWorkShiftSelector workShiftSelector,
			ITeamScheduling teamScheduling)
		{
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_deleteService = deleteService;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_optimizerPreferences = optimizerPreferences;
			_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
			_groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_restrictionAggregator = restrictionAggregator;
			_dynamicBlockFinder = dynamicBlockFinder;
			_groupPersonBuilderBasedOnContractTime = groupPersonBuilderBasedOnContractTime;
			_schedulingOptions = schedulingOptions;
			_skillDayPeriodIntervalDataGenerator = skillDayPeriodIntervalDataGenerator;
			_workShiftFilterService = workShiftFilterService;
			_workShiftSelector = workShiftSelector;
			_teamScheduling = teamScheduling;
		}

		public bool Execute(IList<IScheduleDay> daysToDelete,
							IList<IScheduleDay> daysToSave,
							IList<IScheduleMatrixPro> allMatrixes,
							IOptimizationOverLimitByRestrictionDecider optimizationOverLimitByRestrictionDecider)
		{
			_schedulePartModifyAndRollbackService.ClearModificationCollection();

			IList<IScheduleDay> cleanedList = new List<IScheduleDay>();
			foreach (var scheduleDay in daysToDelete)
			{
				SchedulePartView significant = scheduleDay.SignificantPart();
				if (significant != SchedulePartView.FullDayAbsence && significant != SchedulePartView.DayOff &&
					significant != SchedulePartView.ContractDayOff)
					cleanedList.Add(scheduleDay);
			}
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences);
			_deleteService.DeleteWithResourceCalculation(cleanedList, _schedulePartModifyAndRollbackService,
														 schedulingOptions.ConsiderShortBreaks);
			var unLockedDays = new List<DateOnly>();
			for (var i = 0; i < allMatrixes.Count; i++)
			{
				var openMatrixList = allMatrixes.Where(x => x.Person.Equals(allMatrixes[i].Person));
				foreach (var scheduleMatrixPro in openMatrixList)
				{
					foreach (var scheduleDayPro in scheduleMatrixPro.EffectivePeriodDays.OrderBy(x => x.Day))
					{
						if (scheduleMatrixPro.UnlockedDays.Contains(scheduleDayPro))
							unLockedDays.Add(scheduleDayPro.Day);
					}
				}
			}
			foreach (var scheduleDay in daysToSave)
			{
				if (!scheduleDay.IsScheduled())
					continue;

				SchedulePartView significant = scheduleDay.SignificantPart();
				if (significant == SchedulePartView.FullDayAbsence || significant == SchedulePartView.DayOff ||
					significant == SchedulePartView.ContractDayOff)
					continue;

				DateOnly shiftDate = scheduleDay.DateOnlyAsPeriod.DateOnly;
				_mainShiftOptimizeActivitySpecificationSetter.SetSpecification(schedulingOptions, _optimizerPreferences,
																			   scheduleDay.AssignmentHighZOrder().MainShift,
																			   shiftDate);

				rescheduleTeamBlock(shiftDate, allMatrixes, scheduleDay.Person, unLockedDays);
			}

			return true;
		}

		private void rescheduleTeamBlock(DateOnly scheduleDate,
			IList<IScheduleMatrixPro> allMatrixList,
			IPerson selectedPerson,
			IList<DateOnly> unLockedDays)
		{
            
			var allGroupPersonListOnStartDate = new HashSet<IGroupPerson>();
			allGroupPersonListOnStartDate.Add(_groupPersonBuilderForOptimization.BuildGroupPerson(selectedPerson, scheduleDate));
			
			foreach (var fullGroupPerson in allGroupPersonListOnStartDate.GetRandom(allGroupPersonListOnStartDate.Count, true))
			{
				var groupPersonList = _groupPersonBuilderBasedOnContractTime.SplitTeams(fullGroupPerson, scheduleDate);
                var dateOnlyList = _dynamicBlockFinder.ExtractBlockDays(scheduleDate, fullGroupPerson);
                foreach (var groupPerson in groupPersonList)
				{
					var groupMatrixList = getScheduleMatrixProList(groupPerson, scheduleDate, allMatrixList);
					var restriction = _restrictionAggregator.Aggregate(dateOnlyList, groupPerson, groupMatrixList, _schedulingOptions);
					if (restriction == null) continue;
					var shifts = _workShiftFilterService.Filter(scheduleDate, groupPerson, groupMatrixList, restriction, _schedulingOptions);
					if (shifts != null && shifts.Count > 0)
					{
						IShiftProjectionCache bestShiftProjectionCache;
						if (shifts.Count == 1)
							bestShiftProjectionCache = shifts.First();
						else
						{
							var activityInternalData = _skillDayPeriodIntervalDataGenerator.Generate(fullGroupPerson, dateOnlyList);
							bestShiftProjectionCache = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
																									 _schedulingOptions.WorkShiftLengthHintOption,
																									 _schedulingOptions.UseMinimumPersons,
																									 _schedulingOptions.UseMaximumPersons);
						}
						_teamScheduling.Execute(scheduleDate, dateOnlyList, groupMatrixList, groupPerson,
						                        bestShiftProjectionCache, unLockedDays, new List<IPerson> {selectedPerson});
					}
				}
			}
		}

		private static List<IScheduleMatrixPro> getScheduleMatrixProList(IGroupPerson groupPerson, DateOnly startDate,
		                                                          IEnumerable<IScheduleMatrixPro> matrixList)
		{
			var person = groupPerson;
			var date = startDate;
			var groupMatrixList =
				matrixList.Where(x => person.GroupMembers.Contains(x.Person) && x.SchedulePeriod.DateOnlyPeriod.Contains(date))
				          .ToList();
			return groupMatrixList;
		}

		public void Rollback(DateOnly dateOnly)
		{
			_schedulePartModifyAndRollbackService.Rollback();
			recalculateDay(dateOnly);
		}

		private void recalculateDay(DateOnly dateOnly)
		{
			_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true);
			_resourceOptimizationHelper.ResourceCalculateDate(dateOnly.AddDays(1), true, true);
		}
	}
}
