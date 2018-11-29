using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class NewNightlyRestRule : INewBusinessRule
	{
		private static string friendlyName => Resources.BusinessRuleNightlyRestRuleFriendlyName;
		private readonly IWorkTimeStartEndExtractor _workTimeStartEndExtractor;

		public NewNightlyRestRule(IWorkTimeStartEndExtractor workTimeStartEndExtractor)
		{
			_workTimeStartEndExtractor = workTimeStartEndExtractor;
		}

		public bool IsMandatory => false;

		public bool HaltModify { get; set; } = true;

		public bool Configurable => true;

		public bool ForDelete { get; set; }

		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones,
			IEnumerable<IScheduleDay> scheduleDays)
		{
			var responsList = new List<IBusinessRuleResponse>();
			var groupedByPerson = scheduleDays.GroupBy(s => s.Person);

			foreach (var scheduleDay in groupedByPerson)
			{
				if (!scheduleDay.Any()) continue;
				var period = new DateOnlyPeriod(scheduleDay.Min(s => s.DateOnlyAsPeriod.DateOnly).AddDays(-1),
					scheduleDay.Max(s => s.DateOnlyAsPeriod.DateOnly).AddDays(1));
				var schedules =
					rangeClones[scheduleDay.Key].ScheduledDayCollection(period)
						.ToDictionary(k => k.DateOnlyAsPeriod.DateOnly, convert);
				responsList.AddRange(checkDay(rangeClones[scheduleDay.Key], period, schedules));
			}

			return responsList;
		}

		public string Description => Resources.DescriptionOfNewNightlyRestRule;

		public void ClearMyResponses(IList<IBusinessRuleResponse> responseList, IPerson person, DateOnly dateOnly)
		{
			var dayBefore = dateOnly.AddDays(-1);
			var dayAfter = dateOnly.AddDays(1);
			responseList.Remove(createResponseForRemove(person, dayBefore, new DateOnlyPeriod(dayBefore, dateOnly)));
			responseList.Remove(createResponseForRemove(person, dateOnly, new DateOnlyPeriod(dayBefore, dateOnly)));
			responseList.Remove(createResponseForRemove(person, dateOnly, new DateOnlyPeriod(dateOnly, dayAfter)));
			responseList.Remove(createResponseForRemove(person, dayAfter, new DateOnlyPeriod(dateOnly, dayAfter)));
		}

		public static void AddMyResponse(IList<IBusinessRuleResponse> responseList, IBusinessRuleResponse response)
		{
			responseList.Add(response);
		}

		private IEnumerable<IBusinessRuleResponse> checkDay(IScheduleRange range, DateOnlyPeriod period,
			IDictionary<DateOnly, scheduleDayForValidation> scheduleDay)
		{
			ICollection<IBusinessRuleResponse> responseList = new List<IBusinessRuleResponse>();
			var dayCollection = period.DayCollection();
			for (var dayIndex = 1; dayIndex < dayCollection.Count - 1; dayIndex++)
			{
				var dateToCheck = dayCollection[dayIndex];
				var personPeriod = range.Person.Period(dateToCheck);
				if (personPeriod == null)
					return responseList;

				var yesterday = scheduleDay[dateToCheck.AddDays(-1)];
				var today = scheduleDay[dateToCheck];
				var tomorrow = scheduleDay[dateToCheck.AddDays(1)];
				var nightRest = personPeriod.PersonContract.Contract.WorkTimeDirective.NightlyRest;

				ClearMyResponses(range.BusinessRuleResponseInternalCollection, range.Person, dateToCheck);
				var currentRest = checkDays(yesterday, today, nightRest);
				if (currentRest != null)
				{
					responseList.Add(createResponse(range.Person, yesterday.Date,
						yesterday.Date, today.Date,
						nightRest, currentRest.Value, range, true));
					responseList.Add(createResponse(range.Person, today.Date,
						yesterday.Date, today.Date,
						nightRest, currentRest.Value, range, !HaltModify));
				}

				currentRest = checkDays(today, tomorrow, nightRest);
				if (currentRest != null)
				{
					responseList.Add(createResponse(range.Person, today.Date,
						today.Date, tomorrow.Date,
						nightRest, currentRest.Value, range, !HaltModify));
					responseList.Add(createResponse(range.Person, tomorrow.Date,
						today.Date, tomorrow.Date,
						nightRest, currentRest.Value, range, true));
				}
			}
			return responseList;
		}

		private class scheduleDayForValidation
		{
			public SchedulePartView SignificantPart { get; set; }
			public DateOnly Date { get; set; }
			public DateTime? WorkTimeStart { get; set; }
			public DateTime? WorkTimeEnd { get; set; }
			public IPersonAssignment PersonAssignMent { get; set; }
		}

		private scheduleDayForValidation convert(IScheduleDay scheduleDay)
		{
			var projection = scheduleDay.ProjectionService().CreateProjection();
			return new scheduleDayForValidation
			{
				Date = scheduleDay.DateOnlyAsPeriod.DateOnly,
				SignificantPart = scheduleDay.SignificantPart(),
				WorkTimeStart = _workTimeStartEndExtractor.WorkTimeStart(projection),
				WorkTimeEnd = _workTimeStartEndExtractor.WorkTimeEnd(projection),
				PersonAssignMent= scheduleDay.PersonAssignment()
			};
		}

		private TimeSpan? checkDays(scheduleDayForValidation firstDay, scheduleDayForValidation secondDay, TimeSpan nightRest)
		{
			var firstSignificant = firstDay.SignificantPart;
			var secondSignificant = secondDay.SignificantPart;

			var checkFirstDay = firstSignificant == SchedulePartView.MainShift ||
								firstSignificant == SchedulePartView.Overtime ||overtimeExistsInDayOff(firstDay, firstSignificant);
			var checkSecondDay = secondSignificant == SchedulePartView.MainShift ||
								 secondSignificant == SchedulePartView.Overtime || overtimeExistsInDayOff(secondDay, secondSignificant);

			if (!(checkFirstDay && checkSecondDay))
				return null;

			if (!secondDay.WorkTimeStart.HasValue || !firstDay.WorkTimeEnd.HasValue) return null;

			var restTime = secondDay.WorkTimeStart.Value.Subtract(firstDay.WorkTimeEnd.Value);
			return restTime < nightRest ? (TimeSpan?) restTime : null;
		}

		private static bool overtimeExistsInDayOff(scheduleDayForValidation scheduleDay, SchedulePartView significantOfScheduleDate)
		{
			return significantOfScheduleDate == SchedulePartView.DayOff && scheduleDay.PersonAssignMent.ShiftLayers.OfType<OvertimeShiftLayer>().Any();
		}

		private IBusinessRuleResponse createResponseForRemove(IPerson person, DateOnly dateOnly, DateOnlyPeriod dateOnlyPeriod)
		{
			var dop = dateOnly.ToDateOnlyPeriod();
			var period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());

			return new BusinessRuleResponse(typeof(NewNightlyRestRule), "", HaltModify, IsMandatory, period, person,
				dateOnlyPeriod, friendlyName);
		}

		private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, DateOnly dateOnly1, DateOnly dateOnly2,
			TimeSpan nightRest, TimeSpan currentRest, ISchedule addToRange, bool overridden)
		{
			var dop = dateOnly.ToDateOnlyPeriod();
			var period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var errorMessage = createErrorMessage(dateOnly1, dateOnly2, nightRest, currentRest);
			var response = new BusinessRuleResponse(typeof(NewNightlyRestRule), errorMessage, HaltModify, IsMandatory, period,
					person, new DateOnlyPeriod(dateOnly1, dateOnly2), friendlyName)
				{Overridden = overridden};
			AddMyResponse(addToRange.BusinessRuleResponseInternalCollection, response);
			return response;
		}

		private string createErrorMessage(DateOnly dateOnly1, DateOnly dateOnly2, TimeSpan nightRest, TimeSpan currentRest)
		{
			var errorMessage = Resources.BusinessRuleNightlyRestRuleErrorMessage;
			var currentUiCulture = Thread.CurrentThread.CurrentUICulture;
			var start = dateOnly1.ToShortDateString();
			var end = dateOnly2.ToShortDateString();
			var rest = TimeHelper.GetLongHourMinuteTimeString(currentRest, currentUiCulture);
			var ret = string.Format(currentUiCulture, errorMessage,
				TimeHelper.GetLongHourMinuteTimeString(nightRest, currentUiCulture), start, end, rest);
			return ret;
		}
	}
}