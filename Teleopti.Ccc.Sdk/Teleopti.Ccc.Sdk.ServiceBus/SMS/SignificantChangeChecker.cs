using System;
using System.Collections.Generic;
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
			//TODO get string from resource and language from person
			ret.Add("Your schedule has changed!");
			return ret;
		}
	}
}