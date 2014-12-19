using System;
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
					string message = string.Format(CultureInfo.CurrentCulture,
												 Resources.HasNonMainShiftActivityErrorMessage, person.Name,
												 dateOnly.Date.ToShortDateString());
					ret.Add(createResponse(scheduleDay.Person, dateOnly, message, typeof(NonMainShiftActivityRule)));
				}
				
			}

			return ret;
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