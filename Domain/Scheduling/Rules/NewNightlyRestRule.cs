using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public class NewNightlyRestRule : INewBusinessRule
    {
    	private readonly IWorkTimeStartEndExtractor _workTimeStartEndExtractor;
    	private bool _haltModify = true;
        private readonly string _errorMessage = string.Empty;

		public NewNightlyRestRule(IWorkTimeStartEndExtractor workTimeStartEndExtractor)
		{
			_workTimeStartEndExtractor = workTimeStartEndExtractor;
		}

    	public string ErrorMessage
        {
            get { return _errorMessage; }
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

        public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
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

        private IEnumerable<IBusinessRuleResponse> checkDay(IScheduleRange range, DateOnlyPeriod period, IDictionary<DateOnly, scheduleDayForValidation> scheduleDay)
        {
            ICollection<IBusinessRuleResponse> responseList = new List<IBusinessRuleResponse>();
	        var dayCollection = period.DayCollection();
	        for (int dayIndex = 1; dayIndex < dayCollection.Count - 1; dayIndex++)
	        {
		        DateOnly dateToCheck = dayCollection[dayIndex];
		        IPersonPeriod personPeriod = range.Person.Period(dateToCheck);
		        if (personPeriod == null)
			        return responseList;

		        var yesterday = scheduleDay[dateToCheck.AddDays(-1)];
				var today = scheduleDay[dateToCheck];
				var tomorrow = scheduleDay[dateToCheck.AddDays(1)];
		        TimeSpan nightRest = personPeriod.PersonContract.Contract.WorkTimeDirective.NightlyRest;

		        ClearMyResponses(range.BusinessRuleResponseInternalCollection, range.Person, dateToCheck);
		        TimeSpan? currentRest = checkDays(yesterday, today, nightRest);
		        if (currentRest != null)
		        {
			        responseList.Add(createResponse(range.Person, yesterday.Date,
													yesterday.Date, today.Date,
			                                        nightRest, currentRest.Value, range, true));
					responseList.Add(createResponse(range.Person, today.Date,
													yesterday.Date, today.Date,
			                                        nightRest, currentRest.Value, range, !_haltModify));
		        }

		        currentRest = checkDays(today, tomorrow, nightRest);
		        if (currentRest != null)
		        {
					responseList.Add(createResponse(range.Person, today.Date,
													today.Date, tomorrow.Date,
			                                        nightRest, currentRest.Value, range, !_haltModify));
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
	    }

	    private scheduleDayForValidation convert(IScheduleDay scheduleDay)
	    {
		    var projection = scheduleDay.ProjectionService().CreateProjection();
		    return new scheduleDayForValidation
		    {
			    Date = scheduleDay.DateOnlyAsPeriod.DateOnly,
			    SignificantPart = scheduleDay.SignificantPart(),
			    WorkTimeStart = _workTimeStartEndExtractor.WorkTimeStart(projection),
			    WorkTimeEnd = _workTimeStartEndExtractor.WorkTimeEnd(projection)
		    };
	    }

	    private TimeSpan? checkDays(scheduleDayForValidation firstDay, scheduleDayForValidation secondDay, TimeSpan nightRest)
        {
	        var firstSignificant = firstDay.SignificantPart;
	        var secondSignificant = secondDay.SignificantPart;

			bool checkFirstDay = firstSignificant == SchedulePartView.MainShift ||
								 firstSignificant == SchedulePartView.Overtime;
			bool checkSecondDay = secondSignificant == SchedulePartView.MainShift ||
								  secondSignificant == SchedulePartView.Overtime;

			if(!(checkFirstDay && checkSecondDay))
                return null;

        	if(secondDay.WorkTimeStart.HasValue && firstDay.WorkTimeEnd.HasValue)
			{
				var restTime = secondDay.WorkTimeStart.Value.Subtract(firstDay.WorkTimeEnd.Value);
				if (restTime < nightRest)
					return restTime;
			}
            return null;
        }

        private IBusinessRuleResponse createResponseForRemove(IPerson person, DateOnly dateOnly, DateOnlyPeriod dateOnlyPeriod)
        {
            var dop = new DateOnlyPeriod(dateOnly, dateOnly);
            DateTimePeriod period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());

            return new BusinessRuleResponse(typeof(NewNightlyRestRule), "", _haltModify, IsMandatory, period, person, dateOnlyPeriod);
        }

        private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, DateOnly dateOnly1, DateOnly dateOnly2, 
            TimeSpan nightRest, TimeSpan currentRest, ISchedule addToRange, bool overridden)
        {
            var dop = new DateOnlyPeriod(dateOnly, dateOnly);
            DateTimePeriod period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
            string errorMessage = createErrorMessage(dateOnly1, dateOnly2, nightRest, currentRest);
            IBusinessRuleResponse response = new BusinessRuleResponse(typeof(NewNightlyRestRule), errorMessage, _haltModify, IsMandatory, period, person, new DateOnlyPeriod(dateOnly1,dateOnly2))
                                                 {Overridden = overridden};
            AddMyResponse(addToRange.BusinessRuleResponseInternalCollection, response);
            return response;
        }

        private static string createErrorMessage(DateOnly dateOnly1, DateOnly dateOnly2, TimeSpan nightRest, TimeSpan currentRest)
        {
            var loggedOnCulture = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
            string start = dateOnly1.ToShortDateString();
            string end = dateOnly2.ToShortDateString();
            string rest = TimeHelper.GetLongHourMinuteTimeString(currentRest, loggedOnCulture);
            string ret = string.Format(loggedOnCulture,
                                       UserTexts.Resources.BusinessRuleNightlyRestRuleErrorMessage,
                                       TimeHelper.GetLongHourMinuteTimeString(nightRest, loggedOnCulture), start, end, rest);
            return ret;
        }
    }
}