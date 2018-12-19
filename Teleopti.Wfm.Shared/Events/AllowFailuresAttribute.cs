using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class AllowFailuresAttribute : Attribute
	{
		public int Failures { get; set; }

		public AllowFailuresAttribute(int failures)
		{
			Failures = failures;
		}
	}
}