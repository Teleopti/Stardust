using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

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
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(9))
			};

			var result = command.IsValid();

			result.Should().Be.True();
		}

		[Test]
		public void ShouldNotAcceptStartTimeLaterThanEndTime()
		{
			var command = new AddIntradayAbsenceCommand
			{
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(7))
			};

			var result = command.IsValid();

			result.Should().Be.False();

			command.ValidationResult.Count.Should().Be(1);
			command.ValidationResult.First().Should().Be(Resources.InvalidEndTime);
		}
	}

}
