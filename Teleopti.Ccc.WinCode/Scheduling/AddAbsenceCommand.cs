using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class AddAbsenceCommand : AddLayerCommand
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public AddAbsenceCommand(ISchedulerStateHolder schedulerStateHolder, IScheduleViewBase scheduleViewBase, SchedulePresenterBase presenter, IList<IScheduleDay> scheduleParts)
			: base(schedulerStateHolder, scheduleViewBase, presenter, scheduleParts ?? scheduleViewBase.CurrentColumnSelectedSchedules())
		{
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

				foreach (var dateTime in absencePeriod.DateCollection())
				{
					var dateOnly = new DateOnly(dateTime);
					var scheduleday = SchedulerStateHolder.Schedules[selectedPerson].ScheduledDay(dateOnly);
					if (scheduleday != null)
						selectedAbsenceDaysPerPerson.Add(scheduleday);
				}

				IList<IScheduleDay> selectedUnlockedAbsenceDaysPerPerson = removeLockedSchedules(selectedAbsenceDaysPerPerson);
				IList<IList<IScheduleDay>> aggregateList = AggregatedAbsenceList(selectedUnlockedAbsenceDaysPerPerson);

				foreach (IList<IScheduleDay> scheduleDays in aggregateList)
				{
					IScheduleDay fistDayInList = scheduleDays[0];
					IScheduleDay lastDayInList = scheduleDays[scheduleDays.Count - 1];

					DateTimePeriod aggregateAbsencePeriod = new DateTimePeriod(fistDayInList.Period.StartDateTime, lastDayInList.Period.EndDateTime);
					DateTimePeriod? currentAbsencePeriod = aggregateAbsencePeriod.Intersection(absencePeriod);
					if (!currentAbsencePeriod.HasValue)
						continue;

					IAbsenceLayer absLayer = new AbsenceLayer(absence, currentAbsencePeriod.Value);
					lastDayInList.CreateAndAddAbsence(absLayer);
					Presenter.ModifySchedulePart(selectedAbsenceDaysPerPerson);
					foreach (IScheduleDay part in selectedAbsenceDaysPerPerson)
						ScheduleViewBase.RefreshRangeForAgentPeriod(part.Person, absencePeriod);
				}
			}
		}

		private IList<IScheduleDay> removeLockedSchedules(IEnumerable<IScheduleDay> selectedSchedules)
		{
			IList<IScheduleDay> schedulesToKeep = new List<IScheduleDay>();
			foreach (IScheduleDay schedulePart in selectedSchedules)
			{
				GridlockDictionary lockDictionary = Presenter.LockManager.Gridlocks(schedulePart.Person,
																					schedulePart.DateOnlyAsPeriod.DateOnly);
				if (lockDictionary == null || lockDictionary.Count == 0)
					schedulesToKeep.Add(schedulePart);
			}
			return schedulesToKeep.OrderBy(p => p.DateOnlyAsPeriod.DateOnly).ToList();
		}

		private IList<IList<IScheduleDay>> AggregatedAbsenceList(IList<IScheduleDay> scheduleDays)
		{
			ListAggregator<IScheduleDay> aggregator = new ListAggregator<IScheduleDay>();
			return aggregator.Aggregate(scheduleDays, AreScheduleDaysAttached);
		}


		private static bool AreScheduleDaysAttached(IScheduleDay object1, IScheduleDay object2)
		{
			if(object2.Period.StartDateTime.Subtract(object1.Period.EndDateTime) < new TimeSpan(1, 0, 0, 0))
				return true;
			return false;
		}

	}
}
