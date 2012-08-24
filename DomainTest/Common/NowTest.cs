using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class NowTest
	{
		private INow target;

		[SetUp]
		public void Setup()
		{
			target = new Now();
		}

		[Test]
		public void ShouldBeCurrentTime()
		{
			var nu = DateTime.Now;
			target.LocalTime().Should().Be.GreaterThan(nu.AddSeconds(-1));
			target.LocalTime().Should().Be.LessThan(nu.AddSeconds(1));
		}

		[Test]
		public void ShouldBeCurrentUtcTime()
		{
			var nu = DateTime.UtcNow;
			target.UtcTime().Should().Be.GreaterThan(nu.AddSeconds(-1));
			target.UtcTime().Should().Be.LessThan(nu.AddSeconds(1));
		}

		[Test]
		public void ShouldReturnCurrentDateAsDateOnly()
		{
			target.Date().Date.Should().Be.EqualTo(DateTime.Now.Date);
		}
	}
}