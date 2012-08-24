using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.SMS
{
	public interface ISignificantChangeChecker
	{
		IList<string> SignificantChangeMessages(DateOnlyPeriod dateOnlyPeriod, IPerson person);
	}

	public class SignificantChangeChecker : ISignificantChangeChecker
	{
		public IList<string> SignificantChangeMessages(DateOnlyPeriod dateOnlyPeriod, IPerson person)
		{
			var ret = new List<string>();
			var date = DateTime.Now.Date;
			if (dateOnlyPeriod.StartDate > date.AddDays(14))
				return ret;
			if (dateOnlyPeriod.EndDate < date)
				return ret;

			//NEXT PBI check against readmodel and split messages if too long
			var lang = person.PermissionInformation.UICulture();
			var mess = UserTexts.Resources.ResourceManager.GetString("YourWorkingHoursHaveChanged",lang);
			if (string.IsNullOrEmpty(mess))
				mess = UserTexts.Resources.ResourceManager.GetString("YourWorkingHoursHaveChanged", CultureInfo.GetCultureInfo("en"));

			ret.Add(mess);
			return ret;
		}
	}
}