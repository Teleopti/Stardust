using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class NonMainShiftActivityRule : INewBusinessRule
	{
		public bool IsMandatory => false;

		public bool HaltModify { get; set; } = true;

		public bool Configurable => true;

		public bool ForDelete { get; set; }

		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones,
			IEnumerable<IScheduleDay> scheduleDays)
		{
			var ret = new List<IBusinessRuleResponse>();
			if (scheduleDays == null) return ret;

			var currentUiCulture = Thread.CurrentThread.CurrentUICulture;
			var validScheduleDays = scheduleDays.Where(s => s != null);
			foreach (var scheduleDay in validScheduleDays)
			{
				// To fix part of bug #44168: Hangfire error: ShiftTradeRequestHandler got AcceptShiftTradeEvent on Teleopti WFM
				// Sometimes "NullReferenceException" will be thrown in this method, person==null is the only possible reason.
				// No test case for this check since it's impossible to create a ScheduleDay with null person (ScheduleParameter 
				// does not allow null value for parameter person）.
				// Not sure what's the root cause.
				var person = scheduleDay.Person;
				if (person == null) continue;

				var assignment = scheduleDay.PersonAssignment();
				if (assignment == null) continue;

				var overtimeActivities = assignment.OvertimeActivities();
				var hasMainShiftActivity = isPersonalActivityOverSchedule(assignment);
				var hasMainShiftMeeting = isMeetingOverSchedule(scheduleDay);
				var hasOvertimeActivity = overtimeActivities != null && overtimeActivities.Any();
				if (!hasMainShiftActivity && !hasMainShiftMeeting && !hasOvertimeActivity) continue;

				var assignmentDate = assignment.Date;
				var message = string.Format(currentUiCulture, Resources.HasNonMainShiftActivityErrorMessage, person.Name,
					assignmentDate.Date.ToShortDateString());
				ret.Add(createResponse(person, assignmentDate, message, typeof(NonMainShiftActivityRule)));
			}

			return ret;
		}

		public string Description => Resources.DescriptionOfNonMainShiftActivityRule;

		private bool isPersonalActivityOverSchedule(IPersonAssignment personAssignment)
		{
			var activities = personAssignment.PersonalActivities();
			return activities != null && activities.Any();
		}

		private bool isMeetingOverSchedule(IScheduleDay currentSchedule)
		{
			var meetings = currentSchedule.PersonMeetingCollection();
			return meetings != null && meetings.Any();
		}

		//bug 77297: It will check meeting or personal activity only according to Business Rule setting,
		//bug 77297: regardless of if it would end up within or outside of the shift after the trade. 
		//But I think the original design is better, maybe we want get it back in future, so keep it here.
		//public bool IsOverSchedule(DateTimePeriod period, IList<ShiftLayer> layers)
		//{
		//	if (layers == null || !layers.Any()) return true;

		//	var maxPeriod = new DateTimePeriod( layers.Min(p => p.Period.StartDateTime) , layers.Max(p=>p.Period.EndDateTime));
		//	if (period.EndDateTime <= maxPeriod.StartDateTime || period.StartDateTime >= maxPeriod.EndDateTime) return true;

		//	foreach (var shiftLayer in layers)
		//	{
		//		if (shiftLayer.Payload.InWorkTime) continue;

		//		var layerStartTime = shiftLayer.Period.StartDateTime;
		//		var layerEndTime = shiftLayer.Period.EndDateTime;
		//		if ((layerStartTime < period.EndDateTime && layerEndTime > period.StartDateTime)
		//			|| (layerEndTime > period.StartDateTime && layerStartTime < period.EndDateTime))
		//		{
		//			return true;
		//		}
		//	}

		//	return false;
		//}

		private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message, Type type)
		{
			var friendlyName = Resources.HasNonMainShiftActivityErrorMessage;
			var dop = dateOnly.ToDateOnlyPeriod();
			var period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var response = new BusinessRuleResponse(type, message, HaltModify, IsMandatory, period, person, dop,
				friendlyName) { Overridden = !HaltModify };
			return response;
		}
	}
}