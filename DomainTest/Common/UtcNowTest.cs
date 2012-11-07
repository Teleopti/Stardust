using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class UtcNowTest
	{
		private IUtcNow _target;

		[SetUp]
		public void Setup()
		{
			_target = new UtcNow();
		}
		[Test]
		public void ShouldBeCurrentUtcTime()
		{
			var nu = DateTime.UtcNow;
			_target.UtcDateTime().Should().Be.GreaterThan(nu.AddSeconds(-1));
			_target.UtcDateTime().Should().Be.LessThan(nu.AddSeconds(1));
		}

		[Test]
		public void ShouldReturnIsExplicitlySet()
		{
			_target.IsExplicitlySet().Should().Be.EqualTo(false);
		}
	}
}