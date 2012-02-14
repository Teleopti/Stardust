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
			target.Time.Should().Be.GreaterThan(nu.AddSeconds(-1));
			target.Time.Should().Be.LessThan(nu.AddSeconds(1));
		}
	}
}