using System;
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
		private OvertimeSkillStaffPeriodToSkillIntervalDataMapper _target;
		private IList<ISkillStaffPeriod> _skillStaffPeriodList;
		private ISkillStaffPeriod _skillStaffPeriod1;
		private ISkillStaffPeriod _skillStaffPeriod2;
		private ISkillStaffPeriod _skillStaffPeriod3;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_target = new OvertimeSkillStaffPeriodToSkillIntervalDataMapper();
			_skillStaffPeriod1 = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriod2 = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriod3 = _mock.StrictMock<ISkillStaffPeriod>();
		}

		[Test]
		public void ShouldTestSkillStaffToSkillIntervalMapper()
		{
			_skillStaffPeriodList = new List<ISkillStaffPeriod> { _skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3 };
			var dateTime = new DateTimePeriod(new DateTime(2013, 7, 19, 0, 0, 0, DateTimeKind.Utc),
			                                  new DateTime(2013, 7, 19, 0, 15, 0, DateTimeKind.Utc));
			var dateTime1 = new DateTimePeriod(dateTime.StartDateTime.AddMinutes(15), dateTime.EndDateTime.AddMinutes(15));
			using (_mock.Record())
			{
				Expect.Call(_skillStaffPeriod1.Period).Return(dateTime);
				Expect.Call(_skillStaffPeriod1.RelativeDifference).Return(-8);

				Expect.Call(_skillStaffPeriod2.Period)
					  .Return(dateTime1).Repeat.Twice();
				Expect.Call(_skillStaffPeriod2.RelativeDifference).Return(-9);

				Expect.Call(_skillStaffPeriod3.Period).Return(dateTime);
				Expect.Call(_skillStaffPeriod3.RelativeDifference).Return(-10);
			}
			using (_mock.Playback())
			{
				var returnList = _target.MapSkillIntervalData(_skillStaffPeriodList);
				Assert.AreEqual(returnList.Count, 2);

				Assert.That(returnList[0].Period, Is.EqualTo(dateTime));
				Assert.That(returnList[0].RelativeDifference, Is.EqualTo(-18));

				Assert.That(returnList[1].Period, Is.EqualTo(dateTime1));
				Assert.That(returnList[1].RelativeDifference, Is.EqualTo(-9));
			}
		}
	}
}
