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
				var hasNonMainShiftActivity = scheduleDay.PersonMeetingCollection().Any();
				if (assignment != null)
				{
					hasNonMainShiftActivity = hasNonMainShiftActivity || (assignment.PersonalActivities() != null && assignment.PersonalActivities().Any()) || (assignment.OvertimeActivities() != null && assignment.OvertimeActivities().Any());
				}
				if (hasNonMainShiftActivity)
				{
					if (_toggleManager.IsEnabled(Toggles.MyTimeWeb_AutoShiftTradeWithMeetingAndPersonalActivity_33281))
					{
						if (!isMeetingOverSchedule(scheduleDay, scheduleDays)) return ret;
					}

					string message = string.Format(CultureInfo.CurrentCulture,
						Resources.HasNonMainShiftActivityErrorMessage, person.Name,
						dateOnly.Date.ToShortDateString());
					ret.Add(createResponse(scheduleDay.Person, dateOnly, message, typeof (NonMainShiftActivityRule)));
				}
				
			}

			return ret;
		}

		private bool isMeetingOverSchedule(IScheduleDay currentSchedule, IEnumerable<IScheduleDay> all)
		{
			var shiftLayers = new List<IShiftLayer>();
			foreach (var sd in all)
			{
				if (currentSchedule.Person.Id == sd.Person.Id) continue;

				shiftLayers = (List<IShiftLayer>)sd.PersonAssignment().ShiftLayers;
				var meetings = currentSchedule.PersonMeetingCollection();
				if (meetings.Count == 0) return true;
				foreach (var personMeeting in meetings)
				{
					var maxPeriod = new DateTimePeriod();
					foreach (var shiftLayer in shiftLayers)
					{
						if (!shiftLayer.Payload.InWorkTime)
						{
							if (shiftLayer.Period.ContainsPart(personMeeting.Period)) return true;
						}
						else
						{
							if (maxPeriod.ElapsedTime() < shiftLayer.Period.ElapsedTime()) maxPeriod = shiftLayer.Period;
						}

						if (shiftLayer.Payload.InWorkTime && !maxPeriod.Contains(personMeeting.Period)) return true;

					}
				}
			}
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