using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[TestFixture]
	public class SkillIntervalDataTest
	{
		private IOvertimeSkillIntervalData _target;
		private DateTimePeriod _dtp;

		[SetUp]
		public void Setup()
		{
			_dtp = new DateTimePeriod(2012, 11, 28, 2012, 11, 28);
			_target = new OvertimeSkillIntervalData(_dtp, -0.3);
		}

		[Test]
		public void ShouldContainRelativeDifference()
		{
			Assert.AreEqual(-0.3, _target.RelativeDifference);
		}
	}
}
