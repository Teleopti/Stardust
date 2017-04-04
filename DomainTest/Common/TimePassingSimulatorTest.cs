using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class TimePassingSimulatorTest
	{
		[Test]
		public void ShouldKnowIfDayPassed()
		{
			var target = new TimePassingSimulator("2017-04-04".Utc(), "2017-04-05".Utc());
			var pass = false;

			target.IfDayPassed(() => pass = true);

			pass.Should().Be.True();
		}

		[Test]
		public void ShouldKnowIfDayDidntPass()
		{
			var target = new TimePassingSimulator("2017-04-04 00:00:00".Utc(), "2017-04-04 23:59:59".Utc());
			var pass = false;

			target.IfDayPassed(() => pass = true);

			pass.Should().Be.False();
		}

		[Test]
		public void ShouldPassDayIfYearPassed()
		{
			var target = new TimePassingSimulator("2017-04-04".Utc(), "2018-04-04".Utc());
			var pass = false;

			target.IfDayPassed(() => pass = true);

			pass.Should().Be.True();
		}

		[Test]
		public void ShouldPassDayIfMonthPassed()
		{
			var target = new TimePassingSimulator("2017-04-04".Utc(), "2017-05-04".Utc());
			var pass = false;

			target.IfDayPassed(() => pass = true);

			pass.Should().Be.True();
		}

		[Test]
		public void ShouldKnowIfHourPassed()
		{
			var target = new TimePassingSimulator("2017-04-04 12:00".Utc(), "2017-04-04 13:00".Utc());
			var pass = false;

			target.IfHourPassed(() => pass = true);

			pass.Should().Be.True();
		}

		[Test]
		public void ShouldKnowIfHourDidntPass()
		{
			var target = new TimePassingSimulator("2017-04-04 12:00:00".Utc(), "2017-04-04 12:59:59".Utc());
			var pass = false;

			target.IfHourPassed(() => pass = true);

			pass.Should().Be.False();
		}

		[Test]
		public void ShouldPassHourIfDayPassed()
		{
			var target = new TimePassingSimulator("2017-04-04".Utc(), "2017-05-05".Utc());
			var pass = false;

			target.IfHourPassed(() => pass = true);

			pass.Should().Be.True();
		}

		[Test]
		public void ShouldKnowIfMinutePassed()
		{
			var target = new TimePassingSimulator("2017-04-04 12:00".Utc(), "2017-04-04 12:01".Utc());
			var pass = false;

			target.IfMinutePassed(() => pass = true);

			pass.Should().Be.True();
		}

		[Test]
		public void ShouldKnowIfMinuteDidntPass()
		{
			var target = new TimePassingSimulator("2017-04-04 12:00:00".Utc(), "2017-04-04 12:00:59".Utc());
			var pass = false;

			target.IfMinutePassed(() => pass = true);

			pass.Should().Be.False();
		}

		[Test]
		public void ShouldPassMinuteIfHourPassed()
		{
			var target = new TimePassingSimulator("2017-04-04 12:00".Utc(), "2017-05-04 13:00".Utc());
			var pass = false;

			target.IfMinutePassed(() => pass = true);

			pass.Should().Be.True();
		}

	}
}