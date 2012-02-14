using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
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
            IList<IScheduleDay> origSchedules = selectedSchedules;
            IList<IScheduleDay> returnParts = new List<IScheduleDay>();
            if (!VerifySelectedSchedule(selectedSchedules)) return;
            var n = from a in SchedulerStateHolder.CommonStateHolder.Absences
                    where !((IDeleteTag) a).IsDeleted
                    select a;
    
            var fallbackDefaultHours =
                new SetupDateTimePeriodDefaultLocalHoursForAbsence(selectedSchedules[0]);
            var periodFromSchedules = new SetupDateTimePeriodToSchedulesIfTheyExist(selectedSchedules, fallbackDefaultHours);
            ISetupDateTimePeriod periodSetup = new SetupDateTimePeriodToDefaultPeriod(DefaultPeriod, periodFromSchedules);

            IAddLayerViewModel<IAbsence> dialog1 = ScheduleViewBase.CreateAddAbsenceViewModel(n.ToList(), periodSetup);
            bool result = dialog1.Result;

            if (!result) return;

            IAbsence absence = dialog1.SelectedItem;
            DateTimePeriod period = dialog1.SelectedPeriod;
            List<IScheduleDay> changedParts = new List<IScheduleDay>();
            IList<IPerson> handledPersons = new List<IPerson>();

            if (selectedSchedules.Count > 1)
            {
                foreach (IScheduleDay part in ScheduleParts)
                {
                    selectedSchedules = removeLockedSchedules(origSchedules);
                	bool containLockedDates = selectedSchedules.Count != origSchedules.Count;

                    IScheduleDay day = part;
                    IScheduleDay tempPart = selectedSchedules.FirstOrDefault(s => s.Person.Equals(day.Person));
                    if (tempPart == null)
                        continue;

                    if (handledPersons.Contains(tempPart.Person))
                        continue;

                    handledPersons.Add(tempPart.Person);

                    IList<IList<IScheduleDay>> combinedParts = getCombinedParts(selectedSchedules, part, tempPart);
                    CreateAbsenceLayers(period, combinedParts, absence, returnParts, containLockedDates);
                    changedParts.AddRange(returnParts);
                }
            }
            else
            {
                var selectedSchedule = selectedSchedules.First();
                if (period.StartDateTime < selectedSchedule.DateOnlyAsPeriod.Period().StartDateTime)
                {
                    period = new DateTimePeriod(selectedSchedule.Period.StartDateTime, period.EndDateTime);
                }
                IAbsenceLayer absLayer = new AbsenceLayer(absence, period);
                selectedSchedule.CreateAndAddAbsence(absLayer);
                returnParts.Add(selectedSchedule);
                changedParts.AddRange(returnParts);
            }
            Presenter.ModifySchedulePart(changedParts);
            foreach (IScheduleDay part in changedParts)
            {
                ScheduleViewBase.RefreshRangeForAgentPeriod(part.Person, period);
            }
        }

        private static void CreateAbsenceLayers(DateTimePeriod selectedPeriod, IEnumerable<IList<IScheduleDay>> combinedParts, IAbsence absence, ICollection<IScheduleDay> returnParts, bool containLockedDates)
        {
        	returnParts.Clear();
            foreach (var list in combinedParts)
            {
                //IAbsenceLayer absLayer = new AbsenceLayer(absence, selectedPeriod);
                foreach (var part in list)
                {
                	var start = selectedPeriod.StartDateTime;
                	var end = selectedPeriod.EndDateTime;

					if (selectedPeriod.StartDateTime < part.Period.StartDateTime && containLockedDates)
						start = part.Period.StartDateTime;

					if (selectedPeriod.EndDateTime > part.Period.EndDateTime && containLockedDates)
						end = part.Period.EndDateTime;

					var period = new DateTimePeriod(start, end);

					IAbsenceLayer absLayer = new AbsenceLayer(absence, period);

                    part.CreateAndAddAbsence(absLayer);
                    returnParts.Add(part);

					if(!containLockedDates)
						break;
                }
            }
        }

        private static IList<IList<IScheduleDay>> getCombinedParts(IEnumerable<IScheduleDay> selectedSchedules, IScheduleDay party, IScheduleDay tempPart)
        {
            IList<IList<IScheduleDay>> combinedParts = new List<IList<IScheduleDay>>();
            IList<IScheduleDay> parts = new List<IScheduleDay>();
            parts.Add(tempPart);
            combinedParts.Add(parts);

            foreach (IScheduleDay schedulePart in selectedSchedules)
            {
                if (!party.Person.Equals(schedulePart.Person))
                    continue;
                if (schedulePart.DateOnlyAsPeriod.Period().StartDateTime.Subtract(tempPart.DateOnlyAsPeriod.Period().StartDateTime) > TimeSpan.FromDays(1))
                {
                    parts = new List<IScheduleDay>();
                    parts.Add(schedulePart);
                    combinedParts.Add(parts);
                    tempPart = schedulePart;
                }
                else
                {
                    if (!tempPart.Equals(schedulePart))
                        parts.Add(schedulePart);
                    tempPart = schedulePart;
                }
            }
            return combinedParts;
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
    }
}
