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
			var target = new CustomStatusStep(0, string.Empty, string.Empty, TimeSpan.FromSeconds(secondsSinceLastPing), TimeSpan.FromSeconds(secondsLimit), false);
			var expected = secondsSinceLastPing <= secondsLimit;

			target.Execute().Success
				.Should().Be.EqualTo(expected);
		}
		
		[Test]
		public void ShouldSetMessageWhenSuccess()
		{
			const int secondsSinceLastPing = 50;
			var statusName = Guid.NewGuid().ToString();
			var target = new CustomStatusStep(0, statusName, string.Empty, TimeSpan.FromSeconds(secondsSinceLastPing), TimeSpan.FromSeconds(secondsSinceLastPing + 50), false);
			
			target.Execute().Output
				.Should().Be.EqualTo(string.Format(CustomStatusStep.Message, statusName, secondsSinceLastPing));
		}
		
		[Test]
		public void ShouldSetMessageWhenFailure()
		{
			const int secondsSinceLastPing = 100;
			var statusName = Guid.NewGuid().ToString();
			var target = new CustomStatusStep(0, statusName, string.Empty, TimeSpan.FromSeconds(secondsSinceLastPing), TimeSpan.FromSeconds(secondsSinceLastPing - 50), false);
			
			target.Execute().Output
				.Should().Be.EqualTo(string.Format(CustomStatusStep.Message, statusName, secondsSinceLastPing));
		}

		[Test]
		public void ShouldSetMessageWhenDeadForLongTime()
		{
			var statusName = Guid.NewGuid().ToString();
			var target = new CustomStatusStep(0, statusName, string.Empty, TimeSpan.FromDays(50), TimeSpan.FromSeconds(50), false);
			
			target.Execute().Output
				.Should().Be.EqualTo(string.Format(CustomStatusStep.MessageWhenDeadForLongTime, statusName));
		}
	}
}