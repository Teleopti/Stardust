﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class NonMainShiftActivityRule : INewBusinessRule
	{
		private bool _haltModify = true;
		private readonly IScheduleCommandToggle _toggleManager;

		public NonMainShiftActivityRule(IScheduleCommandToggle toggleManager)
		{
			_toggleManager = toggleManager;
		}		

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

		public bool ForDelete { get; set; }

		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
		{
			var ret = new List<IBusinessRuleResponse>();
			foreach (var scheduleDay in scheduleDays)
			{
				var assignment = scheduleDay.PersonAssignment();
				var dateOnly = assignment.Date;
				var person = scheduleDay.Person;
				bool hasNonMainShiftMeeting;
				bool hasNonMainShiftActivity;

				if (_toggleManager.IsEnabled(Toggles.MyTimeWeb_AutoShiftTradeWithMeetingAndPersonalActivity_33281))
				{
					hasNonMainShiftMeeting = isMeetingOverSchedule(scheduleDay);
					hasNonMainShiftActivity = isPersonalActivityOverSchedule(scheduleDay);
				}
				else
				{
					hasNonMainShiftMeeting = scheduleDay.PersonMeetingCollection().Any();
					hasNonMainShiftActivity = assignment.PersonalActivities() != null && assignment.PersonalActivities().Any();
				}
				
				if (assignment != null)
				{
					hasNonMainShiftActivity = hasNonMainShiftActivity || hasNonMainShiftMeeting || (assignment.OvertimeActivities() != null && assignment.OvertimeActivities().Any());
				}
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

		private bool isPersonalActivityOverSchedule(IScheduleDay currentSchedule)
		{
			var shiftLayers = (List<IShiftLayer>)currentSchedule.PersonAssignment().ShiftLayers;
			var activities = currentSchedule.PersonAssignment().PersonalActivities();
			if (activities != null && activities.Any(activity => isOverSchedule(activity.Period, shiftLayers)))
			{
				return true;
			}

			return false;
		}

		private bool isMeetingOverSchedule(IScheduleDay currentSchedule)
		{
			var shiftLayers = (List<IShiftLayer>)currentSchedule.PersonAssignment().ShiftLayers;
			var meetings = currentSchedule.PersonMeetingCollection();
			if (meetings != null && meetings.Any(personMeeting => isOverSchedule(personMeeting.Period, shiftLayers)))
			{
				return true;
			}
			return false;
		}

		private bool isOverSchedule(DateTimePeriod period, IList<IShiftLayer> layers)
		{
			var maxPeriod = new DateTimePeriod( layers.Min(p => p.Period.StartDateTime) , layers.Max(p=>p.Period.EndDateTime));

			foreach (var shiftLayer in layers)
			{
				if (!shiftLayer.Payload.InWorkTime)
				{
					if ((shiftLayer.Period.StartDateTime < period.EndDateTime && shiftLayer.Period.EndDateTime > period.StartDateTime)
						||( shiftLayer.Period.EndDateTime > period.StartDateTime && shiftLayer.Period.StartDateTime < period.EndDateTime)) return true;
				}
			}
			if (period.EndDateTime <= maxPeriod.StartDateTime || period.StartDateTime >= maxPeriod.EndDateTime) return true;

			return false;
		}

		private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message, Type type)
		{
			var dop = new DateOnlyPeriod(dateOnly, dateOnly);
			DateTimePeriod period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			IBusinessRuleResponse response = new BusinessRuleResponse(type, message, _haltModify, IsMandatory, period, person, dop) { Overridden = !_haltModify };
			return response;
		}
	}
}