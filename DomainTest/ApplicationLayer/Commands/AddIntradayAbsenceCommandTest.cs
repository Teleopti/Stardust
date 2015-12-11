using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Commands
{
	[TestFixture]
	public class AddIntradayAbsenceCommandTest
	{
		[Test]
		public void ShouldAcceptCorrectTimeOfDays()
		{
			var command = new AddIntradayAbsenceCommand
			{
				StartTime = new DateTime(2013, 11, 27, 14, 00, 00, DateTimeKind.Utc),
				EndTime = new DateTime(2013, 11, 27, 15, 00, 00, DateTimeKind.Utc)
			};

			var result = command.IsValid();

			result.Should().Be.True();
		}

		[Test]
		public void ShouldNotAcceptStartTimeLaterThanEndTime()
		{
			var command = new AddIntradayAbsenceCommand
			{
				StartTime = new DateTime(2013, 11, 27, 15, 00, 00, DateTimeKind.Utc),
				EndTime = new DateTime(2013, 11, 27, 14, 00, 00, DateTimeKind.Utc)
			};

			var result = command.IsValid();

			result.Should().Be.False();

			command.ValidationResult.Should().Be(Resources.InvalidEndTime);
		}
	}

}
