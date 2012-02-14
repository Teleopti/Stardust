using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Time
{
	[TestFixture]
	public class TimeOfDayTest
	{
		[Test]
		public void ShouldSetChosenTimeSpan()
		{
			var time = TimeSpan.FromHours(3);
			var target = new TimeOfDay(time);

			target.Time
				.Should().Be.EqualTo(time);
		}
	}
}