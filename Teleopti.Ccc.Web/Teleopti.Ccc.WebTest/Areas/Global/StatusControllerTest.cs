using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Global;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class StatusControllerTest
	{
		private const string forecasting = "Forecasting";
			
		[Test]
		public void ShouldReturnRunningStateWhenNotBlocked()
		{
			var target = new StatusController(new BasicActionThrottler());
			var result = target.Status(forecasting);
			((bool)result.IsRunning).Should().Be.False();
		}

		[Test]
		public void ShouldReturnRunningStateWhenBlocked()
		{
			var basicActionThrottler = new BasicActionThrottler();
			var target = new StatusController(basicActionThrottler);
			basicActionThrottler.Block(forecasting);
			var result = target.Status(forecasting);
			((bool)result.IsRunning).Should().Be.True();
		}
	}
}