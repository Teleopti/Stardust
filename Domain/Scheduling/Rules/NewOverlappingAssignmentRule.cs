using System;
using System.Collections.Generic;
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

        public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
        {
            var responseList = new HashSet<IBusinessRuleResponse>();
            if (!ForDelete)
            {
                foreach (IScheduleDay day in scheduleDays)
                {
                    var person = day.Person;
                    var currentSchedules = rangeClones[person];
                    var dateToCheck = day.DateOnlyAsPeriod.DateOnly;
                    var daysToCheck = currentSchedules.ScheduledDayCollection(new DateOnlyPeriod(dateToCheck.AddDays(-1), dateToCheck.AddDays(1)));

                    var periodToValidate = new DateOnlyPeriod(dateToCheck.AddDays(-1), dateToCheck.AddDays(1))
                        .ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
                    foreach (var scheduleDay in daysToCheck)
                    {
                        foreach (var conflict in scheduleDay.PersonAssignmentConflictCollection)
                        {
                            if (periodToValidate.Intersect(conflict.Period))
                            {
                                IBusinessRuleResponse response = CreateResponse(person, scheduleDay.DateOnlyAsPeriod.DateOnly, UserTexts.Resources.BusinessRuleOverlappingErrorMessage2,
                                                                                typeof(NewOverlappingAssignmentRule));

                                responseList.Add(response);
                            }
                        }
                    }
                }
            }   
            
            return responseList;
        }

        private IBusinessRuleResponse CreateResponse(IPerson person, DateOnly dateOnly, string message, Type type)
        {
            var dop = new DateOnlyPeriod(dateOnly, dateOnly);
            DateTimePeriod period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
            var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
            IBusinessRuleResponse response = new BusinessRuleResponse(type, message, HaltModify, IsMandatory, period, person, dateOnlyPeriod) { Overridden = !HaltModify };
            return response;
        }
    }
}