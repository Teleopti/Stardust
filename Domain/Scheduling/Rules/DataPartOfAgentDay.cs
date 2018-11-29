using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

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

		public bool IsMandatory { get; }
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
				if(assignment.ShiftLayers.Count() == assignment.PersonalActivities().Count()) continue;
				var assignmentPeriod = assignment.Period;
				var dateOnly = assignment.Date;
				var dateOnlyPeriod = dateOnly.ToDateOnlyPeriod();
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