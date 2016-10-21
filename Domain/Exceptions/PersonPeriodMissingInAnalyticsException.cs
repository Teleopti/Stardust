using System;

namespace Teleopti.Ccc.Domain.Exceptions
{
	public class PersonPeriodMissingInAnalyticsException : DataMissingInAnalyticsException
	{
		public PersonPeriodMissingInAnalyticsException() : base("PersonPeriod")
		{
		}
		public PersonPeriodMissingInAnalyticsException(Guid personPeriodCode) : base($"PersonPeriod ({personPeriodCode})")
		{
		}
	}
}