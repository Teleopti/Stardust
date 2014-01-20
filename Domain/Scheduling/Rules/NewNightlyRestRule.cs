using System;
using System.Collections.Generic;
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

            foreach (var scheduleDay in scheduleDays)
            {
                responsList.AddRange(checkDay(rangeClones[scheduleDay.Person], scheduleDay));
            }

            return responsList;
        }

        public void ClearMyResponses(IList<IBusinessRuleResponse> responseList, IPerson person, DateOnly dateOnly)
        {
            responseList.Remove(createResponseForRemove(person, dateOnly.AddDays(-1), new DateOnlyPeriod(dateOnly.AddDays(-1), dateOnly)));
            responseList.Remove(createResponseForRemove(person, dateOnly, new DateOnlyPeriod(dateOnly.AddDays(-1), dateOnly)));
            responseList.Remove(createResponseForRemove(person, dateOnly, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1))));
            responseList.Remove(createResponseForRemove(person, dateOnly.AddDays(1), new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1))));
        }

        public static void AddMyResponse(IList<IBusinessRuleResponse> responseList, IBusinessRuleResponse response)
        {
            responseList.Add(response);
        }

        private IEnumerable<IBusinessRuleResponse> checkDay(IScheduleRange range, IScheduleDay scheduleDay)
        {
            ICollection<IBusinessRuleResponse> responsList = new List<IBusinessRuleResponse>();
            DateOnly dateToCheck = scheduleDay.DateOnlyAsPeriod.DateOnly;
            IPersonPeriod personPeriod = range.Person.Period(dateToCheck);
            if (personPeriod == null)
                return responsList;

            IScheduleDay yesterday = range.ScheduledDay(dateToCheck.AddDays(-1));
            IScheduleDay today = range.ScheduledDay(dateToCheck);
            IScheduleDay tomorrow = range.ScheduledDay(dateToCheck.AddDays(1));
            TimeSpan nightRest = personPeriod.PersonContract.Contract.WorkTimeDirective.NightlyRest;

            ClearMyResponses(range.BusinessRuleResponseInternalCollection, range.Person, dateToCheck);
            TimeSpan? currentRest = checkDays(yesterday, today, nightRest);
            if(currentRest != null)
            {
                responsList.Add(createResponse(range.Person, yesterday.DateOnlyAsPeriod.DateOnly,
                                               yesterday.DateOnlyAsPeriod.DateOnly, today.DateOnlyAsPeriod.DateOnly,
                                               nightRest, currentRest.Value, range, true));
                responsList.Add(createResponse(range.Person, today.DateOnlyAsPeriod.DateOnly,
                                               yesterday.DateOnlyAsPeriod.DateOnly, today.DateOnlyAsPeriod.DateOnly,
                                               nightRest, currentRest.Value, range, !_haltModify));
            }
            
            currentRest = checkDays(today, tomorrow, nightRest);
            if (currentRest != null)
            {
                responsList.Add(createResponse(range.Person, today.DateOnlyAsPeriod.DateOnly,
                                               today.DateOnlyAsPeriod.DateOnly, tomorrow.DateOnlyAsPeriod.DateOnly,
                                               nightRest, currentRest.Value, range, !_haltModify));
                responsList.Add(createResponse(range.Person, tomorrow.DateOnlyAsPeriod.DateOnly,
                                               today.DateOnlyAsPeriod.DateOnly, tomorrow.DateOnlyAsPeriod.DateOnly,
                                               nightRest, currentRest.Value, range, true));
            }
            
            return responsList;
        }

        private TimeSpan? checkDays(IScheduleDay firstdDay, IScheduleDay secondDay, TimeSpan nightRest)
        {
	        var firstSignificant = firstdDay.SignificantPart();
	        var secondSignificant = secondDay.SignificantPart();

			bool checkFirstDay = firstSignificant == SchedulePartView.MainShift ||
								 firstSignificant == SchedulePartView.Overtime;
			bool checkSecondDay = secondSignificant == SchedulePartView.MainShift ||
								  secondSignificant == SchedulePartView.Overtime;

			if(!(checkFirstDay && checkSecondDay))
                return null;

        	var secondDayStart = _workTimeStartEndExtractor.WorkTimeStart(secondDay.ProjectionService().CreateProjection());
        	var firstDayEnd = _workTimeStartEndExtractor.WorkTimeEnd(firstdDay.ProjectionService().CreateProjection());
			if(secondDayStart != null && firstDayEnd != null)
			{
				var restTime = secondDayStart.Value.Subtract(firstDayEnd.Value);
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
            var loggedOnCulture = TeleoptiPrincipal.Current.Regional.Culture;
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