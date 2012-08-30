using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	public interface ISignificantChangeChecker
	{
		INotificationMessage SignificantChangeMessages(DateOnlyPeriod dateOnlyPeriod, IPerson person);
	}

	public class SignificantChangeChecker : ISignificantChangeChecker
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public INotificationMessage SignificantChangeMessages(DateOnlyPeriod dateOnlyPeriod, IPerson person)
		{
			var ret = new NotificationMessage();
			var date = DateTime.Now.Date;
			if (dateOnlyPeriod.StartDate > date.AddDays(14))
				return ret;
			if (dateOnlyPeriod.EndDate < date)
				return ret;

			//NEXT PBI check against readmodel and split messages if too long
			var lang = person.PermissionInformation.UICulture();
			var mess = UserTexts.Resources.ResourceManager.GetString("YourWorkingHoursHaveChanged",lang);
			if (string.IsNullOrEmpty(mess))
				mess = UserTexts.Resources.ResourceManager.GetString("YourWorkingHoursHaveChanged");

			ret.Subject = mess;
			return ret;
		}
	}
}