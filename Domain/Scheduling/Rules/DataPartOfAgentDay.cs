using System.Collections.Generic;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class DataPartOfAgentDay : INewBusinessRule
	{
		public DataPartOfAgentDay()
		{
			//Vad är detta!?!?
			ErrorMessage = string.Empty;
			IsMandatory = true;
			HaltModify = true;
			ForDelete = true;
		}

		public string ErrorMessage { get; private set; }
		public bool IsMandatory { get; private set; }
		public bool HaltModify { get; set; }
		public bool ForDelete { get; set; }

		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
		{
			var ret = new List<IBusinessRuleResponse>();
			foreach (var scheduleDay in scheduleDays)
			{
				var assignment = scheduleDay.PersonAssignment();
				if (assignment != null)
				{
					var assignmentPeriod = assignment.Period;
					var dateOnly = assignment.Date;
					var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
					//don't want dep to person here...
					var dateOnlyAsDatePeriod = dateOnlyPeriod.ToDateTimePeriod(scheduleDay.Person.PermissionInformation.DefaultTimeZone());

					if (!dateOnlyAsDatePeriod.Contains(assignmentPeriod.StartDateTime))
					{
						ret.Add(new BusinessRuleResponse(typeof(DataPartOfAgentDay), Resources.NotAllowedMoveOfAssignmentToOtherDate, true, true, assignmentPeriod,
																						 scheduleDay.Person, dateOnlyPeriod));
					}
				}
			}
			return ret;
		}
	}
}