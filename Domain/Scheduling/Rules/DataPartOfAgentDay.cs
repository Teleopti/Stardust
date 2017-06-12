using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class DataPartOfAgentDay : INewBusinessRule
	{
		public DataPartOfAgentDay()
		{
			IsMandatory = true;
			HaltModify = true;
			ForDelete = true;
		}

		public bool IsMandatory { get; private set; }
		public bool HaltModify { get; set; }
		public bool Configurable => false;
		public bool ForDelete { get; set; }

		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
		{
			var ret = new List<IBusinessRuleResponse>();
			foreach (var scheduleDay in scheduleDays)
			{
				var assignment = scheduleDay.PersonAssignment();
				if (assignment == null) continue;

				var assignmentPeriod = assignment.Period;
				var dateOnly = assignment.Date;
				var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
				//don't want dep to person here...
				var agentTimeZone = scheduleDay.Person.PermissionInformation.DefaultTimeZone();

				var assignmentDayInAgentTimeZone = TimeZoneHelper.ConvertFromUtc(assignment.Period.StartDateTime, agentTimeZone).Date;
				var scheduleDayInAgentTimezone = TimeZoneHelper.ConvertFromUtc(scheduleDay.Period.StartDateTime, agentTimeZone).Date;
				if (assignmentDayInAgentTimeZone == scheduleDayInAgentTimezone) continue;

				var friendlyName = Resources.NotAllowedMoveOfAssignmentToOtherDate;
				ret.Add(new BusinessRuleResponse(typeof(DataPartOfAgentDay), Resources.NotAllowedMoveOfAssignmentToOtherDate, true, true, assignmentPeriod,
												 scheduleDay.Person, dateOnlyPeriod, friendlyName));
			}
			return ret;
		}

		public string Description => Resources.DescriptionOfDataPartOfAgentDay;
	}
}