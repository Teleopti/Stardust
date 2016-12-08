﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class NonMainShiftActivityRule : INewBusinessRule
	{
		private bool _haltModify = true;

		public string ErrorMessage
		{
			get { return ""; }
		}

		public bool IsMandatory
		{
			get { return false; }
		}

		public bool HaltModify
		{
			get { return _haltModify; }
			set { _haltModify = value; }
		}

		public bool Configurable => true;

		public bool ForDelete { get; set; }

		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
		{
			var ret = new List<IBusinessRuleResponse>();
			foreach (var scheduleDay in scheduleDays)
			{
				var assignment = scheduleDay.PersonAssignment();
				var dateOnly = assignment.Date;
				var person = scheduleDay.Person;

				var hasNonMainShiftMeeting = isMeetingOverSchedule(scheduleDay);
				var hasNonMainShiftActivity = isPersonalActivityOverSchedule(assignment);
				hasNonMainShiftActivity = hasNonMainShiftActivity || hasNonMainShiftMeeting || (assignment.OvertimeActivities() != null && assignment.OvertimeActivities().Any());

				if (hasNonMainShiftActivity)
				{
					string message = string.Format(CultureInfo.CurrentCulture,
						Resources.HasNonMainShiftActivityErrorMessage, person.Name,
						dateOnly.Date.ToShortDateString());
					ret.Add(createResponse(scheduleDay.Person, dateOnly, message, typeof (NonMainShiftActivityRule)));
				}
			}

			return ret;
		}

		public string FriendlyName => Resources.HasNonMainShiftActivityErrorMessage;
		public string Description => Resources.DescriptionOfNonMainShiftActivityRule;

		private bool isPersonalActivityOverSchedule(IPersonAssignment personAssignment)
		{
			var shiftLayers = personAssignment.ShiftLayers;
			var activities = personAssignment.PersonalActivities();
			if (activities != null && activities.Any(activity => isOverSchedule(activity.Period, shiftLayers)))
			{
				return true;
			}

			return false;
		}

		private bool isMeetingOverSchedule(IScheduleDay currentSchedule)
		{
			var shiftLayers = currentSchedule.PersonAssignment().ShiftLayers;
			var meetings = currentSchedule.PersonMeetingCollection();
			if (meetings != null && meetings.Any(personMeeting => isOverSchedule(personMeeting.Period, shiftLayers)))
			{
				return true;
			}
			return false;
		}

		private bool isOverSchedule(DateTimePeriod period, IEnumerable<IShiftLayer> layers)
		{
			if (!layers.Any()) return true;

			var maxPeriod = new DateTimePeriod( layers.Min(p => p.Period.StartDateTime) , layers.Max(p=>p.Period.EndDateTime));
			if (period.EndDateTime <= maxPeriod.StartDateTime || period.StartDateTime >= maxPeriod.EndDateTime) return true;

			foreach (var shiftLayer in layers)
			{
				if (!shiftLayer.Payload.InWorkTime)
				{
					if ((shiftLayer.Period.StartDateTime < period.EndDateTime && shiftLayer.Period.EndDateTime > period.StartDateTime)
						||( shiftLayer.Period.EndDateTime > period.StartDateTime && shiftLayer.Period.StartDateTime < period.EndDateTime)) return true;
				}
			}

			return false;
		}

		private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message, Type type)
		{
			var dop = dateOnly.ToDateOnlyPeriod();
			var period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var response = new BusinessRuleResponse(type, message, _haltModify, IsMandatory, period, person, dop, FriendlyName) { Overridden = !_haltModify };
			return response;
		}
	}
}