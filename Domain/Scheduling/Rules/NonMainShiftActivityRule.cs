using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

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
			foreach (var scheduleDay in scheduleDays)
			{
				var assignment = scheduleDay?.PersonAssignment();
				if (assignment == null) continue;

				var hasMainShiftMeeting = isMeetingOverSchedule(scheduleDay);
				var hasMainShiftActivity = isPersonalActivityOverSchedule(assignment);
				var hasOvertimeActivity = assignment.OvertimeActivities() != null && assignment.OvertimeActivities().Any();
				if (!hasMainShiftActivity && !hasMainShiftMeeting && !hasOvertimeActivity) continue;

				var person = scheduleDay.Person;
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
			var shiftLayers = personAssignment.ShiftLayers.ToList();
			var activities = personAssignment.PersonalActivities();
			return activities != null && activities.Any(activity => isOverSchedule(activity.Period, shiftLayers));
		}

		private bool isMeetingOverSchedule(IScheduleDay currentSchedule)
		{
			var shiftLayers = currentSchedule.PersonAssignment().ShiftLayers.ToList();
			var meetings = currentSchedule.PersonMeetingCollection();
			return meetings != null && meetings.Any(personMeeting => isOverSchedule(personMeeting.Period, shiftLayers));
		}

		private bool isOverSchedule(DateTimePeriod period, IList<ShiftLayer> layers)
		{
			if (!layers.Any()) return true;

			var maxPeriod = new DateTimePeriod( layers.Min(p => p.Period.StartDateTime) , layers.Max(p=>p.Period.EndDateTime));
			if (period.EndDateTime <= maxPeriod.StartDateTime || period.StartDateTime >= maxPeriod.EndDateTime) return true;

			foreach (var shiftLayer in layers)
			{
				if (shiftLayer.Payload.InWorkTime) continue;

				var layerStartTime = shiftLayer.Period.StartDateTime;
				var layerEndTime = shiftLayer.Period.EndDateTime;
				if ((layerStartTime < period.EndDateTime && layerEndTime > period.StartDateTime)
					|| (layerEndTime > period.StartDateTime && layerStartTime < period.EndDateTime))
				{
					return true;
				}
			}

			return false;
		}

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