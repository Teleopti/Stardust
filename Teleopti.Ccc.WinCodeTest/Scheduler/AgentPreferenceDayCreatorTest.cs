using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.WinCode.Scheduling;
using NUnit.Framework;
using Rhino.Mocks;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class AgentPreferenceDayCreatorTest
	{
		private AgentPreferenceDayCreator _preferenceDayCreator;

	
		[SetUp]
		public void Setup()
		{
			_preferenceDayCreator = new AgentPreferenceDayCreator();	
		}

		[Test]
		public void ShouldBeAbleToCreateWithValidTimes()
		{
			TimeSpan? minStart = TimeSpan.FromHours(1);
			TimeSpan? maxStart = TimeSpan.FromHours(2);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			var result = _preferenceDayCreator.CanCreate(minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);
		}
	}
}
