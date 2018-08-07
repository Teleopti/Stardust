using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public interface IShiftTradeMultiSchedulesSelectableChecker
	{
		bool CheckSelectable(bool hasAbsence, IScheduleDay myScheduleDay, IScheduleDay personToScheduleDay, DateOnly date, IPerson personTo, out string unSelectableReason);
	}

	public class ShiftTradeMultiSchedulesSelectableChecker : IShiftTradeMultiSchedulesSelectableChecker
	{
		private readonly ILoggedOnUser _loggedOnUser;

		public ShiftTradeMultiSchedulesSelectableChecker(ILoggedOnUser loggedOnUser)
		{
			_loggedOnUser = loggedOnUser;
		}

		public bool CheckSelectable(bool hasAbsence, IScheduleDay myScheduleDay, IScheduleDay personToScheduleDay, 
									DateOnly date, IPerson personTo, out string unSelectableReason)
		{
			unSelectableReason = "";

			if (hasAbsence)
			{
				unSelectableReason = Resources.AbsenceCannotBeTraded;
				return false;
			}

			if (!isDateSatisfied(myScheduleDay, personToScheduleDay, date, personTo))
			{
				unSelectableReason = String.Format(Resources.ScheduleDateDoNotMatch, personTo.Name);
				return false;
			}

			if (!isSkillSatisfied(personTo, date))
			{
				unSelectableReason = String.Format(Resources.SkillDoNotMatch, personTo.Name);
				return false;
			}

			return true;
		}

		private bool isSkillSatisfied(IPerson personTo, DateOnly date)
		{
			var checkItem = new ShiftTradeAvailableCheckItem(date, _loggedOnUser.CurrentUser(), personTo);
			var skillSpecification = new ShiftTradeSkillSpecification();
			return skillSpecification.IsSatisfiedBy(checkItem);
		}

		private bool isDateSatisfied(IScheduleDay myScheduleDay, IScheduleDay personToScheduleDay, DateOnly date, IPerson personTo)
		{
			var personToFirstLayer = personToScheduleDay?.PersonAssignment()?.ShiftLayers.FirstOrDefault();
			var myTimezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var personTostartTime = TimeZoneHelper.ConvertToUtc(date.Date, myTimezone);
			if (personToFirstLayer != null) personTostartTime = personToFirstLayer.Period.StartDateTime;

			var personToTimeZone = personTo.PermissionInformation.DefaultTimeZone();
			var myfirstLayer = myScheduleDay?.PersonAssignment()?.ShiftLayers.FirstOrDefault();
			var myStartTime = TimeZoneHelper.ConvertToUtc(date.Date, personToTimeZone);
			if (myfirstLayer != null) myStartTime = myfirstLayer.Period.StartDateTime;

			var startInMyTimezone = TimeZoneHelper.ConvertFromUtc(personTostartTime, myTimezone);
			var startInPersonToTimezone = TimeZoneHelper.ConvertFromUtc(myStartTime, personToTimeZone);
			if (startInMyTimezone < date.Date || startInMyTimezone >= date.Date.AddHours(24)
											  || startInPersonToTimezone < date.Date || startInPersonToTimezone >= date.Date.AddDays(24))
			{
				return false;
			}

			return true;
		}
	}
}