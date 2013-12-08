using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamDayOffScheduler
	{
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		void DayOffScheduling(IList<IScheduleMatrixPro> matrixListAll, IList<IPerson> selectedPersons,
		                      ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions,
		                      IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization);
	}

	public class TeamDayOffScheduler : ITeamDayOffScheduler
	{
		private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IHasContractDayOffDefinition _hasContractDayOffDefinition;
		private readonly IMatrixDataListCreator _matrixDataListCreator;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public TeamDayOffScheduler(
			IDayOffsInPeriodCalculator dayOffsInPeriodCalculator,
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IHasContractDayOffDefinition hasContractDayOffDefinition,
			 IMatrixDataListCreator matrixDataListCreator,
			 ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_hasContractDayOffDefinition = hasContractDayOffDefinition;
			_matrixDataListCreator = matrixDataListCreator;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public void DayOffScheduling(IList<IScheduleMatrixPro> matrixListAll, IList<IPerson> selectedPersons,
		                             ISchedulePartModifyAndRollbackService rollbackService,
		                             ISchedulingOptions schedulingOptions,
		                             IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
		{
			var matrixDataList = _matrixDataListCreator.Create(matrixListAll, schedulingOptions);
			var matrixDataForSelectedPersons = new List<IMatrixData>();
			foreach (var matrixData in matrixDataList)
			{
				if (selectedPersons.Contains(matrixData.Matrix.Person))
					matrixDataForSelectedPersons.Add(matrixData);
			}

			var cancelPressed = false;
			foreach (var matrixData in matrixDataForSelectedPersons)
			{
				var matrix = matrixData.Matrix;
				var unlockedMatrixDays = matrix.UnlockedDays;
				var person = matrix.Person;
				foreach (var scheduleDayPro in unlockedMatrixDays)
				{
					var scheduleDate = scheduleDayPro.Day;
					IEffectiveRestriction restriction;
					var selectedMatrixesForTeam = getMatrixesAndRestriction(matrixListAll, selectedPersons, schedulingOptions,
					                                                        groupPersonBuilderForOptimization, person, scheduleDate,
					                                                        out restriction);
					var canceled = addDaysOffForTeam(selectedMatrixesForTeam, schedulingOptions, rollbackService, scheduleDate, restriction);
					if (canceled)
					{
						return;
					}
				}
				foreach (var scheduleDayPro in unlockedMatrixDays)
				{
					var scheduleDate = scheduleDayPro.Day;
					IEffectiveRestriction restriction;
					var selectedMatrixesForTeam = getMatrixesAndRestriction(matrixListAll, selectedPersons, schedulingOptions,
					                                                        groupPersonBuilderForOptimization, person, scheduleDate,
					                                                        out restriction);
					var canceled = addContractDaysOffForTeam(selectedMatrixesForTeam, schedulingOptions, rollbackService, scheduleDate, restriction);
					if (canceled)
					{
						cancelPressed = true;
						break;
					}
				}

				if (cancelPressed)
					break;
			}
		}

		private List<IScheduleMatrixPro> getMatrixesAndRestriction(IList<IScheduleMatrixPro> matrixListAll, IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions,
		                      IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization,
		                      IPerson person, DateOnly scheduleDate,
		                      out IEffectiveRestriction restriction)
		{
			var selectedMatrixesForTeam = new List<IScheduleMatrixPro>();
			var groupPerson = groupPersonBuilderForOptimization.BuildGroupPerson(person, scheduleDate);

			List<IScheduleMatrixPro> matrixesOfOneTeam;
			restriction = getMatrixOfOneTeam(matrixListAll, schedulingOptions, groupPerson, scheduleDate, out matrixesOfOneTeam);

			foreach (var scheduleMatrixPro in matrixesOfOneTeam)
			{
				if (selectedPersons.Contains(scheduleMatrixPro.Person))
					selectedMatrixesForTeam.Add(scheduleMatrixPro);
			}

			return selectedMatrixesForTeam;
		}

		private IEffectiveRestriction getMatrixOfOneTeam(IEnumerable<IScheduleMatrixPro> matrixListAll, ISchedulingOptions schedulingOptions,
	                                                     IGroupPerson groupPerson, DateOnly scheduleDate,
	                                                     out List<IScheduleMatrixPro> matrixesOfOneTeam)
	    {
	        var scheduleDictionary = _schedulingResultStateHolder.Schedules;
			var groupMembers = groupPerson.GroupMembers.ToList();
			var restriction = _effectiveRestrictionCreator.GetEffectiveRestriction(groupMembers,
	                                                                               scheduleDate, schedulingOptions,
	                                                                               scheduleDictionary);
			matrixesOfOneTeam = matrixListAll.Where(x => groupMembers.Contains(x.Person)).ToList();
	        return restriction;
	    }

		private bool addDaysOffForTeam(IEnumerable<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions,
		                               ISchedulePartModifyAndRollbackService rollbackService,
		                               DateOnly scheduleDate,
		                               IEffectiveRestriction restriction)
		{
			if (restriction == null || restriction.DayOffTemplate == null)
				return false;

			if (EffectiveRestrictionCreator.OptionsConflictWithRestrictions(schedulingOptions, restriction))
				return false;

			foreach (var scheduleMatrixPro in matrixList)
			{
				var part = scheduleMatrixPro.GetScheduleDayByKey(scheduleDate).DaySchedulePart();
				if (part.IsScheduled())
					continue;

				part.CreateAndAddDayOff(restriction.DayOffTemplate);
				rollbackService.Modify(part);

				var eventArgs = new SchedulingServiceBaseEventArgs(part);
				OnDayScheduled(eventArgs);
				if (eventArgs.Cancel)
					return true;
			}

			return false;
		}

		private bool addContractDaysOffForTeam(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions,
		                                       ISchedulePartModifyAndRollbackService rollbackService, DateOnly scheduleDate,
		                                       IEffectiveRestriction restriction)
		{
			foreach (var matrix in matrixList)
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
				if (hasCorrectNumberOfDaysOff || currentDaysOff >= targetDaysOff)
					continue;

				IScheduleDay part = matrix.GetScheduleDayByKey(scheduleDate).DaySchedulePart();

				if (!_hasContractDayOffDefinition.IsDayOff(part))
					continue;

				part.CreateAndAddDayOff(schedulingOptions.DayOffTemplate);
				rollbackService.Modify(part);

				var eventArgs = new SchedulingServiceBaseEventArgs(part);
				OnDayScheduled(eventArgs);
				if (eventArgs.Cancel)
					return true;
			}

			return false;
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
