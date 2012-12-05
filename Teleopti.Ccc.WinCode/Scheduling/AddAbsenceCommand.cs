using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class AddAbsenceCommand : AddLayerCommand
	{
		private readonly IPrincipalAuthorization _principalAuthorization;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public AddAbsenceCommand(ISchedulerStateHolder schedulerStateHolder, IScheduleViewBase scheduleViewBase, ISchedulePresenterBase presenter, IList<IScheduleDay> scheduleParts, IPrincipalAuthorization principalAuthorization)
			: base(schedulerStateHolder, scheduleViewBase, presenter, scheduleParts ?? scheduleViewBase.CurrentColumnSelectedSchedules())
		{
			_principalAuthorization = principalAuthorization;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public override void Execute()
		{

			var selectedSchedulesFromView = ScheduleViewBase.SelectedSchedules();
			IList<IScheduleDay> selectedSchedules = !selectedSchedulesFromView.IsEmpty()
														? selectedSchedulesFromView
														: ScheduleParts;
			IList<IScheduleDay> originalSchedules = selectedSchedules;
			if (!VerifySelectedSchedule(selectedSchedules)) return;

			var n = from a in SchedulerStateHolder.CommonStateHolder.Absences
					where !((IDeleteTag)a).IsDeleted
					select a;

			var fallbackDefaultHours =
				new SetupDateTimePeriodDefaultLocalHoursForAbsence(selectedSchedules[0]);
			var periodFromSchedules = new SetupDateTimePeriodToSchedulesIfTheyExist(selectedSchedules, fallbackDefaultHours);
			ISetupDateTimePeriod periodSetup = new SetupDateTimePeriodToDefaultPeriod(DefaultPeriod, periodFromSchedules);

			IAddLayerViewModel<IAbsence> addAbsenceDialog = ScheduleViewBase.CreateAddAbsenceViewModel(n.ToList(), periodSetup);
			bool dialogResult = addAbsenceDialog.Result;
			if (!dialogResult) return;

			IAbsence absence = addAbsenceDialog.SelectedItem;
			DateTimePeriod absencePeriod = addAbsenceDialog.SelectedPeriod;

			IList<IPerson> selectedPersons = new List<IPerson>();
			foreach (IScheduleDay scheduleDay in originalSchedules)
			{
				IPerson currentPerson = scheduleDay.Person;
				if (currentPerson != null)
				{
					if (!selectedPersons.Contains(currentPerson))
						selectedPersons.Add(currentPerson);
				}
			}

			foreach (IPerson selectedPerson in selectedPersons)
			{
				IList<IScheduleDay> selectedAbsenceDaysPerPerson = new List<IScheduleDay>();
                
				var dateOnlyPeriod = absencePeriod.ToDateOnlyPeriod(selectedPerson.PermissionInformation.DefaultTimeZone());
				foreach (var dateOnly in dateOnlyPeriod.DayCollection())
				{
					var scheduleday = SchedulerStateHolder.Schedules[selectedPerson].ScheduledDay(dateOnly);
					if (scheduleday != null)
						selectedAbsenceDaysPerPerson.Add(scheduleday);
				}

				IList<IScheduleDay> selectedUnlockedAbsenceDaysPerPerson = removeLockedSchedules(selectedAbsenceDaysPerPerson);
				IList<IList<IScheduleDay>> aggregateList = AggregatedAbsenceList(selectedUnlockedAbsenceDaysPerPerson);

				foreach (IList<IScheduleDay> scheduleDays in aggregateList)
				{
					IScheduleDay firstDayInList = scheduleDays[0];
					IScheduleDay lastDayInList = scheduleDays[scheduleDays.Count - 1];

					DateTimePeriod aggregateAbsencePeriod = new DateTimePeriod(firstDayInList.Period.StartDateTime, lastDayInList.Period.EndDateTime);
					DateTimePeriod? currentAbsencePeriod = aggregateAbsencePeriod.Intersection(absencePeriod);
					if (!currentAbsencePeriod.HasValue)
						continue;

					var modifiedList = new List<IScheduleDay>();

					/* Bugfix for 20329> ESL not updating when making a multiday absence and do rollback
					 * The main problem is that the absence layer is added to one scedule day only but can last over several days.
					 * As a result, only one day is added to the undo list, and the information about the length of the layer is lost during the
					 * restore action so only one day is recalculated in ESL
					 * The solution is to fake schedule modification with a clone on those days that are affected.
					*/
					// clone all affected days 
					for (int i = 1; i < scheduleDays.Count; i++)
					{
						IScheduleDay current = scheduleDays[i];
						IScheduleDay clone = current.Clone() as IScheduleDay;
						modifiedList.Add(clone);
					}

					// finally the absence is added to the first day only
					IAbsenceLayer absLayer = new AbsenceLayer(absence, currentAbsencePeriod.Value);
					firstDayInList.CreateAndAddAbsence(absLayer);
					modifiedList.Add(firstDayInList);

					if (!Presenter.ModifySchedulePart(modifiedList))
                    {
                        var scheduleRange = (IValidateScheduleRange) SchedulerStateHolder.Schedules[selectedPerson];
                        scheduleRange.ValidateBusinessRules(NewBusinessRuleCollection.MinimumAndPersonAccount(SchedulerStateHolder.SchedulingResultState));
                        return;
                    }
				}

				ScheduleViewBase.RefreshRangeForAgentPeriod(selectedPerson, absencePeriod);
			}
			
		}

		private IList<IScheduleDay> removeLockedSchedules(IEnumerable<IScheduleDay> selectedSchedules)
		{
			IList<IScheduleDay> schedulesToKeep = new List<IScheduleDay>();
			foreach (IScheduleDay schedulePart in selectedSchedules)
			{
				GridlockDictionary lockDictionary = Presenter.LockManager.Gridlocks(schedulePart.Person, schedulePart.DateOnlyAsPeriod.DateOnly);
				var keep = true;
				if (!(lockDictionary == null || lockDictionary.Count == 0))
				{
					foreach (var gridLock in lockDictionary)
					{
						if (!gridLock.Value.LockType.Equals(LockType.WriteProtected))
						{
							keep = false;
							break;
						}

						if (_principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule))
							continue;
						keep = false;
						break;
					}
				}

				if (keep)
					schedulesToKeep.Add(schedulePart);
			}

			return schedulesToKeep.OrderBy(p => p.DateOnlyAsPeriod.DateOnly).ToList();
		}

		private static IList<IList<IScheduleDay>> AggregatedAbsenceList(IList<IScheduleDay> scheduleDays)
		{
			ListAggregator<IScheduleDay> aggregator = new ListAggregator<IScheduleDay>();
			return aggregator.Aggregate(scheduleDays, AreScheduleDaysAttached);
		}


		private static bool AreScheduleDaysAttached(IScheduleDay object1, IScheduleDay object2)
		{
			return object1.DateOnlyAsPeriod.DateOnly.AddDays(1) == object2.DateOnlyAsPeriod.DateOnly;
		}

	}
}
