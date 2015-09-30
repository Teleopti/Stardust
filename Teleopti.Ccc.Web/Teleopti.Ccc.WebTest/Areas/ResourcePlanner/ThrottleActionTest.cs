using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Global;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class ThrottleActionTest
	{
		[Test]
		public void ShouldOnlyAllowOneCallerAtATime()
		{
			var target = new BasicActionThrottler();
			target.Block(ThrottledAction.Forecasting);

			target.IsBlocked(ThrottledAction.Forecasting).Should().Be.True();
		}

		[Test]
		public void ShouldNotAllowTwoBlocking()
		{
			var target = new BasicActionThrottler();
			target.Block(ThrottledAction.Forecasting);
			Assert.Throws<InvalidOperationException>(()=> target.Block(ThrottledAction.Forecasting));
		}

		[Test]
		public async void ShouldNotCancelPauseWithinAllottedTime()
		{
			var target = new BasicActionThrottler();
			var reference = target.Block(ThrottledAction.Forecasting);

			target.Pause(reference, TimeSpan.FromSeconds(5));
			await Task.Delay(TimeSpan.FromMilliseconds(5));
			target.IsBlocked(ThrottledAction.Forecasting).Should().Be.True();
		}

		[Test]
		public async void ShouldCancelPause()
		{
			var target = new BasicActionThrottler();
			var reference = target.Block(ThrottledAction.Forecasting);

			target.Pause(reference,TimeSpan.Zero);
			await Task.Delay(TimeSpan.FromMilliseconds(5));
			target.IsBlocked(ThrottledAction.Forecasting).Should().Be.False();
		}

		[Test]
		public void ShouldReleaseLockWhenFinished()
		{
			var target = new BasicActionThrottler();
			var reference = target.Block(ThrottledAction.Forecasting);

			target.Finish(reference);
			target.IsBlocked(ThrottledAction.Forecasting).Should().Be.False();
		}

		[Test]
		public void ShouldNotBlockOtherActionsWhenBlocked()
		{
			var target = new BasicActionThrottler();
			target.Block(ThrottledAction.Forecasting);

			target.IsBlocked(ThrottledAction.Forecasting).Should().Be.True();
			target.IsBlocked(ThrottledAction.Scheduling).Should().Be.False();
		}

		[Test]
		public void ShouldNotAllowBlockingUnknownAction()
		{
			var target = new BasicActionThrottler();
			Assert.Throws<InvalidOperationException>(()=>target.Block("random action name"));
		}

		[Test]
		public async void ShouldBeBlockedAfterResume()
		{
			var target = new BasicActionThrottler();
			var reference = target.Block(ThrottledAction.Forecasting);

			target.Pause(reference, TimeSpan.FromMilliseconds(5));
			target.Resume(reference);

			await Task.Delay(TimeSpan.FromMilliseconds(5));
			target.IsBlocked(ThrottledAction.Forecasting).Should().Be.True();
		}
	}
}