using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public class NewMaxOneDayOffRule : INewBusinessRule
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
        {
            var responseList = new HashSet<IBusinessRuleResponse>();
            if(!ForDelete)
            {
                foreach (var scheduleDay in scheduleDays)
                {
                    IPerson person = scheduleDay.Person;
                    DateOnly dateToCheck = scheduleDay.DateOnlyAsPeriod.DateOnly;
                    IScheduleRange currentSchedules = rangeClones[person];
                    var oldResponses = currentSchedules.BusinessRuleResponseInternalCollection;
                    oldResponses.Remove(CreateResponse(person, dateToCheck, "remove", typeof(NewMaxOneDayOffRule)));
                    //on delete this should be empty and never runned

                    var dayOffs = scheduleDay.PersonDayOffCollection();
                    if (dayOffs != null && dayOffs.Count > 1)
                    {
                        
                        IBusinessRuleResponse response = CreateResponse(scheduleDay.Person,
                            scheduleDay.DateOnlyAsPeriod.DateOnly, String.Format(CultureInfo.CurrentUICulture, UserTexts.Resources.DuplicateDaysOffValidationError, scheduleDay.DateOnlyAsPeriod.DateOnly.ToShortDateString( CultureInfo.CurrentCulture  )),
                                                                                typeof(NewMaxOneDayOffRule));
                        if (response != null)
                        {
                            responseList.Add(response);
                            oldResponses.Add(response);
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
            IBusinessRuleResponse response = new BusinessRuleResponse(type, message, HaltModify, IsMandatory, period, person, dateOnlyPeriod)
                                                 {Overridden = !HaltModify};
            return response;
        }
    }
}
