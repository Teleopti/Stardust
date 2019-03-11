using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Status;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Status
{
	[DomainTest]
	public class CustomStatusStepTest
	{
		[Test]
		public void ShouldSetSuccessCorrectly([Range(0, 3)]int secondsSinceLastPing, [Range(0, 3)] int secondsLimit)
		{
			var timeSinceLastPing = new FakeTimeSinceLastPing().SetValue(TimeSpan.FromSeconds(secondsSinceLastPing));
			var target = new CustomStatusStep(string.Empty, string.Empty, timeSinceLastPing, TimeSpan.FromSeconds(secondsLimit));
			var expected = secondsSinceLastPing <= secondsLimit;

			target.Execute().Success
				.Should().Be.EqualTo(expected);
		}
		
		[Test]
		public void ShouldSetMessageWhenSuccess()
		{
			const int secondsSinceLastPing = 50;
			var statusName = Guid.NewGuid().ToString();
			var timeSinceLastPing = new FakeTimeSinceLastPing().SetValue(TimeSpan.FromSeconds(secondsSinceLastPing));
			var target = new CustomStatusStep(statusName, string.Empty, timeSinceLastPing, TimeSpan.FromSeconds(secondsSinceLastPing + 50));
			
			target.Execute().Output
				.Should().Be.EqualTo(string.Format(CustomStatusStep.Message, statusName, secondsSinceLastPing));
		}
		
		[Test]
		public void ShouldSetMessageWhenFailure()
		{
			const int secondsSinceLastPing = 100;
			var statusName = Guid.NewGuid().ToString();
			var timeSinceLastPing = new FakeTimeSinceLastPing().SetValue(TimeSpan.FromSeconds(secondsSinceLastPing));
			var target = new CustomStatusStep(statusName, string.Empty, timeSinceLastPing, TimeSpan.FromSeconds(secondsSinceLastPing - 50));
			
			target.Execute().Output
				.Should().Be.EqualTo(string.Format(CustomStatusStep.Message, statusName, secondsSinceLastPing));
		}

		[Test]
		public void ShouldSetMessageWhenDeadForLongTime()
		{
			var statusName = Guid.NewGuid().ToString();
			var timeSinceLastPing = new FakeTimeSinceLastPing().SetValue(TimeSpan.FromDays(50));
			var target = new CustomStatusStep(statusName, string.Empty, timeSinceLastPing, TimeSpan.FromSeconds(50));
			
			target.Execute().Output
				.Should().Be.EqualTo(string.Format(CustomStatusStep.MessageWhenDeadForLongTime, statusName));
		}
	}
}