using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
	public interface ITeamDayOffScheduler
	{
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
		void DayOffScheduling(IList<IScheduleMatrixPro> matrixListAll, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions, IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization);
	}

	public class TeamDayOffScheduler : ITeamDayOffScheduler
	{
		private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private readonly IScheduleDayAvailableForDayOffSpecification _scheduleDayAvailableForDayOffSpecification;
		private readonly IHasContractDayOffDefinition _hasContractDayOffDefinition;
		private readonly IMatrixDataListInSteadyState _matrixDataListInSteadyState;
		private readonly IMatrixDataListCreator _matrixDataListCreator;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public TeamDayOffScheduler(
			IDayOffsInPeriodCalculator dayOffsInPeriodCalculator,
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IScheduleDayAvailableForDayOffSpecification scheduleDayAvailableForDayOffSpecification,
			IHasContractDayOffDefinition hasContractDayOffDefinition,
			IMatrixDataListInSteadyState matrixDataListInSteadyState,
			 IMatrixDataListCreator matrixDataListCreator,
			 ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_scheduleDayAvailableForDayOffSpecification = scheduleDayAvailableForDayOffSpecification;
			_hasContractDayOffDefinition = hasContractDayOffDefinition;
			_matrixDataListInSteadyState = matrixDataListInSteadyState;
			_matrixDataListCreator = matrixDataListCreator;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public void DayOffScheduling(IList<IScheduleMatrixPro> matrixListAll, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions, IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
		{
            if (matrixListAll == null) return;
            if (groupPersonBuilderForOptimization == null) return;
            var person = matrixListAll[0].Person;
			var matrixesOfOnePerson = matrixListAll.Where(x => x.Person == person);
			var listOfMatrixes = new List<List<IScheduleMatrixPro>>();
			foreach (var scheduleMatrixPro in matrixesOfOnePerson)
			{
				var date = scheduleMatrixPro.UnlockedDays.First().Day;
				var matrixesInOneSchedulePeriod = matrixListAll.Where(x => x.SchedulePeriod.DateOnlyPeriod.Contains(date)).ToList();
				listOfMatrixes.Add(matrixesInOneSchedulePeriod);
			}

			foreach (var matrixListInOneSchedulePeriod in listOfMatrixes)
			{
				var matrixDataList = _matrixDataListCreator.Create(matrixListInOneSchedulePeriod, schedulingOptions);
				var useSameDaysOffOnAll = _matrixDataListInSteadyState.IsListInSteadyState(matrixDataList);
				if (useSameDaysOffOnAll)
				{
					foreach (var matrixData in matrixDataList)
					{
						foreach (var scheduleDayPro in matrixData.Matrix.UnlockedDays)
						{
							var scheduleDate = scheduleDayPro.Day;
							var groupPerson = groupPersonBuilderForOptimization.BuildGroupPerson(person, scheduleDate);
							var scheduleDictionary = _schedulingResultStateHolder.Schedules;
							var restriction = _effectiveRestrictionCreator.GetEffectiveRestriction(groupPerson.GroupMembers,
																								   scheduleDate, schedulingOptions,
																								   scheduleDictionary);
							addDaysOffForTeam(matrixListInOneSchedulePeriod, schedulingOptions, scheduleDate, restriction);
						}
						foreach (var scheduleDayPro in matrixData.Matrix.UnlockedDays)
						{
							var scheduleDate = scheduleDayPro.Day;
							var groupPerson = groupPersonBuilderForOptimization.BuildGroupPerson(person, scheduleDate);
							var scheduleDictionary = _schedulingResultStateHolder.Schedules;
							var restriction = _effectiveRestrictionCreator.GetEffectiveRestriction(groupPerson.GroupMembers,
																								   scheduleDate, schedulingOptions,
																								   scheduleDictionary);
							addContractDaysOffForTeam(matrixListInOneSchedulePeriod, schedulingOptions, rollbackService, scheduleDate, restriction);
						}
					}
				}
				else
				{
					addDaysOff(matrixListInOneSchedulePeriod, schedulingOptions);
					addContractDaysOff(matrixListInOneSchedulePeriod, rollbackService, schedulingOptions);
				}
			}
		}

		private void addDaysOffForTeam(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions, DateOnly scheduleDate,
								IEffectiveRestriction restriction)
		{
			if (restriction == null || restriction.DayOffTemplate == null) return;
			if (EffectiveRestrictionCreator.OptionsConflictWithRestrictions(schedulingOptions, restriction)) return;
			if (matrixList.Any(x => x.GetScheduleDayByKey(scheduleDate).DaySchedulePart().IsScheduled())) return;

			foreach (var scheduleMatrixPro in matrixList)
			{
				var part = scheduleMatrixPro.GetScheduleDayByKey(scheduleDate).DaySchedulePart();
				try
				{
					part.CreateAndAddDayOff(restriction.DayOffTemplate);
					_schedulePartModifyAndRollbackService.Modify(part);
				}
				catch (DayOffOutsideScheduleException)
				{
					_schedulePartModifyAndRollbackService.Rollback();
				}
				var eventArgs = new SchedulingServiceBaseEventArgs(part);
				OnDayScheduled(eventArgs);
				if (eventArgs.Cancel) return;
			}
		}

		private void addContractDaysOffForTeam(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions,
										ISchedulePartModifyAndRollbackService rollbackService, DateOnly scheduleDate,
										IEffectiveRestriction restriction)
		{
			if (rollbackService == null)
				throw new ArgumentNullException("rollbackService");
			if (restriction != null && restriction.NotAllowedForDayOffs)
				return;
			foreach (var matrix in matrixList)
			{
				var schedulePeriod = matrix.SchedulePeriod;
				if (!schedulePeriod.IsValid)
					break;

				int targetDaysOff;
				int currentDaysOff;
				bool hasCorrectNumberOfDaysOff = _dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(schedulePeriod,
																									  out targetDaysOff,
																									  out currentDaysOff);
				if (hasCorrectNumberOfDaysOff && currentDaysOff > 0)
					break;

				if (currentDaysOff >= targetDaysOff)
					break;

				IScheduleDay part = matrix.GetScheduleDayByKey(scheduleDate).DaySchedulePart();
				if (!_scheduleDayAvailableForDayOffSpecification.IsSatisfiedBy(part))
					break;

				if (!_hasContractDayOffDefinition.IsDayOff(part))
					break;

				try
				{
					part.CreateAndAddDayOff(schedulingOptions.DayOffTemplate);
					rollbackService.Modify(part);
				}
				catch (DayOffOutsideScheduleException)
				{
					rollbackService.Rollback();
				}
				var eventArgs = new SchedulingServiceBaseEventArgs(part);
				OnDayScheduled(eventArgs);
				if (eventArgs.Cancel)
					break;
			}
		}

		private void addDaysOff(IEnumerable<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions)//, IEnumerable<DateOnly> dates, IEnumerable<IPerson> persons)
		{

			foreach (var scheduleMatrixPro in matrixList)
			{
				foreach (var scheduleDayPro in scheduleMatrixPro.UnlockedDays)
				{
					var part = scheduleDayPro.DaySchedulePart();
					if (part.IsScheduled()) continue;

					var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(part, schedulingOptions);

					if (effectiveRestriction == null || effectiveRestriction.DayOffTemplate == null) continue;
					// borde inte detta hanteras när effective restriction skapas och då returnera null??
					if (EffectiveRestrictionCreator.OptionsConflictWithRestrictions(schedulingOptions, effectiveRestriction)) continue;
					try
					{
						part.CreateAndAddDayOff(effectiveRestriction.DayOffTemplate);
						_schedulePartModifyAndRollbackService.Modify(part);
					}
					catch (DayOffOutsideScheduleException)
					{
						_schedulePartModifyAndRollbackService.Rollback();
					}

					var eventArgs = new SchedulingServiceBaseEventArgs(part);
					OnDayScheduled(eventArgs);
					if (eventArgs.Cancel) return;
				}
			}
		}

		private void addContractDaysOff(IList<IScheduleMatrixPro> matrixListAll, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions)
		{
			if (rollbackService == null)
				throw new ArgumentNullException("rollbackService");

			foreach (var matrix in matrixListAll)
			{
				var schedulePeriod = matrix.SchedulePeriod;
				if (!schedulePeriod.IsValid)
					continue;

				int targetDaysOff;
				int currentDaysOff;
				bool hasCorrectNumberOfDaysOff = _dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(schedulePeriod,
																									  out targetDaysOff,
																									  out currentDaysOff);
				if (hasCorrectNumberOfDaysOff && currentDaysOff > 0)
					continue;



				foreach (var scheduleDayPro in matrix.UnlockedDays)
				{
					if (currentDaysOff >= targetDaysOff)
						continue;

					IScheduleDay part = scheduleDayPro.DaySchedulePart();
					if (!_scheduleDayAvailableForDayOffSpecification.IsSatisfiedBy(part))
						continue;

					if (!_hasContractDayOffDefinition.IsDayOff(part))
						continue;

					IEffectiveRestriction effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(part, schedulingOptions);
					if (effectiveRestriction != null && effectiveRestriction.NotAllowedForDayOffs)
						continue;

					try
					{
						part.CreateAndAddDayOff(schedulingOptions.DayOffTemplate);
						rollbackService.Modify(part);
						currentDaysOff++;
					}
					catch (DayOffOutsideScheduleException)
					{
						rollbackService.Rollback();
					}
					var eventArgs = new SchedulingServiceBaseEventArgs(part);
					OnDayScheduled(eventArgs);
					if (eventArgs.Cancel)
						return;
				}
			}
		}

		protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
		{
			EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
			if (temp != null)
			{
				temp(this, scheduleServiceBaseEventArgs);
			}
		}
	}
}
