using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface INewDayOffRule : INewBusinessRule{}

	public class NewDayOffRule : INewDayOffRule
	{
		private readonly IWorkTimeStartEndExtractor _workTimeStartEndExtractor;
		private readonly CultureInfo _loggedOnCulture = Thread.CurrentThread.CurrentCulture;

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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization",
			"CA1303:Do not pass literals as localized parameters",
			MessageId =
				"Teleopti.Ccc.Domain.Scheduling.Rules.NewDayOffRule.createResponse(Teleopti.Interfaces.Domain.IPerson,Teleopti.Interfaces.Domain.DateOnly,System.String)"
			)]
		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones,
			IEnumerable<IScheduleDay> scheduleDays)
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
				var schedules = currentSchedules.ScheduledDayCollection(period).Select(convert).ToArray();

				foreach (var checkDay in schedules)
				{
					oldResponses.Remove(createResponse(scheduleDay.Key, checkDay.Date, "remove"));
					var dayOff = checkDay.DayOff;
					if (dayOff != null)
					{
						DateTimePeriod layerAfterPeriod = periodOfLayerAfter(dayOff, schedules);
						DateTimePeriod layerBeforePeriod = periodOfLayerBefore(dayOff, schedules);

						if (DayOffDoesConflictWithActivity(dayOff, layerBeforePeriod, layerAfterPeriod))
						{
							IBusinessRuleResponse response = createResponse(scheduleDay.Key, checkDay.Date, ErrorMessage);
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

		private dayForValidation convert(IScheduleDay scheduleDay)
		{
			var dayForValidation = new dayForValidation {Date = scheduleDay.DateOnlyAsPeriod.DateOnly};
			var assignment = scheduleDay.PersonAssignment();
			if (assignment != null)
			{
				var projection = new Lazy<IVisualLayerCollection>(assignment.ProjectionService().CreateProjection);
				dayForValidation.ProjectionStart =
					new Lazy<DateTime?>(() => _workTimeStartEndExtractor.WorkTimeStart(projection.Value));
				dayForValidation.ProjectionEnd = new Lazy<DateTime?>(() => _workTimeStartEndExtractor.WorkTimeEnd(projection.Value));
				dayForValidation.HasAssignment = true;
				dayForValidation.StartDateTime = assignment.Period.StartDateTime;
				dayForValidation.DayOff = assignment.DayOff();
			}
			return dayForValidation;
		}

		private class dayForValidation
		{
			public DateTime StartDateTime { get; set; }
			public DateOnly Date { get; set; }
			public bool HasAssignment { get; set; }
			public Lazy<DateTime?> ProjectionStart { get; set; }
			public Lazy<DateTime?> ProjectionEnd { get; set; }
			public IDayOff DayOff { get; set; }
		}

		public bool DayOffDoesConflictWithActivity(IDayOff dayOff, DateTimePeriod assignmentBeforePeriod,
			DateTimePeriod assignmentAfterPeriod)
		{
			if (dayOff.TargetLength > (assignmentAfterPeriod.StartDateTime - assignmentBeforePeriod.EndDateTime))
			{
				// we can't fit the day off beween the days
				ErrorMessage = string.Format(_loggedOnCulture, UserTexts.Resources.BusinessRuleDayOffErrorMessage1 + ErrorMessage,
					dayOff.Anchor);
				return true;
			}
			if (DayOffConflictWithAssignmentBefore(dayOff, assignmentBeforePeriod) ||
				DayOffConflictWithAssignmentAfter(dayOff, assignmentAfterPeriod))
			{
				if (dayOffCannotBeMoved(dayOff, assignmentBeforePeriod, assignmentAfterPeriod))
				{
					return true;
				}
			}

			return false;
		}

		private bool dayOffCannotBeMoved(IDayOff dayOff, DateTimePeriod assignmentBeforePeriod,
			DateTimePeriod assignmentAfterPeriod)
		{
			DateTime startDayOff = dayOffStartEnd(dayOff).StartDateTime;
			DateTime endDayOff = dayOffStartEnd(dayOff).EndDateTime;

			if (DayOffConflictWithAssignmentBefore(dayOff, assignmentBeforePeriod))
			{
				string day = dayOff.Anchor.Date.ToString("d", _loggedOnCulture);
				// find out how long the conflict is
				TimeSpan conflictTime = assignmentBeforePeriod.EndDateTime - startDayOff;
				// flexibility does not allow the move
				if (dayOff.Boundary.EndDateTime < endDayOff.Add(conflictTime))
				{

					ErrorMessage = string.Format(_loggedOnCulture, UserTexts.Resources.BusinessRuleDayOffErrorMessage2 + ErrorMessage,
						day);
					return true;
				}
				// if the flexibility allows it and we don't get a conflict at the end it can be moved
				return false;
			}

			else
			{
				string day = dayOff.Anchor.Date.ToString("d", _loggedOnCulture);
				// find out how long the conflict is
				TimeSpan conflictTime = assignmentAfterPeriod.StartDateTime - endDayOff;
				// flexibility does not allow the move
				if (startDayOff.Add(conflictTime) < dayOff.Boundary.StartDateTime)
				{
					ErrorMessage = string.Format(_loggedOnCulture, UserTexts.Resources.BusinessRuleDayOffErrorMessage4 + ErrorMessage,
						day);
					return true;
				}
				// if the flexibility allows it and we don't get a conflict at the end it cann be moved
				return false;
			}
		}

		public static bool DayOffConflictWithAssignmentBefore(IDayOff dayOff, DateTimePeriod periodOfAssignment)
		{
			// if the assignment is on the same day then it's not a conflict
			if (periodOfAssignment.StartDateTime.Date == dayOff.Anchor.Date)
			{
				return false;
			}
			// if the StartDateTimeOfAssignment is too late just return false   
			if (periodOfAssignment.StartDateTime > dayOff.Anchor)
			{
				return false;
			}
			return periodOfAssignment.EndDateTime > dayOffStartEnd(dayOff).StartDateTime;
		}

		// checkes if the activity starts before the end of the day off, does not use the flexibility (comes later)
		public static bool DayOffConflictWithAssignmentAfter(IDayOff dayOff, DateTimePeriod periodOfAssignment)
		{
			// if the assignment is on the same day then it's not a conflict
			if (periodOfAssignment.StartDateTime.Date == dayOff.Anchor.Date)
			{
				return false;
			}
			// if the StartDateTimeOfAssignment is too early just return false
			if (periodOfAssignment.EndDateTime < dayOff.Anchor)
			{
				return false;
			}
			return periodOfAssignment.StartDateTime < dayOffStartEnd(dayOff).EndDateTime;
		}

		private static DateTimePeriod dayOffStartEnd(IDayOff dayOff)
		{
			DateTime startDayOff = dayOff.Anchor.AddMinutes(-(dayOff.TargetLength.TotalMinutes/2));
			DateTime endDayOff = dayOff.Anchor.AddMinutes((dayOff.TargetLength.TotalMinutes/2));

			return new DateTimePeriod(startDayOff, endDayOff);
		}

		private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message)
		{
			var dop = new DateOnlyPeriod(dateOnly, dateOnly);
			DateTimePeriod period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			IBusinessRuleResponse response = new BusinessRuleResponse(typeof (NewDayOffRule), message, _haltModify, IsMandatory,
				period, person, dateOnlyPeriod) {Overridden = !_haltModify};
			return response;
		}

		private DateTimePeriod periodOfLayerBefore(IDayOff personDayOff, dayForValidation[] daysToCheck)
		{
			var assignmentJustBeforeDayOff = getAssignmentJustBeforeDayOff(personDayOff, daysToCheck);
			var layerBeforePeriod = new DateTimePeriod(1900, 1, 1, 1900, 1, 2);

			if (assignmentJustBeforeDayOff != null)
			{
				if (assignmentJustBeforeDayOff.ProjectionStart.Value.HasValue &&
					assignmentJustBeforeDayOff.ProjectionEnd.Value.HasValue)
					layerBeforePeriod = new DateTimePeriod(assignmentJustBeforeDayOff.ProjectionStart.Value.Value,
						assignmentJustBeforeDayOff.ProjectionEnd.Value.Value);
			}
			return layerBeforePeriod;
		}

		private static dayForValidation getAssignmentJustBeforeDayOff(IDayOff dayOff, dayForValidation[] daysToCheck)
		{
			dayForValidation returnVal = null;
			foreach (var day in daysToCheck)
			{
				if (day.HasAssignment)
				{
					if (day.StartDateTime < dayOff.Anchor && (returnVal==null || returnVal.StartDateTime<day.StartDateTime))
					{
						returnVal = day;
					}
					else
					{
						break;
					}
				}
			}

			return returnVal;
		}

		private DateTimePeriod periodOfLayerAfter(IDayOff personDayOff, dayForValidation[] daysToCheck)
		{
			var assignmentJustAfterDayOff = getAssignmentJustAfterDayOff(personDayOff, daysToCheck);
			var periodOfAssignmentAfter = new DateTimePeriod(2100, 1, 1, 2100, 1, 2);
			if (assignmentJustAfterDayOff != null)
			{
				if (assignmentJustAfterDayOff.ProjectionStart.Value.HasValue &&
					assignmentJustAfterDayOff.ProjectionEnd.Value.HasValue)
					periodOfAssignmentAfter = new DateTimePeriod(assignmentJustAfterDayOff.ProjectionStart.Value.Value,
						assignmentJustAfterDayOff.ProjectionEnd.Value.Value);
			}
			return periodOfAssignmentAfter;
		}

		private static dayForValidation getAssignmentJustAfterDayOff(IDayOff dayOff, dayForValidation[] daysToCheck)
		{
			foreach (var day in daysToCheck)
			{
				if (day.HasAssignment)
				{
					if (day.StartDateTime > dayOff.Anchor)
						return day;
				}
			}

			return null;
		}
	}
}
