using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public class NewOverlappingAssignmentRule : INewBusinessRule
    {
        public string ErrorMessage
        {
            get { return ""; }
        }

        public bool IsMandatory
        {
            get { return true; }
        }

        public bool HaltModify
        {
            get { return true; }
            set { }
        }

        public bool ForDelete { get; set; }

	    public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones,
	                                                       IEnumerable<IScheduleDay> scheduleDays)
	    {
		    var responseList = new HashSet<IBusinessRuleResponse>();
		    if (!ForDelete)
		    {
			    foreach (IScheduleDay day in scheduleDays)
			    {
				    var person = day.Person;
				    var dateToCheck = day.DateOnlyAsPeriod.DateOnly;
				    foreach (var assignment in day.PersonAssignmentCollection())
				    {
					    var assignmentPeriod = assignment.Period;
					    var range = rangeClones[person];
						checkToday(day, assignmentPeriod, person, dateToCheck, responseList);
						checkAgainstOtherDate(person, dateToCheck.AddDays(-1), assignmentPeriod, range, responseList);
						checkAgainstOtherDate(person, dateToCheck.AddDays(1), assignmentPeriod, range, responseList);
				    }
			    }
		    }

		    return responseList;
	    }

		private void checkToday(IScheduleDay day, DateTimePeriod assignmentPeriod, IPerson person, DateOnly dateToCheck,
	                            HashSet<IBusinessRuleResponse> responseList)
	    {
		    foreach (var conflict in day.PersonAssignmentConflictCollection)
		    {
				if (assignmentPeriod.Intersect(conflict.Period))
			    {
				    IBusinessRuleResponse response = createResponse(person, dateToCheck,
				                                                    UserTexts.Resources.BusinessRuleOverlappingErrorMessage2,
				                                                    typeof (NewOverlappingAssignmentRule));
				    responseList.Add(response);
			    }
		    }
	    }

		private void checkAgainstOtherDate(IPerson person, DateOnly otherDateTocheck, DateTimePeriod assignmentPeriod, IScheduleRange range, HashSet<IBusinessRuleResponse> responseList)
		{
			var otherDay = range.ScheduledDay(otherDateTocheck);
		    foreach (var otherAss in otherDay.PersonAssignmentCollection())
		    {
			    if (assignmentPeriod.Intersect(otherAss.Period))
			    {
					IBusinessRuleResponse response = createResponse(person, otherDateTocheck,
																	UserTexts.Resources.BusinessRuleOverlappingErrorMessage2,
																	typeof(NewOverlappingAssignmentRule));
					responseList.Add(response);
			    }
		    }

		}

        private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message, Type type)
        {
            var dop = new DateOnlyPeriod(dateOnly, dateOnly);
            DateTimePeriod period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
            var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
            IBusinessRuleResponse response = new BusinessRuleResponse(type, message, HaltModify, IsMandatory, period, person, dateOnlyPeriod) { Overridden = !HaltModify };
            return response;
        }
    }
}