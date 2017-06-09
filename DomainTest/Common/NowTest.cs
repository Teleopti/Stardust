using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
	/// <summary>
	/// "Modified now" part is tested in ModifyNowTest
	/// </summary>
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
			target.ServerDateTime_DontUse().Should().Be.GreaterThan(nu.AddSeconds(-1));
			target.ServerDateTime_DontUse().Should().Be.LessThan(nu.AddSeconds(1));
		}

		[Test]
		public void ShouldBeCurrentUtcTime()
		{
			var nu = DateTime.UtcNow;
			target.UtcDateTime().Should().Be.GreaterThan(nu.AddSeconds(-1));
			target.UtcDateTime().Should().Be.LessThan(nu.AddSeconds(1));
		}

		[Test]
		public void ShouldReturnCurrentDateAsDateOnly()
		{
			target.ServerDate_DontUse().Date.Should().Be.EqualTo(DateTime.Now.Date);
		}
	}
}