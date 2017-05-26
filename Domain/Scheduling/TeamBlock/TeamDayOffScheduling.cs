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
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class TeamDayOffScheduling
	{
		private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IHasContractDayOffDefinition _hasContractDayOffDefinition;
		private readonly IMatrixDataListCreator _matrixDataListCreator;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
		private readonly IScheduleDayAvailableForDayOffSpecification _scheduleDayAvailableForDayOffSpecification;
		private readonly ICurrentAuthorization _authorization;

		public TeamDayOffScheduling(
			IDayOffsInPeriodCalculator dayOffsInPeriodCalculator,
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IHasContractDayOffDefinition hasContractDayOffDefinition,
			IMatrixDataListCreator matrixDataListCreator,
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
			var matrixDataForSelectedPersons = new List<IMatrixData>();
			foreach (var matrixData in matrixDataList)
			{
				if (selectedPersons.Contains(matrixData.Matrix.Person))
					matrixDataForSelectedPersons.Add(matrixData);
			}

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
			var selectedMatrixesForOnePerson = new List<IScheduleMatrixPro>();

			var group = groupPersonBuilderForOptimization.ForOptimization().BuildGroup(_schedulingResultStateHolder().PersonsInOrganization, person, scheduleDate);

			restriction = getMatrixOfOneTeam(matrixes, schedulingOptions, group, scheduleDate, out List<IScheduleMatrixPro> matrixesOfOneTeam, person);

			foreach (var scheduleMatrixPro in matrixesOfOneTeam)
			{
				var currentPerson = scheduleMatrixPro.Person;
				if (currentPerson.Equals(person))
				{
					selectedMatrixesForOnePerson.Add(scheduleMatrixPro);
				}
			}

			return selectedMatrixesForOnePerson;
		}

		private IEffectiveRestriction getMatrixOfOneTeam(IEnumerable<IScheduleMatrixPro> matrixes, SchedulingOptions schedulingOptions, Group group, DateOnly scheduleDate, out List<IScheduleMatrixPro> matrixesOfOneTeam, IPerson person)
		{
			var scheduleDictionary = _schedulingResultStateHolder().Schedules;
			var groupMembers = group.GroupMembers.ToList();
			var restriction = _effectiveRestrictionCreator.GetEffectiveRestrictionForSinglePerson(person,
				scheduleDate, schedulingOptions,
				scheduleDictionary);
			var matrixesOfOne = matrixes.Where(x => groupMembers.Contains(x.Person)).ToList();
			matrixesOfOneTeam = new List<IScheduleMatrixPro>();

			foreach (var scheduleMatrixPro in matrixesOfOne)
			{
				var schedulePeriod = scheduleMatrixPro.SchedulePeriod;
				if (schedulePeriod.IsValid && schedulePeriod.DateOnlyPeriod.Contains(scheduleDate))
					matrixesOfOneTeam.Add(scheduleMatrixPro);
			}

			return restriction;
		}

		private static bool addDaysOffForTeam(ISchedulingCallback schedulingCallback, IEnumerable<IScheduleMatrixPro> matrixes, SchedulingOptions schedulingOptions,
			ISchedulePartModifyAndRollbackService rollbackService, DateOnly scheduleDate, IEffectiveRestriction restriction)
		{
			if (restriction == null || restriction.DayOffTemplate == null)
				return false;
			if (EffectiveRestrictionCreator.OptionsConflictWithRestrictions(schedulingOptions, restriction))
				return false;

			foreach (var scheduleMatrixPro in matrixes)
			{
				var part = scheduleMatrixPro.GetScheduleDayByKey(scheduleDate).DaySchedulePart();
				if (part.IsScheduled())
					continue;

				part.CreateAndAddDayOff(restriction.DayOffTemplate);
				rollbackService.Modify(part);

				schedulingCallback.Scheduled(new SchedulingCallbackInfo(part, true));
				if (schedulingCallback.IsCancelled)
					return true;
			}

			return false;
		}

		private void addContractDaysOffForTeam(ISchedulingCallback schedulingCallback, IEnumerable<IScheduleMatrixPro> matrixList, SchedulingOptions schedulingOptions,
			ISchedulePartModifyAndRollbackService rollbackService)
		{
			foreach (var matrix in matrixList)
			{
				int targetDaysOff;
				IList<IScheduleDay> currentOffDaysList;
				_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(matrix.SchedulePeriod, out targetDaysOff, out currentOffDaysList);

				var schedulePeriod = matrix.SchedulePeriod;
				if (!schedulePeriod.IsValid)
					continue;

				int currentDaysOff = currentOffDaysList.Count;
				if (currentDaysOff >= targetDaysOff)
					continue;

				var unlockedDates = matrix.UnlockedDays.Select(sdp => sdp.Day).ToList();
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
							if (!_authorization.Current().IsPermitted(personAssignment.FunctionPath, bestScheduleDay.DateOnlyAsPeriod.DateOnly, bestScheduleDay.Person)) continue;

							rollbackService.Modify(bestScheduleDay);
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

					_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(schedulePeriod, out targetDaysOff, out currentOffDaysList);
				}
			}
		}
	}
}