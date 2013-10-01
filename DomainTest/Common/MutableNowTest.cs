using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class MutableNowTest
	{
		[Test]
		public void CanResetUsingRealTime()
		{
			var target = new MutableNow();
			var nu = DateTime.UtcNow;
			target.Mutate(nu.AddYears(2));
			target.Mutate(null);

			target.UtcDateTime().Should().Be.IncludedIn(nu.AddMinutes(-1), nu.AddMinutes(1));
		}

		[Test]
		public void ShouldBeExplicitlySet()
		{
			var target = new MutableNow();
			target.Mutate(DateTime.Now);
			target.IsMutated().Should().Be.True();
		}

		[Test]
		public void ShouldNotBeExplicitlySet()
		{
			var target = new MutableNow();
			target.IsMutated().Should().Be.False();
		}
	}
}