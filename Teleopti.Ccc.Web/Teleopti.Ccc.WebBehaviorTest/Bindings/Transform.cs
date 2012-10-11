using System;
using TechTalk.SpecFlow;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class Transform
	{
		[StepArgumentTransformation]
		public static TimePeriod ToTimePeriod(string value)
		{
			var values = value.Split('-');
			var start = ToTimeSpan(values[0]);
			var end = start;
			if (values.Length > 1)
				end = ToTimeSpan(values[1]);
			return new TimePeriod(start, end);
		}

		[StepArgumentTransformation]
		public static TimeSpan ToTimeSpan(string value)
		{
			return TimeSpan.Parse(value);
		}

		[StepArgumentTransformation]
		public static TimeSpan? ToNullableTimeSpan(string value)
		{
			if (value == null)
				return null;
			return TimeSpan.Parse(value);
		}
	}
}