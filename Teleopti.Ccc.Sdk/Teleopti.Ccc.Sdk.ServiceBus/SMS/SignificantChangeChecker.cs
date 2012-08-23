using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.SMS
{
	public interface ISignificantChangeChecker
	{
		bool IsSignificantChange(DateOnlyPeriod dateOnlyPeriod, IPerson person);
	}

	public class SignificantChangeChecker : ISignificantChangeChecker
	{
		public bool IsSignificantChange(DateOnlyPeriod dateOnlyPeriod, IPerson person)
		{
			var date = DateTime.Now.Date;
			if (dateOnlyPeriod.StartDate > date.AddDays(14))
				return false;
			if (dateOnlyPeriod.EndDate < date)
				return false;
			return true;
		}
	}
}