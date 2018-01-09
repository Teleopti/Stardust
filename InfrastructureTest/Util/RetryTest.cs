using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Util;

namespace Teleopti.Ccc.InfrastructureTest.Util
{
	class RetryTest
	{
		[Test]
		public void ShouldFinishWithoutRetry()
		{
			var executed = false;
			Retry.Handle<InvalidOperationException>()
				.Do(() => { executed = true;});
			executed.Should().Be.True();
		}

		[Test]
		public void ShouldFinishAfterOneRetry()
		{
			var executed = false;
			var iteration = 0;
			Retry.Handle<InvalidOperationException>()
				.WaitAndRetry(TimeSpan.Zero)
				.Do(() =>
				{
					iteration++;
					if (iteration == 1) throw new InvalidOperationException("For test");
					executed = true;
				});
			executed.Should().Be.True();
		}

		[Test]
		public void ShouldThrowDirectlyForOtherExceptionType()
		{
			var executed = false;
			var iteration = 0;
			Assert.Throws<ArgumentException>(() => Retry.Handle<InvalidOperationException>()
				.WaitAndRetry(TimeSpan.Zero)
				.Do(() =>
				{
					iteration++;
					if (iteration == 1) throw new ArgumentException("For test");
					executed = true;
				}));
			iteration.Should().Be.EqualTo(1);
			executed.Should().Be.False();
		}

		[Test]
		public void ShouldWaitBetweenRetries()
		{
			var executed = false;
			var iteration = 0;
			var timeBefore = DateTime.UtcNow;
			DateTime timeAfter = timeBefore;
			Retry.Handle<InvalidOperationException>()
				.WaitAndRetry(TimeSpan.FromMilliseconds(1))
				.WaitAndRetry(TimeSpan.FromMilliseconds(2))
				.Do(() =>
				{
					iteration++;
					if (iteration <= 2) throw new InvalidOperationException("For test");
					executed = true;
					timeAfter = DateTime.UtcNow;
				});
			executed.Should().Be.True();
			timeAfter.Subtract(timeBefore).TotalMilliseconds.Should().Be.GreaterThanOrEqualTo(3);
		}

		[Test]
		public void ShouldThrowAfterRetries()
		{
			var executed = false;
			Assert.Throws<InvalidOperationException>(()=> Retry.Handle<InvalidOperationException>()
				.WaitAndRetry(TimeSpan.FromMilliseconds(1))
				.Do(() => throw new InvalidOperationException("For test")));
			executed.Should().Be.False();
		}
	}
}