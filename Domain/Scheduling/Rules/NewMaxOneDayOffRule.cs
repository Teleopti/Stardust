using System;
using System.Collections.Generic;
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

        public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
        {
            var responseList = new HashSet<IBusinessRuleResponse>();
            if(!ForDelete)
            {
                foreach (var scheduleDay in scheduleDays)
                {
                    var dayOffs = scheduleDay.PersonDayOffCollection();
                    if (dayOffs != null && dayOffs.Count > 1)
                    {
                        IBusinessRuleResponse response = CreateResponse(scheduleDay.Person, scheduleDay.DateOnlyAsPeriod.DateOnly, "Max One Day Off",
                                                                                typeof(NewMaxOneDayOffRule));
                        responseList.Add(response);
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
