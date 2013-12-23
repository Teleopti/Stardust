﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.SkillInterval
{
	[TestFixture]
	public class CreateSkillIntervalDatasPerActivtyForDateTest
	{
		private MockRepository _mocks;
		private ICreateSkillIntervalDatasPerActivtyForDate _target;
		private ICalculateAggregatedDataForActivtyAndDate _calculateAggregatedDataForActivtyAndDate;
		private ISkillResolutionProvider _skillResolutionProvider;
		private ISkill _skill1;
		private ISkill _skill2;
		private ISkillDay _skillDayForSkill1;
		private ISkillDay _skillDayForSkill2;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_calculateAggregatedDataForActivtyAndDate = _mocks.StrictMock<ICalculateAggregatedDataForActivtyAndDate>();
			_skillResolutionProvider = _mocks.StrictMock<ISkillResolutionProvider>();
			_target = new CreateSkillIntervalDatasPerActivtyForDate(_calculateAggregatedDataForActivtyAndDate, _skillResolutionProvider);
			_skill1 = SkillFactory.CreateSkill("s1");
			_skill2 = SkillFactory.CreateSkill("s1");
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_skillDayForSkill1 = _mocks.StrictMock<ISkillDay>();
			_skillDayForSkill2 = _mocks.StrictMock<ISkillDay>();
		}

		[Test]
		public void ShouldCreateOneListPerSkillIfDifferentActivity()
		{
			var skillList = new List<ISkill> {_skill1, _skill2};
			var skillDayList = new List<ISkillDay> {_skillDayForSkill1, _skillDayForSkill2};
			using (_mocks.Record())
			{
				Expect.Call(_skillResolutionProvider.MinimumResolution(skillList)).Return(15);
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> {new DateOnly()}))
				      .Return(skillDayList);
				Expect.Call(_skillDayForSkill1.Skill).Return(_skill1).Repeat.Any();
				Expect.Call(_skillDayForSkill2.Skill).Return(_skill2).Repeat.Any();
				Expect.Call(_calculateAggregatedDataForActivtyAndDate.CalculateFor(skillDayList, _skill1.Activity, 15))
				      .Return(new List<ISkillIntervalData>());
				Expect.Call(_calculateAggregatedDataForActivtyAndDate.CalculateFor(skillDayList, _skill2.Activity, 15))
					  .Return(new List<ISkillIntervalData>());
			}

			using (_mocks.Playback())
			{
				var result = _target.CreateFor(new DateOnly(), skillList, _schedulingResultStateHolder);
				Assert.AreEqual(2, result.Count);
			}
      
      

		}

	}
}