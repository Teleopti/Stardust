using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			MessageId = "3")]
		public void DayOffScheduling(IList<IScheduleMatrixPro> matrixListAll,
		                             ISchedulePartModifyAndRollbackService rollbackService,
		                             ISchedulingOptions schedulingOptions,
		                             IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
		{
			if (matrixListAll == null)
				return;
			if (groupPersonBuilderForOptimization == null)
				return;

			var matrixDataList = _matrixDataListCreator.Create(matrixListAll, schedulingOptions);
			var useSameDaysOffOnAll = _matrixDataListInSteadyState.IsListInSteadyState(matrixDataList);
			if (useSameDaysOffOnAll)
			{
				foreach (var matrixData in matrixDataList)
				{
					foreach (var scheduleDayPro in matrixData.Matrix.UnlockedDays)
					{
						var scheduleDate = scheduleDayPro.Day;
						var groupPerson = groupPersonBuilderForOptimization.BuildGroupPerson(matrixData.Matrix.Person, scheduleDate);
						var scheduleDictionary = _schedulingResultStateHolder.Schedules;
						var groupMembers = groupPerson.GroupMembers;
						var restriction = _effectiveRestrictionCreator.GetEffectiveRestriction(groupMembers,
						                                                                       scheduleDate, schedulingOptions,
						                                                                       scheduleDictionary);
						var matrixesOfOneTeam = matrixListAll.Where(x => groupMembers.Contains(x.Person)).ToList();
						addDaysOffForTeam(matrixesOfOneTeam, schedulingOptions, scheduleDate, restriction);
					}
					foreach (var scheduleDayPro in matrixData.Matrix.UnlockedDays)
					{
						var scheduleDate = scheduleDayPro.Day;
						var groupPerson = groupPersonBuilderForOptimization.BuildGroupPerson(matrixData.Matrix.Person, scheduleDate);
						var scheduleDictionary = _schedulingResultStateHolder.Schedules;
						var groupMembers = groupPerson.GroupMembers;
						var restriction = _effectiveRestrictionCreator.GetEffectiveRestriction(groupMembers,
						                                                                       scheduleDate, schedulingOptions,
						                                                                       scheduleDictionary);
						var matrixesOfOneTeam = matrixListAll.Where(x => groupMembers.Contains(x.Person)).ToList();
						addContractDaysOffForTeam(matrixesOfOneTeam, schedulingOptions, rollbackService, scheduleDate, restriction);
					}
				}
			}
			else
			{
				addDaysOff(matrixListAll, schedulingOptions);
				addContractDaysOff(matrixListAll, rollbackService, schedulingOptions);
			}
		}

		private void addDaysOffForTeam(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions,
		                               DateOnly scheduleDate,
		                               IEffectiveRestriction restriction)
		{
			if (restriction == null || restriction.DayOffTemplate == null)
				return;
			if (EffectiveRestrictionCreator.OptionsConflictWithRestrictions(schedulingOptions, restriction))
				return;
			if (matrixList.Any(x => x.GetScheduleDayByKey(scheduleDate).DaySchedulePart().IsScheduled()))
				return;

			foreach (var scheduleMatrixPro in matrixList)
			{
				var part = scheduleMatrixPro.GetScheduleDayByKey(scheduleDate).DaySchedulePart();

				part.CreateAndAddDayOff(restriction.DayOffTemplate);
				_schedulePartModifyAndRollbackService.Modify(part);

				var eventArgs = new SchedulingServiceBaseEventArgs(part);
				OnDayScheduled(eventArgs);
				if (eventArgs.Cancel)
					return;
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

				IList<IScheduleDay> currentOffDaysList;
				bool hasCorrectNumberOfDaysOff = _dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(schedulePeriod,
				                                                                                      out targetDaysOff,
				                                                                                      out currentOffDaysList);
				int currentDaysOff = currentOffDaysList.Count;
				if (hasCorrectNumberOfDaysOff && currentDaysOff >= targetDaysOff)
					break;

				if (currentDaysOff >= targetDaysOff)
					break;

				IScheduleDay part = matrix.GetScheduleDayByKey(scheduleDate).DaySchedulePart();
				if (!_scheduleDayAvailableForDayOffSpecification.IsSatisfiedBy(part))
					break;

				if (!_hasContractDayOffDefinition.IsDayOff(part))
					break;

				part.CreateAndAddDayOff(schedulingOptions.DayOffTemplate);
				rollbackService.Modify(part);

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
					if (part.IsScheduled()) 
						continue;

					var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(part, schedulingOptions);

					if (effectiveRestriction == null || effectiveRestriction.DayOffTemplate == null) 
						continue;

					// borde inte detta hanteras n�r effective restriction skapas och d� returnera null??
					if (EffectiveRestrictionCreator.OptionsConflictWithRestrictions(schedulingOptions, effectiveRestriction)) 
						continue;
					
						part.CreateAndAddDayOff(effectiveRestriction.DayOffTemplate);
						_schedulePartModifyAndRollbackService.Modify(part);
					
					var eventArgs = new SchedulingServiceBaseEventArgs(part);
					OnDayScheduled(eventArgs);
					if (eventArgs.Cancel) return;
				}
			}
		}

		private void addContractDaysOff(IList<IScheduleMatrixPro> matrixListAll,
		                                ISchedulePartModifyAndRollbackService rollbackService,
		                                ISchedulingOptions schedulingOptions)
		{
			if (rollbackService == null)
				throw new ArgumentNullException("rollbackService");

			foreach (var matrix in matrixListAll)
			{
				var schedulePeriod = matrix.SchedulePeriod;
				if (!schedulePeriod.IsValid)
					continue;

				int targetDaysOff;

				IList<IScheduleDay> currentOffDaysList;
				bool hasCorrectNumberOfDaysOff = _dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(schedulePeriod,
				                                                                                      out targetDaysOff,
				                                                                                      out currentOffDaysList);
				int currentDaysOff = currentOffDaysList.Count;
				if (hasCorrectNumberOfDaysOff && currentDaysOff >= targetDaysOff)
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

					IEffectiveRestriction effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(part,
					                                                                                                  schedulingOptions);
					if (effectiveRestriction != null && effectiveRestriction.NotAllowedForDayOffs)
						continue;

					part.CreateAndAddDayOff(schedulingOptions.DayOffTemplate);
					rollbackService.Modify(part);
					currentDaysOff++;

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
