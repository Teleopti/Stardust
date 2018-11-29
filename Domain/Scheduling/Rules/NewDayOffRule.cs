using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface INewDayOffRule : INewBusinessRule{}

	public class NewDayOffRule : INewDayOffRule
	{
		private readonly IWorkTimeStartEndExtractor _workTimeStartEndExtractor;
		private readonly CultureInfo _loggedOnCulture = Thread.CurrentThread.CurrentUICulture;

		public NewDayOffRule(IWorkTimeStartEndExtractor workTimeStartEndExtractor)
		{
			_workTimeStartEndExtractor = workTimeStartEndExtractor;
		}

		public string ErrorMessage { get; private set; } = "";

		public bool IsMandatory => false;

		public bool HaltModify { get; set; } = true;

		public bool Configurable => true;

		public bool ForDelete { get; set; }

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
					if (dayOff == null) continue;

					var layerAfterPeriod = periodOfLayerAfter(dayOff, schedules);
					var layerBeforePeriod = periodOfLayerBefore(dayOff, schedules);

					if (!DayOffDoesConflictWithActivity(dayOff, layerBeforePeriod, layerAfterPeriod)) continue;
					var response = createResponse(scheduleDay.Key, checkDay.Date, ErrorMessage);
					ErrorMessage = "";
					if (!ForDelete)
						responseList.Add(response);
					oldResponses.Add(response);
				}
			}

			return responseList;
		}

		public string Description => Resources.DescriptionOfNewDayOffRule;

		private dayForValidation convert(IScheduleDay scheduleDay)
		{
			var assignment = scheduleDay.PersonAssignment();
			if (assignment == null) return new dayForValidation
			{
				Date = scheduleDay.DateOnlyAsPeriod.DateOnly
			};

			var projection = new Lazy<IVisualLayerCollection>(assignment.ProjectionService().CreateProjection);
			return new dayForValidation
			{
				Date = scheduleDay.DateOnlyAsPeriod.DateOnly,
				ProjectionStart = new Lazy<DateTime?>(() => _workTimeStartEndExtractor.WorkTimeStart(projection.Value)),
				ProjectionEnd = new Lazy<DateTime?>(() => _workTimeStartEndExtractor.WorkTimeEnd(projection.Value)),
				HasAssignment = true,
				StartDateTime = assignment.Period.StartDateTime,
				DayOff = assignment.DayOff()
			};
		}

		private class dayForValidation
		{
			public DateTime StartDateTime { get; set; }
			public DateOnly Date { get; set; }
			public bool HasAssignment { get; set; }
			public Lazy<DateTime?> ProjectionStart { get; set; }
			public Lazy<DateTime?> ProjectionEnd { get; set; }
			public DayOff DayOff { get; set; }
		}

		public bool DayOffDoesConflictWithActivity(DayOff dayOff, DateTimePeriod assignmentBeforePeriod,
			DateTimePeriod assignmentAfterPeriod)
		{
			var dayOffErrorMessage1 = Resources.BusinessRuleDayOffErrorMessage1;
			if (dayOff.TargetLength > (assignmentAfterPeriod.StartDateTime - assignmentBeforePeriod.EndDateTime))
			{
				// we can't fit the day off beween the days
				ErrorMessage = string.Format(_loggedOnCulture, dayOffErrorMessage1 + ErrorMessage,
					dayOff.Anchor);
				return true;
			}

			if (!DayOffConflictWithAssignmentBefore(dayOff, assignmentBeforePeriod) &&
				!DayOffConflictWithAssignmentAfter(dayOff, assignmentAfterPeriod)) return false;

			return dayOffCannotBeMoved(dayOff, assignmentBeforePeriod, assignmentAfterPeriod);
		}

		private bool dayOffCannotBeMoved(DayOff dayOff, DateTimePeriod assignmentBeforePeriod,
			DateTimePeriod assignmentAfterPeriod)
		{
			var dateTimePeriod = dayOffStartEnd(dayOff);
			var startDayOff = dateTimePeriod.StartDateTime;
			var endDayOff = dateTimePeriod.EndDateTime;

			if (DayOffConflictWithAssignmentBefore(dayOff, assignmentBeforePeriod))
			{
				var day = dayOff.Anchor.Date.ToString("d", _loggedOnCulture);
				// find out how long the conflict is
				var conflictTime = assignmentBeforePeriod.EndDateTime - startDayOff;
				// flexibility does not allow the move
				if (dayOff.Boundary.EndDateTime >= endDayOff.Add(conflictTime)) return false;

				var dayOffErrorMessage2 = Resources.BusinessRuleDayOffErrorMessage2;
				ErrorMessage = string.Format(_loggedOnCulture, dayOffErrorMessage2 + ErrorMessage, day);
				return true;
				// if the flexibility allows it and we don't get a conflict at the end it can be moved
			}
			else
			{
				var day = dayOff.Anchor.Date.ToString("d", _loggedOnCulture);
				// find out how long the conflict is
				var conflictTime = assignmentAfterPeriod.StartDateTime - endDayOff;
				// flexibility does not allow the move
				if (startDayOff.Add(conflictTime) >= dayOff.Boundary.StartDateTime) return false;

				var dayOffErrorMessage4 = Resources.BusinessRuleDayOffErrorMessage4;
				ErrorMessage = string.Format(_loggedOnCulture, dayOffErrorMessage4 + ErrorMessage, day);
				return true;
				// if the flexibility allows it and we don't get a conflict at the end it cann be moved
			}
		}

		public static bool DayOffConflictWithAssignmentBefore(DayOff dayOff, DateTimePeriod periodOfAssignment)
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
		public static bool DayOffConflictWithAssignmentAfter(DayOff dayOff, DateTimePeriod periodOfAssignment)
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

		private static DateTimePeriod dayOffStartEnd(DayOff dayOff)
		{
			var startDayOff = dayOff.Anchor.AddMinutes(-(dayOff.TargetLength.TotalMinutes/2));
			var endDayOff = dayOff.Anchor.AddMinutes((dayOff.TargetLength.TotalMinutes/2));

			return new DateTimePeriod(startDayOff, endDayOff);
		}

		private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message)
		{
			var dop = dateOnly.ToDateOnlyPeriod();
			var period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var friendlyName = Resources.BusinessRuleDayOffFriendlyName;
			IBusinessRuleResponse response = new BusinessRuleResponse(typeof (NewDayOffRule), message, HaltModify, IsMandatory,
				period, person, dateOnlyPeriod, friendlyName) {Overridden = !HaltModify};
			return response;
		}

		private DateTimePeriod periodOfLayerBefore(DayOff personDayOff, dayForValidation[] daysToCheck)
		{
			var layerBeforePeriod = new DateTimePeriod(1900, 1, 1, 1900, 1, 2);

			var assignmentJustBeforeDayOff = getAssignmentJustBeforeDayOff(personDayOff, daysToCheck);
			if (assignmentJustBeforeDayOff == null) return layerBeforePeriod;

			if (assignmentJustBeforeDayOff.ProjectionStart.Value.HasValue &&
				assignmentJustBeforeDayOff.ProjectionEnd.Value.HasValue)
			{
				layerBeforePeriod = new DateTimePeriod(assignmentJustBeforeDayOff.ProjectionStart.Value.Value,
					assignmentJustBeforeDayOff.ProjectionEnd.Value.Value);
			}

			return layerBeforePeriod;
		}

		private static dayForValidation getAssignmentJustBeforeDayOff(DayOff dayOff, dayForValidation[] daysToCheck)
		{
			dayForValidation returnVal = null;
			foreach (var day in daysToCheck)
			{
				if (!day.HasAssignment) continue;

				if(day.Date.Date >= dayOff.Anchor.Date)
					continue;

				if (day.StartDateTime < dayOff.Anchor && (returnVal==null || returnVal.StartDateTime<day.StartDateTime))
				{
					returnVal = day;
				}
				else
				{
					break;
				}
			}

			return returnVal;
		}

		private DateTimePeriod periodOfLayerAfter(DayOff personDayOff, dayForValidation[] daysToCheck)
		{
			var periodOfAssignmentAfter = new DateTimePeriod(2100, 1, 1, 2100, 1, 2);

			var assignmentJustAfterDayOff = getAssignmentJustAfterDayOff(personDayOff, daysToCheck);
			if (assignmentJustAfterDayOff == null) return periodOfAssignmentAfter;

			if (assignmentJustAfterDayOff.ProjectionStart.Value.HasValue &&
				assignmentJustAfterDayOff.ProjectionEnd.Value.HasValue)
				periodOfAssignmentAfter = new DateTimePeriod(assignmentJustAfterDayOff.ProjectionStart.Value.Value,
					assignmentJustAfterDayOff.ProjectionEnd.Value.Value);
			return periodOfAssignmentAfter;
		}

		private static dayForValidation getAssignmentJustAfterDayOff(DayOff dayOff, dayForValidation[] daysToCheck)
		{
			return daysToCheck.Where(day => day.HasAssignment)
				.Where(day => day.Date.Date > dayOff.Anchor.Date)
				.FirstOrDefault(day => day.StartDateTime > dayOff.Anchor);
		}
	}
}