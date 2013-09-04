using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public class GapsInAssignmentRule : INewBusinessRule
    {
        private readonly IGapsInAssignment _gapsInAssignment;

        public GapsInAssignmentRule(IGapsInAssignment gapsInAssignment)
        {
            _gapsInAssignment = gapsInAssignment;
        }

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

        public bool ForDelete
        {
            get { return true; }
            set { }
        }

        public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
        {
            var responseList = new HashSet<IBusinessRuleResponse>();
            IPerson person = null;
            IScheduleDay day = null;
            try
            {
                foreach (IScheduleDay scheduleDay in scheduleDays)
                {
                    person = scheduleDay.Person;
                    day = scheduleDay;
                    foreach (var assignment in scheduleDay.PersonAssignmentCollection())
                    {
                        _gapsInAssignment.CheckEntity(assignment);
                    }
                }
            }
            catch (ValidationException exception)
            {
                responseList.Add(createResponse(person, day, exception.Message));
            }
            
            return responseList;
        }

        private IBusinessRuleResponse createResponse(IPerson person, IScheduleDay scheduleDay, string errorMessage)
        {
            DateOnly day = scheduleDay.DateOnlyAsPeriod.DateOnly;
            var dateOnlyPeriod = new DateOnlyPeriod(day, day);
            DateTimePeriod period = dateOnlyPeriod.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
            return new BusinessRuleResponse(typeof(GapsInAssignmentRule), errorMessage, HaltModify, IsMandatory, period, person, dateOnlyPeriod);
        }
    }
}
