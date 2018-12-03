using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class TeamDayOffScheduling
	{
		private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IHasContractDayOffDefinition _hasContractDayOffDefinition;
		private readonly MatrixDataListCreator _matrixDataListCreator;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
		private readonly IScheduleDayAvailableForDayOffSpecification _scheduleDayAvailableForDayOffSpecification;
		private readonly ICurrentAuthorization _authorization;

		public TeamDayOffScheduling(
			IDayOffsInPeriodCalculator dayOffsInPeriodCalculator,
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IHasContractDayOffDefinition hasContractDayOffDefinition,
			MatrixDataListCreator matrixDataListCreator,
			Func<ISchedulingResultStateHolder> schedulingResultStateHolder,
			IScheduleDayAvailableForDayOffSpecification scheduleDayAvailableForDayOffSpecification,
			ICurrentAuthorization authorization)
		{
			_dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_hasContractDayOffDefinition = hasContractDayOffDefinition;
			_matrixDataListCreator = matrixDataListCreator;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_scheduleDayAvailableForDayOffSpecification = scheduleDayAvailableForDayOffSpecification;
			_authorization = authorization;
		}

		public void DayOffScheduling(ISchedulingCallback schedulingCallback, IEnumerable<IScheduleMatrixPro> matrixes, IEnumerable<IPerson> selectedPersons,
			ISchedulePartModifyAndRollbackService rollbackService,
			SchedulingOptions schedulingOptions,
			IGroupPersonBuilderWrapper groupPersonBuilderForOptimization)
		{
			var matrixDataList = _matrixDataListCreator.Create(matrixes, schedulingOptions);
			var matrixDataForSelectedPersons =
				matrixDataList.Where(matrixData => selectedPersons.Contains(matrixData.Matrix.Person)).ToList();

			foreach (var matrixData in matrixDataForSelectedPersons)
			{
				var matrix = matrixData.Matrix;
				var unlockedMatrixDays = matrix.UnlockedDays;
				var person = matrix.Person;
				foreach (var scheduleDayPro in unlockedMatrixDays)
				{
					var scheduleDate = scheduleDayPro.Day;
					var selectedMatrixesForTeam = getMatrixesAndRestriction(matrixes, schedulingOptions, groupPersonBuilderForOptimization, person, scheduleDate, out IEffectiveRestriction restriction);
					var canceled = addDaysOffForTeam(schedulingCallback, selectedMatrixesForTeam, schedulingOptions, rollbackService, scheduleDate, restriction);
					if (canceled)
					{
						return;
					}
				}
			}

			addContractDaysOffForTeam(schedulingCallback, matrixDataForSelectedPersons.Select(x => x.Matrix), schedulingOptions, rollbackService);
		}

		private IEnumerable<IScheduleMatrixPro> getMatrixesAndRestriction(IEnumerable<IScheduleMatrixPro> matrixes, SchedulingOptions schedulingOptions,
			IGroupPersonBuilderWrapper groupPersonBuilderForOptimization,
			IPerson person, DateOnly scheduleDate,
			out IEffectiveRestriction restriction)
		{
			var group = groupPersonBuilderForOptimization.ForOptimization().BuildGroup(_schedulingResultStateHolder().LoadedAgents, person, scheduleDate);

			restriction = getMatrixOfOneTeam(matrixes, schedulingOptions, group, scheduleDate, out List<IScheduleMatrixPro> matrixesOfOneTeam, person);

			var selectedMatrixesForOnePerson =
				matrixesOfOneTeam.Where(scheduleMatrixPro => scheduleMatrixPro.Person.Equals(person)).ToList();
			return selectedMatrixesForOnePerson;
		}

		private IEffectiveRestriction getMatrixOfOneTeam(IEnumerable<IScheduleMatrixPro> matrixes, SchedulingOptions schedulingOptions, Group group, DateOnly scheduleDate, out List<IScheduleMatrixPro> matrixesOfOneTeam, IPerson person)
		{
			var scheduleDictionary = _schedulingResultStateHolder().Schedules;
			var groupMembers = group.GroupMembers.ToArray();
			var restriction = _effectiveRestrictionCreator.GetEffectiveRestrictionForSinglePerson(person,
				scheduleDate, schedulingOptions,
				scheduleDictionary);
			var matrixesOfOne = matrixes.Where(x => groupMembers.Contains(x.Person));
			matrixesOfOneTeam = matrixesOfOne.Where(scheduleMatrixPro =>
			{
				var schedulePeriod = scheduleMatrixPro.SchedulePeriod;
				return schedulePeriod.IsValid && schedulePeriod.DateOnlyPeriod.Contains(scheduleDate);
			}).ToList();

			return restriction;
		}

		private static bool addDaysOffForTeam(ISchedulingCallback schedulingCallback, IEnumerable<IScheduleMatrixPro> matrixes, SchedulingOptions schedulingOptions,
			ISchedulePartModifyAndRollbackService rollbackService, DateOnly scheduleDate, IEffectiveRestriction restriction)
		{
			if (restriction?.DayOffTemplate == null)
				return false;
			if (EffectiveRestrictionCreator.OptionsConflictWithRestrictions(schedulingOptions, restriction))
				return false;

			foreach (var scheduleMatrixPro in matrixes)
			{
				var part = scheduleMatrixPro.GetScheduleDayByKey(scheduleDate).DaySchedulePart();
				if (part.IsScheduled())
					continue;

				part.CreateAndAddDayOff(restriction.DayOffTemplate);
				rollbackService.Modify(part, NewBusinessRuleCollection.Minimum());

				schedulingCallback.Scheduled(new SchedulingCallbackInfo(part, true));
				if (schedulingCallback.IsCancelled)
					return true;
			}

			return false;
		}

		private void addContractDaysOffForTeam(ISchedulingCallback schedulingCallback, IEnumerable<IScheduleMatrixPro> matrixList, SchedulingOptions schedulingOptions,
			ISchedulePartModifyAndRollbackService rollbackService)
		{
			var authorization = _authorization.Current();
			var scheduleDictionary = _schedulingResultStateHolder().Schedules;
			foreach (var matrix in matrixList)
			{
				_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(scheduleDictionary, matrix.SchedulePeriod, out var targetDaysOff, out var currentOffDaysList);

				var schedulePeriod = matrix.SchedulePeriod;
				if (!schedulePeriod.IsValid)
					continue;

				int currentDaysOff = currentOffDaysList.Count;
				if (currentDaysOff >= targetDaysOff)
					continue;

				var unlockedDates = matrix.UnlockedDays.Select(sdp => sdp.Day).ToArray();
				var foundSpot = true;
				
				while (currentOffDaysList.Count < targetDaysOff && foundSpot)
				{
					var sortedWeeks = _dayOffsInPeriodCalculator.WeekPeriodsSortedOnDayOff(matrix);
					foundSpot = false;

					foreach (var dayOffOnPeriod in sortedWeeks)
					{
						if (schedulingCallback.IsCancelled)
							return;
						var bestScheduleDay = dayOffOnPeriod.FindBestSpotForDayOff(_hasContractDayOffDefinition, _scheduleDayAvailableForDayOffSpecification, _effectiveRestrictionCreator, schedulingOptions);
						if (bestScheduleDay == null) continue;
						if (!unlockedDates.Contains(bestScheduleDay.DateOnlyAsPeriod.DateOnly))
							continue;
						try
						{
							bestScheduleDay.CreateAndAddDayOff(schedulingOptions.DayOffTemplate);

							var personAssignment = bestScheduleDay.PersonAssignment();
							if (!authorization.IsPermitted(personAssignment.FunctionPath, bestScheduleDay.DateOnlyAsPeriod.DateOnly, bestScheduleDay.Person)) continue;

							rollbackService.Modify(bestScheduleDay, NewBusinessRuleCollection.Minimum());
							foundSpot = true;
						}
						catch (DayOffOutsideScheduleException)
						{
							rollbackService.Rollback();
						}
						schedulingCallback.Scheduled(new SchedulingCallbackInfo(bestScheduleDay, true));
						if (schedulingCallback.IsCancelled)
							return;

						break;
					}

					_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(scheduleDictionary, schedulePeriod, out targetDaysOff, out currentOffDaysList);
				}
			}
		}
	}
}