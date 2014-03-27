using System;
using System.Collections.Specialized;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Rta.Server.Repeater;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Rta.ServerTest.Repeater
{
	public class MinuteTriggerTest
	{
		[Test]
		public void ShouldNotCrashWithPositiveInterval()
		{
			var configReader = MockRepository.GenerateMock<IConfigReader>();
			configReader.Stub(x => x.AppSettings).Return(new NameValueCollection {{MinuteTrigger.RepeatIntervalKey, "1"}});
			var target = new MinuteTrigger(configReader);
			var action = new Action(dummy);
			target.Initialize(action);
		}

		[Test]
		public void ShouldNotAcceptNegativeValues([Values(-2,0)] int value)
		{
			var configReader = MockRepository.GenerateMock<IConfigReader>();
			configReader.Stub(x => x.AppSettings).Return(new NameValueCollection { { MinuteTrigger.RepeatIntervalKey, value.ToString() } });
			var target = new MinuteTrigger(configReader);
			var action = new Action(dummy);
			Assert.Throws<InvalidOperationException>(()=>target.Initialize(action));
		}

		[Test]
		public void ShouldTriggerMoreThanOnce()
		{
			var ok = 0;
			var configReader = MockRepository.GenerateMock<IConfigReader>();
			configReader.Stub(x => x.AppSettings).Return(new NameValueCollection { { MinuteTrigger.RepeatIntervalKey, "extremelyLow" } });
			var target = new MinuteTrigger(configReader);
			var action = new Action(() => ok++);

			target.Initialize(action);

			for (var i = 0; i < 10; i++)
			{
				if (ok >= 2) return;
				Thread.Sleep(10);
			}
			ok.Should().Be.GreaterThanOrEqualTo(2);
		}

		private static void dummy() { }
	}
}
