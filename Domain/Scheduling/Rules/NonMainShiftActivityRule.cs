﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
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
			var currentUiCulture = Thread.CurrentThread.CurrentCulture;
			var ret = new List<IBusinessRuleResponse>();
			foreach (var scheduleDay in scheduleDays)
			{
				var assignment = scheduleDay.PersonAssignment();
				var dateOnly = assignment.Date;
				var person = scheduleDay.Person;

				var hasNonMainShiftMeeting = isMeetingOverSchedule(scheduleDay);
				var hasNonMainShiftActivity = isPersonalActivityOverSchedule(assignment);
				hasNonMainShiftActivity = hasNonMainShiftActivity || hasNonMainShiftMeeting ||
										  (assignment.OvertimeActivities() != null && assignment.OvertimeActivities().Any());

				if (!hasNonMainShiftActivity) continue;

				var message = string.Format(currentUiCulture, Resources.HasNonMainShiftActivityErrorMessage, person.Name,
					dateOnly.Date.ToShortDateString());
				ret.Add(createResponse(scheduleDay.Person, dateOnly, message, typeof(NonMainShiftActivityRule)));
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

		private bool isOverSchedule(DateTimePeriod period, IList<IShiftLayer> layers)
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