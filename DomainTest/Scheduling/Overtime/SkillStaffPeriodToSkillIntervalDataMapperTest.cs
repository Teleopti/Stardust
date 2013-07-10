using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[TestFixture]
	public class SkillStaffPeriodToSkillIntervalDataMapperTest
	{
		private MockRepository _mock;
		private SkillStaffPeriodToSkillIntervalDataMapper _target;
		private IList<ISkillStaffPeriod> _skillStaffPeriodList;
		private ISkillStaffPeriod _skillStaffPeriod1;
		private ISkillStaffPeriod _skillStaffPeriod2;
		private ISkillStaffPeriod _skillStaffPeriod3;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_target = new SkillStaffPeriodToSkillIntervalDataMapper();
			_skillStaffPeriod1 = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriod2 = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriod3 = _mock.StrictMock<ISkillStaffPeriod>();
		}

		[Test]
		public void ShouldTestSkillStaffToSkillIntervalMapper()
		{
			_skillStaffPeriodList = new List<ISkillStaffPeriod> { _skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3 };
			var dateTime = new DateTimePeriod();
			using (_mock.Record())
			{
				Expect.Call(_skillStaffPeriod1.Period).Return(dateTime);
				Expect.Call(_skillStaffPeriod1.RelativeDifference).Return(10);

				Expect.Call(_skillStaffPeriod2.Period).Return(dateTime);
				Expect.Call(_skillStaffPeriod2.RelativeDifference).Return(22);

				Expect.Call(_skillStaffPeriod3.Period).Return(dateTime);
				Expect.Call(_skillStaffPeriod3.RelativeDifference).Return(11);
			}
			using (_mock.Playback())
			{
				var returnList = _target.MapSkillIntervalData(_skillStaffPeriodList);
				Assert.AreEqual(returnList.Count, 3);

				Assert.That(returnList[0].Period, Is.EqualTo(dateTime));
				Assert.That(returnList[0].RelativeDifference, Is.EqualTo(10));

				Assert.That(returnList[1].Period, Is.EqualTo(dateTime));
				Assert.That(returnList[1].RelativeDifference, Is.EqualTo(22));

				Assert.That(returnList[2].Period, Is.EqualTo(dateTime));
				Assert.That(returnList[2].RelativeDifference, Is.EqualTo(11));

			}
		}
	}
}
