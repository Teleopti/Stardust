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

        public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
        {
            var responseList = new HashSet<IBusinessRuleResponse>();
            if (!ForDelete)
            {
				var groupedByPerson = scheduleDays.GroupBy(s => s.Person);

				foreach (var scheduleDay in groupedByPerson)
				{
					if (!scheduleDay.Any()) continue;
					var period = new DateOnlyPeriod(scheduleDay.Min(s => s.DateOnlyAsPeriod.DateOnly).AddDays(-1),
													scheduleDay.Max(s => s.DateOnlyAsPeriod.DateOnly).AddDays(1));
					var currentSchedules = rangeClones[scheduleDay.Key];
					var daysToCheck =
						currentSchedules.ScheduledDayCollection(period);
				
                    var periodToValidate = period.ToDateTimePeriod(scheduleDay.Key.PermissionInformation.DefaultTimeZone());
                    foreach (var dayToCheck in daysToCheck)
                    {
						foreach (var conflict in dayToCheck.PersonAssignmentConflictCollection)
                        {
                            if (periodToValidate.Intersect(conflict.Period))
                            {
                                IBusinessRuleResponse response = CreateResponse(scheduleDay.Key, dayToCheck.DateOnlyAsPeriod, UserTexts.Resources.BusinessRuleOverlappingErrorMessage2,
                                                                                typeof(NewOverlappingAssignmentRule));

                                responseList.Add(response);
                            }
                        }
                    }
                }
            }   
            
            return responseList;
        }

        private IBusinessRuleResponse CreateResponse(IPerson person, IDateOnlyAsDateTimePeriod dateOnly, string message, Type type)
        {
            var dop = new DateOnlyPeriod(dateOnly.DateOnly, dateOnly.DateOnly);
            IBusinessRuleResponse response = new BusinessRuleResponse(type, message, HaltModify, IsMandatory, dateOnly.Period(), person, dop) { Overridden = !HaltModify };
            return response;
        }
    }
}