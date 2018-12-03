using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.TestCommon.FakeData;



namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.SkillInterval
{
	[TestFixture]
	public class CreateSkillIntervalDatasPerActivtyForDateTest
	{
		private MockRepository _mocks;
		private ICreateSkillIntervalDatasPerActivtyForDate _target;
		private ICalculateAggregatedDataForActivtyAndDate _calculateAggregatedDataForActivtyAndDate;
		private ISkill _skill1;
		private ISkill _skill2;
		private ISkillDay _skillDayForSkill1;
		private ISkillDay _skillDayForSkill2;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_calculateAggregatedDataForActivtyAndDate = _mocks.StrictMock<ICalculateAggregatedDataForActivtyAndDate>();
			_target = new CreateSkillIntervalDatasPerActivtyForDate(_calculateAggregatedDataForActivtyAndDate);
			_skill1 = SkillFactory.CreateSkill("s1");
			_skill2 = SkillFactory.CreateSkill("s1");
			_skillDayForSkill1 = _mocks.StrictMock<ISkillDay>();
			_skillDayForSkill2 = _mocks.StrictMock<ISkillDay>();
		}

		[Test]
		public void ShouldCreateOneListPerSkillIfDifferentActivity()
		{
			var date = DateOnly.Today;
			var skillList = new List<ISkill> {_skill1, _skill2};
			var skillDayList = new List<ISkillDay> {_skillDayForSkill1, _skillDayForSkill2};
			var skillIntervalData1 =
				new SkillIntervalData(
					new DateTimePeriod(new DateTime(2013, 10, 02, 16, 0, 0, DateTimeKind.Utc),
									   new DateTime(2013, 10, 02, 17, 0, 0, DateTimeKind.Utc)), 6, 6, 0, null, null);
			var skillIntervalData2 =
				new SkillIntervalData(
					new DateTimePeriod(new DateTime(2013, 10, 02, 16, 0, 0, DateTimeKind.Utc),
									   new DateTime(2013, 10, 02, 17, 0, 0, DateTimeKind.Utc)), 6, 6, 0, null, null);
			using (_mocks.Record())
			{ 
				Expect.Call(_skillDayForSkill1.Skill).Return(_skill1).Repeat.Any();
				Expect.Call(_skillDayForSkill1.CurrentDate).Return(date).Repeat.Any();
				Expect.Call(_skillDayForSkill2.Skill).Return(_skill2).Repeat.Any();
				Expect.Call(_skillDayForSkill2.CurrentDate).Return(date).Repeat.Any();
				Expect.Call(_calculateAggregatedDataForActivtyAndDate.CalculateFor(skillDayList, _skill1.Activity, 15))
					  .Return(new List<ISkillIntervalData> { skillIntervalData1 });
				Expect.Call(_calculateAggregatedDataForActivtyAndDate.CalculateFor(skillDayList, _skill2.Activity, 15))
					  .Return(new List<ISkillIntervalData> { skillIntervalData2});
			}

			using (_mocks.Playback())
			{
				var result = _target.CreateFor(date, skillList, skillDayList);
				Assert.AreEqual(2, result.Count);
			}
      
      

		}
        
	}
}