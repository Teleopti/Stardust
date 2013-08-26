﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface INewDayOffRule : INewBusinessRule{}
    public class NewDayOffRule : INewDayOffRule
    {
    	private readonly IWorkTimeStartEndExtractor _workTimeStartEndExtractor;
    	private readonly CultureInfo _loggedOnCulture = TeleoptiPrincipal.Current.Regional.Culture;

        private bool _haltModify = true;
        private string _errorMessage = "";

		public NewDayOffRule(IWorkTimeStartEndExtractor workTimeStartEndExtractor)
		{
			_workTimeStartEndExtractor = workTimeStartEndExtractor;
		}

    	public string ErrorMessage
        {
            get { return _errorMessage; }
            private set { _errorMessage = value; }
        }

        public bool IsMandatory
        {
            get { return false; }
        }

        public bool HaltModify
        {
            get { return _haltModify; }
            set { _haltModify = value; }
        }

        public bool ForDelete { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Scheduling.Rules.NewDayOffRule.createResponse(Teleopti.Interfaces.Domain.IPerson,Teleopti.Interfaces.Domain.DateOnly,System.String)")]
		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
        {
            var responseList = new HashSet<IBusinessRuleResponse>();

			var groupedByPerson = scheduleDays.GroupBy(s => s.Person);

			foreach (var scheduleDay in groupedByPerson)
			{
				if (!scheduleDay.Any()) continue;
				var period = new DateOnlyPeriod(scheduleDay.Min(s => s.DateOnlyAsPeriod.DateOnly).AddDays(-3),
				                                scheduleDay.Max(s => s.DateOnlyAsPeriod.DateOnly).AddDays(3));
				var currentSchedules = rangeClones[scheduleDay.Key];
				var oldResponses = currentSchedules.BusinessRuleResponseInternalCollection;
				var schedules = currentSchedules.ScheduledDayCollection(period);

				foreach (var checkDay in schedules)
				{
					var checkDateOnly = checkDay.DateOnlyAsPeriod.DateOnly;
					oldResponses.Remove(createResponse(scheduleDay.Key, checkDateOnly, "remove"));
					foreach (IPersonDayOff personDayOff in checkDay.PersonDayOffCollection())
					{
						DateTimePeriod layerAfterPeriod = periodOfLayerAfter(personDayOff, schedules);
						DateTimePeriod layerBeforePeriod = periodOfLayerBefore(personDayOff, schedules);

						if (DayOffDoesConflictWithActivity(personDayOff, layerBeforePeriod, layerAfterPeriod))
						{
							IBusinessRuleResponse response = createResponse(scheduleDay.Key, checkDateOnly, ErrorMessage);
							ErrorMessage = "";
							if (!ForDelete)
								responseList.Add(response);
							oldResponses.Add(response);
						}
					}
				}
			}

			return responseList;
        }

        public bool DayOffDoesConflictWithActivity(IPersonDayOff dayOff, DateTimePeriod assignmentBeforePeriod, DateTimePeriod assignmentAfterPeriod)
        {
            if(dayOff.DayOff.TargetLength > (assignmentAfterPeriod.StartDateTime - assignmentBeforePeriod.EndDateTime))
            {
                // we can't fit the day off beween the days
                ErrorMessage = string.Format(_loggedOnCulture, UserTexts.Resources.BusinessRuleDayOffErrorMessage1 + ErrorMessage, dayOff.DayOff.Anchor);
                return true;
            }
            // if both are true the day off cannot be moved
            //if (DayOffConflictWithAssignmentBefore(dayOff, assignmentBeforePeriod) && DayOffConflictWithAssignmentAfter(dayOff, assignmentAfterPeriod))
            //{
            //    ErrorMessage = string.Format(_loggedOnCulture, UserTexts.Resources.BusinessRuleDayOffErrorMessage1 + ErrorMessage, dayOff.DayOff.Anchor);
            //    return true;
            //}
            if (DayOffConflictWithAssignmentBefore(dayOff, assignmentBeforePeriod) || DayOffConflictWithAssignmentAfter(dayOff, assignmentAfterPeriod))
            {
                if (dayOffCannotBeMoved(dayOff, assignmentBeforePeriod, assignmentAfterPeriod))
                {
                    return true;
                }
            }

            return false;
        }

        private bool dayOffCannotBeMoved(IPersonDayOff dayOff, DateTimePeriod assignmentBeforePeriod, DateTimePeriod assignmentAfterPeriod)
        {
            DateTime startDayOff = dayOffStartEnd(dayOff).StartDateTime;
            DateTime endDayOff = dayOffStartEnd(dayOff).EndDateTime;

            if (DayOffConflictWithAssignmentBefore(dayOff, assignmentBeforePeriod))
            {
                string day = dayOff.DayOff.Anchor.Date.ToString("d", _loggedOnCulture);
                // find out how long the conflict is
                TimeSpan conflictTime = assignmentBeforePeriod.EndDateTime - startDayOff;
                // flexibility does not allow the move
                if (dayOff.DayOff.Boundary.EndDateTime < endDayOff.Add(conflictTime))
                {

                    ErrorMessage = string.Format(_loggedOnCulture, UserTexts.Resources.BusinessRuleDayOffErrorMessage2 + ErrorMessage, day);
                    return true;
                }
                // the layer After conflicts with a move
                //if (endDayOff.Add(conflictTime) > assignmentAfterPeriod.StartDateTime)
                //{
                //    ErrorMessage = string.Format(_loggedOnCulture, UserTexts.Resources.BusinessRuleDayOffErrorMessage3 + ErrorMessage, day);
                //    return true;
                //}
                // if the flexibility allows it and we don't get a conflict at the end it can be moved
                return false;
            }

            else
            {
                string day = dayOff.DayOff.Anchor.Date.ToString("d", _loggedOnCulture);
                // find out how long the conflict is
                TimeSpan conflictTime = assignmentAfterPeriod.StartDateTime - endDayOff;
                // flexibility does not allow the move
                if (startDayOff.Add(conflictTime) < dayOff.DayOff.Boundary.StartDateTime)
                {
                    ErrorMessage = string.Format(_loggedOnCulture, UserTexts.Resources.BusinessRuleDayOffErrorMessage4 + ErrorMessage, day);
                    return true;
                }
                // the layer Before conflicts with a move
                //if (startDayOff.Add(conflictTime) < assignmentBeforePeriod.EndDateTime)
                //{
                //    ErrorMessage = string.Format(_loggedOnCulture, UserTexts.Resources.BusinessRuleDayOffErrorMessage5 + ErrorMessage, day);
                //    return true;
                //}
                // if the flexibility allows it and we don't get a conflict at the end it cann be moved
                return false;
            }
        }

        public static bool DayOffConflictWithAssignmentBefore(IPersonDayOff dayOff, DateTimePeriod periodOfAssignment)
        {
            // if the assignment is on the same day then it's not a conflict
            if (periodOfAssignment.StartDateTime.Date == dayOff.DayOff.Anchor.Date)
            {
                return false;
            }
            // if the StartDateTimeOfAssignment is too late just return false   
            if (periodOfAssignment.StartDateTime > dayOff.DayOff.Anchor)
            {
                return false;
            }
            return periodOfAssignment.EndDateTime > dayOffStartEnd(dayOff).StartDateTime;
        }

        // checkes if the activity starts before the end of the day off, does not use the flexibility (comes later)
        public static bool DayOffConflictWithAssignmentAfter(IPersonDayOff dayOff, DateTimePeriod periodOfAssignment)
        {
            // if the assignment is on the same day then it's not a conflict
            if (periodOfAssignment.StartDateTime.Date == dayOff.DayOff.Anchor.Date)
            {
                return false;
            }
            // if the StartDateTimeOfAssignment is too early just return false
            if (periodOfAssignment.EndDateTime < dayOff.DayOff.Anchor)
            {
                return false;
            }
            return periodOfAssignment.StartDateTime < dayOffStartEnd(dayOff).EndDateTime;
        }

        private static DateTimePeriod dayOffStartEnd(IPersonDayOff dayOff)
        {
            DateTime startDayOff = dayOff.DayOff.Anchor.AddMinutes(-(dayOff.DayOff.TargetLength.TotalMinutes / 2));
            DateTime endDayOff = dayOff.DayOff.Anchor.AddMinutes((dayOff.DayOff.TargetLength.TotalMinutes / 2));

            return new DateTimePeriod(startDayOff, endDayOff);
        }

        private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message)
        {
            var dop = new DateOnlyPeriod(dateOnly, dateOnly);
            DateTimePeriod period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
            var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
            IBusinessRuleResponse response = new BusinessRuleResponse(typeof(NewDayOffRule), message, _haltModify, IsMandatory, period, person, dateOnlyPeriod) { Overridden = !_haltModify };
            return response;
        }

        private DateTimePeriod periodOfLayerBefore(IPersonDayOff personDayOff, IEnumerable<IScheduleDay> daysToCheck)
        {
            var assignmentJustBeforeDayOff = getAssignmentJustBeforeDayOff(personDayOff, daysToCheck);
            var layerBeforePeriod = new DateTimePeriod(1900, 1, 1, 1900, 1, 2);

            if (assignmentJustBeforeDayOff != null)
            {
				var proj = assignmentJustBeforeDayOff.ProjectionService().CreateProjection();
				var start = _workTimeStartEndExtractor.WorkTimeStart(proj);
				var end = _workTimeStartEndExtractor.WorkTimeEnd(proj);
				if (start.HasValue && end.HasValue)
					layerBeforePeriod = new DateTimePeriod(start.Value, end.Value);
                
				//layerBeforePeriod = assignmentJustBeforeDayOff.Period;
            }
            return layerBeforePeriod;
        }

        private static IPersonAssignment getAssignmentJustBeforeDayOff(IPersonDayOff dayOff, IEnumerable<IScheduleDay> daysToCheck)
        {
            IPersonAssignment returnVal = null;
            foreach (IScheduleDay day in daysToCheck)
            {
                foreach (var assignment in day.PersonAssignmentCollectionDoNotUse())
                {
                    if (assignment.Period.StartDateTime < dayOff.DayOff.Anchor)
                    {
                        returnVal = assignment;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return returnVal;
        }

        private  DateTimePeriod periodOfLayerAfter(IPersonDayOff personDayOff, IEnumerable<IScheduleDay> daysToCheck)
        {
            var assignmentJustAfterDayOff = getAssignmentJustAfterDayOff(personDayOff, daysToCheck);
            var periodOfAssignmentAfter = new DateTimePeriod(2100, 1, 1, 2100, 1, 2);
            if (assignmentJustAfterDayOff != null)
            {
            	var proj =  assignmentJustAfterDayOff.ProjectionService().CreateProjection();
            	var start = _workTimeStartEndExtractor.WorkTimeStart(proj);
            	var end = _workTimeStartEndExtractor.WorkTimeEnd(proj);
				if(start.HasValue && end.HasValue)
					periodOfAssignmentAfter = new DateTimePeriod(start.Value, end.Value);
            }
            return periodOfAssignmentAfter;
        }

        private static IPersonAssignment getAssignmentJustAfterDayOff(IPersonDayOff dayOff, IEnumerable<IScheduleDay> daysToCheck)
        {
            foreach (IScheduleDay day in daysToCheck)
            {
                foreach (var assignment in day.PersonAssignmentCollectionDoNotUse())
                {
                    if (assignment.Period.StartDateTime > dayOff.DayOff.Anchor)
                        return assignment;
                }
            }

            return null;
        }
    }
}
