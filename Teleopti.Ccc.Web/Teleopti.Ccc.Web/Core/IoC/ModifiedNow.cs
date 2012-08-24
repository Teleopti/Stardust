using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.IoC
{
	public class ModifiedNow : INow
	{
		private DateTime utcNow;

		public ModifiedNow(DateTime setUtcNow)
		{
			InParameter.VerifyDateIsUtc("setUtcNow", setUtcNow);
			utcNow = setUtcNow;
		}

		public DateTime LocalTime()
		{
			//not correct we think... need to inject a culture. do later if needed for the behaviour tests...
			return utcNow.ToLocalTime();
		}

		public DateTime UtcTime()
		{
			return utcNow;
		}

		public DateOnly Date()
		{
			return new DateOnly(LocalTime());
		}
	}
}