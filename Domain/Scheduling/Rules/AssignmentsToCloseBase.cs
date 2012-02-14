using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public abstract class AssignmentsToCloseBase : IBusinessRule, IAssignmentPeriodRule
    {

        private string _errorMessage;

        public string ErrorMessage
        {
            get { return _errorMessage; }
        }

        public virtual bool IsMandatory
        {
            get { return Mandatory(); }
        }

        public bool Validate(IScheduleRange currentSchedules, IScheduleRange previousSchedules, DateOnly dateToCheck)
        {
            IPersonPeriod period = currentSchedules.Person.Period(dateToCheck);
            if (period == null)
            {

                _errorMessage = string.Format(CultureInfo.CurrentCulture,
                                             UserTexts.Resources.BusinessRuleNoContractErrorMessage, currentSchedules.Person.Name,
                                             dateToCheck.Date.ToShortDateString());
                return false;
            }

            //man borde nog jobba mot projektioner här? Isåfall - hämta per dag (ScheduledDay eller ScheduledDayCollection)
            var dtp =
                new DateOnlyPeriod(dateToCheck.AddDays(-1), dateToCheck.AddDays(1)).ToDateTimePeriod(
                    currentSchedules.Person.PermissionInformation.DefaultTimeZone());
            IScheduleDay partPart = currentSchedules.ScheduledPeriod(dtp);

            TimeSpan? nightRest = NightlyRest(currentSchedules.Person, dateToCheck);
            TimeSpan nightRestValue;
            if (nightRest.HasValue)
                nightRestValue = nightRest.Value;
            else
                nightRestValue = TimeSpan.Zero;

            var personAssignments = partPart.PersonAssignmentCollection();
            foreach (IPersonAssignment assignment in personAssignments)
            {
                var idx = personAssignments.IndexOf(assignment);
                if (idx < personAssignments.Count - 1)
                {
                    // not last one
                    IPersonAssignment nextAssignment = personAssignments[idx + 1];
                    // if on the same day it must be on tghe day we check
                    if (assignment.Period.StartDateTime.Date == nextAssignment.Period.StartDateTime.Date)
                    {
                        if (dateToCheck.Date == assignment.Period.StartDateTime.Date)
                            if (toShortNightRest(assignment, nextAssignment, nightRestValue)) return false;
                    }
                    else
                    {
                        if (toShortNightRest(assignment, nextAssignment, nightRestValue)) return false;
                    }

                }
            }
            return true;
        }

        public DateTimePeriod LongestDateTimePeriodForAssignment(IScheduleRange current, DateOnly dateToCheck)
        {
            DateTime approximateTime = new DateTime(dateToCheck.Year, dateToCheck.Month, dateToCheck.Day, 12, 0, 0, DateTimeKind.Unspecified);
            DateTime approxUtc = TimeZoneHelper.ConvertToUtc(approximateTime,
                                                             current.Person.PermissionInformation.DefaultTimeZone());

            IPersonPeriod period = current.Person.Period(dateToCheck);
            if (period == null)
            {

                _errorMessage = string.Format(CultureInfo.CurrentCulture,
                                             UserTexts.Resources.BusinessRuleNoContractErrorMessage, current.Person.Name,
                                             dateToCheck.Date.ToShortDateString());

                return new DateTimePeriod(approxUtc, approxUtc);
            }

            IPersonAssignment assBefore = null;
            IPersonAssignment assAfter = null;
            foreach (IPersonAssignment ass in current.ScheduledPeriod(current.Period).PersonAssignmentCollection())
            {
                if (ass.MainShift != null)
                {
                    if (ass.Period.Contains(approxUtc))
                        return new DateTimePeriod(approxUtc, approxUtc);

                    if (ass.Period.EndDateTime < approxUtc)
                    {
                        assBefore = ass;
                    }
                    if (approxUtc < ass.Period.StartDateTime)
                    {
                        assAfter = ass;
                        break;
                    }
                }
            }
            DateTime earliestStartTime = endTimeOnAssignmentBeforePlusNightRest(current.ScheduledPeriod(current.Period), assBefore, dateToCheck);
            DateTime latestEndTime = startTimeOnAssignmentAfterMinusNightRest(current.ScheduledPeriod(current.Period), assAfter, dateToCheck);

            if (latestEndTime < earliestStartTime)
            {
                latestEndTime = earliestStartTime;
            }
            return new DateTimePeriod(earliestStartTime, latestEndTime);
        }

        private DateTime endTimeOnAssignmentBeforePlusNightRest(IScheduleDay currentCompletePart, IPersonAssignment assBefore, DateOnly dateToCheckOn)
        {
            if (assBefore == null)
            {
                return currentCompletePart.Period.StartDateTime;
            }
            TimeSpan? nightRest = NightlyRest(currentCompletePart.Person, dateToCheckOn);
            if(nightRest.HasValue)
            {
                return assBefore.Period.EndDateTime.Add(nightRest.Value);
            }
            return assBefore.Period.EndDateTime;
        }

        private DateTime startTimeOnAssignmentAfterMinusNightRest(IScheduleDay currentCompletePart, IPersonAssignment assAfter, DateOnly dateToCheckOn )
        {
            if (assAfter == null)
            {
                return currentCompletePart.Period.EndDateTime;
            }

            TimeSpan? nightRest = NightlyRest(currentCompletePart.Person, dateToCheckOn);
            if (nightRest.HasValue)
            {
                return assAfter.Period.StartDateTime.Add(-nightRest.Value);
            }
            return assAfter.Period.StartDateTime;
        }

        private bool toShortNightRest(IPersonAssignment assignment, IPersonAssignment nextAssignment, TimeSpan? nightlyRest)
        {
            if (nextAssignment.Period.StartDateTime - assignment.Period.EndDateTime < nightlyRest)
            {
                var loggedOnCulture =
                    StateHolderReader.Instance.StateReader.SessionScopeData.LoggedOnPerson.PermissionInformation.Culture();
                string start = assignment.Period.LocalEndDateTime.ToString(loggedOnCulture);
                string end = nextAssignment.Period.LocalStartDateTime.ToString(loggedOnCulture);
                string rest = (nextAssignment.Period.StartDateTime - assignment.Period.EndDateTime).TotalHours.ToString(loggedOnCulture);

                if(nightlyRest.HasValue)
                {
                    _errorMessage = string.Format(loggedOnCulture,
                                                 UserTexts.Resources.BusinessRuleNightlyRestRuleErrorMessage,
                                                 nightlyRest.Value.TotalHours, start, end, rest);
                }
                else
                {
                    _errorMessage = UserTexts.Resources.BusinessRuleOverlappingErrorMessage2;
                }
                return true;
            }
            return false;
        }

        protected abstract TimeSpan? NightlyRest(IPerson person, DateOnly dateToCheckOn);

        protected abstract bool Mandatory();

    }
}