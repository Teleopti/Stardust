using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Security.Principal;
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
		private readonly IScheduleDayAvailableForDayOffSpecification _scheduleDayAvailableForDayOffSpecification;
		private SchedulingServiceBaseEventArgs _progressEvent;

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public TeamDayOffScheduler(
			IDayOffsInPeriodCalculator dayOffsInPeriodCalculator,
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IHasContractDayOffDefinition hasContractDayOffDefinition,
			 IMatrixDataListCreator matrixDataListCreator,
			 ISchedulingResultStateHolder schedulingResultStateHolder,
			IScheduleDayAvailableForDayOffSpecification scheduleDayAvailableForDayOffSpecification)
		{
			_dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_hasContractDayOffDefinition = hasContractDayOffDefinition;
			_matrixDataListCreator = matrixDataListCreator;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_scheduleDayAvailableForDayOffSpecification = scheduleDayAvailableForDayOffSpecification;
		}

		public void DayOffScheduling(IList<IScheduleMatrixPro> matrixListAll, IList<IPerson> selectedPersons,
		                             ISchedulePartModifyAndRollbackService rollbackService,
		                             ISchedulingOptions schedulingOptions,
		                             IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
		{
			_progressEvent = null;
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
					var canceled = addContractDaysOffForTeam(selectedMatrixesForTeam, schedulingOptions, rollbackService);
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

		private IEnumerable<IScheduleMatrixPro> getMatrixesAndRestriction(IEnumerable<IScheduleMatrixPro> matrixListAll, IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions,
		                      IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization,
		                      IPerson person, DateOnly scheduleDate,
		                      out IEffectiveRestriction restriction)
		{
			var selectedMatrixesForOnePerson = new List<IScheduleMatrixPro>();
			var group = groupPersonBuilderForOptimization.BuildGroup(person, scheduleDate);

			List<IScheduleMatrixPro> matrixesOfOneTeam;
			restriction = getMatrixOfOneTeam(matrixListAll, schedulingOptions, group, scheduleDate, out matrixesOfOneTeam, person);

			foreach (var scheduleMatrixPro in matrixesOfOneTeam)
			{
				var currentPerson = scheduleMatrixPro.Person;
				if (selectedPersons.Contains(currentPerson) && currentPerson == person)
					selectedMatrixesForOnePerson.Add(scheduleMatrixPro);
			}

			return selectedMatrixesForOnePerson;
		}

		private IEffectiveRestriction getMatrixOfOneTeam(IEnumerable<IScheduleMatrixPro> matrixListAll, ISchedulingOptions schedulingOptions, Group group, DateOnly scheduleDate, out List<IScheduleMatrixPro> matrixesOfOneTeam, IPerson person)
	    {
	        var scheduleDictionary = _schedulingResultStateHolder.Schedules;
			var groupMembers = group.GroupMembers.ToList();
			var restriction = _effectiveRestrictionCreator.GetEffectiveRestrictionForSinglePerson( person,
	                                                                               scheduleDate, schedulingOptions,
	                                                                               scheduleDictionary);
			var matrixesOfOne = matrixListAll.Where(x => groupMembers.Contains(x.Person)).ToList();
			matrixesOfOneTeam = new List<IScheduleMatrixPro>();
			
			foreach (var scheduleMatrixPro in matrixesOfOne)
			{
				var schedulePeriod = scheduleMatrixPro.SchedulePeriod;
				if (schedulePeriod.IsValid && schedulePeriod.DateOnlyPeriod.Contains(scheduleDate))
					matrixesOfOneTeam.Add(scheduleMatrixPro);
			}
			
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

				var eventArgs = new SchedulingServiceSuccessfulEventArgs(part);
				OnDayScheduled(eventArgs);
				if (eventArgs.Cancel)
					return true;

				if (_progressEvent != null && _progressEvent.UserCancel)
					return true;
			}

			return false;
		}

		private bool addContractDaysOffForTeam(IEnumerable<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions,
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
				if(currentDaysOff >= targetDaysOff)
					continue;

				var foundSpot = true;

				while (currentOffDaysList.Count < targetDaysOff && foundSpot)
				{
					var sortedWeeks = _dayOffsInPeriodCalculator.WeekPeriodsSortedOnDayOff(matrix);
					foundSpot = false;

					foreach (var dayOffOnPeriod in sortedWeeks)
					{
						var bestScheduleDay = dayOffOnPeriod.FindBestSpotForDayOff(_hasContractDayOffDefinition, _scheduleDayAvailableForDayOffSpecification, _effectiveRestrictionCreator, schedulingOptions);
						if (bestScheduleDay == null) continue;
						try
						{
							bestScheduleDay.CreateAndAddDayOff(schedulingOptions.DayOffTemplate);

							var personAssignment = bestScheduleDay.PersonAssignment();
							var authorization = PrincipalAuthorization.Instance();
							if (!(authorization.IsPermitted(personAssignment.FunctionPath, bestScheduleDay.DateOnlyAsPeriod.DateOnly, bestScheduleDay.Person))) continue;

							rollbackService.Modify(bestScheduleDay);
							foundSpot = true;
						}
						catch (DayOffOutsideScheduleException)
						{
							rollbackService.Rollback();
						}
						var eventArgs = new SchedulingServiceSuccessfulEventArgs(bestScheduleDay);
						OnDayScheduled(eventArgs);
						if (eventArgs.Cancel)
							return true;

						if (_progressEvent != null && _progressEvent.UserCancel)
							return true;

						break;
					}

					_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(schedulePeriod, out targetDaysOff, out currentOffDaysList);
				}	
			}

			return false;
		}

		protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
		{
			EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
			if (temp != null)
			{
				temp(this, scheduleServiceBaseEventArgs);

				if (_progressEvent != null && _progressEvent.UserCancel)
					return;

				_progressEvent = scheduleServiceBaseEventArgs;
			}
		}
	}
}
