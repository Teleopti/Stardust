﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation.IntraIntervalAnalyze
{
	[TestFixture]
	public class SkillDayIntraIntervalFinderTest
	{
		private SkillDayIntraIntervalFinder _target;
		private MockRepository _mock;
		private IIntraIntervalFinder _intraIntervalFinder;
		private ISkillDay _skillDay;
		private IResourceCalculationDataContainer _resourceCalculationDataContainer;
		private ISkillStaffPeriod _skillStaffPeriod1;
		private ISkillStaffPeriod _skillStaffPeriod2;
		private ISkill _skill;
		private ISkillActivityCountCollector _countCollector;
		private IList<ISkillStaffPeriod> _skillStaffPeriods;
		private IList<DateTimePeriod> _dateTimePeriods;
		private IFullIntervalFinder _fullIntervalFinder;
		
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_skillDay = _mock.StrictMock<ISkillDay>();
			_resourceCalculationDataContainer = _mock.StrictMock<IResourceCalculationDataContainer>();
			_intraIntervalFinder = _mock.StrictMock<IIntraIntervalFinder>();
			_countCollector = _mock.StrictMock<ISkillActivityCountCollector>();
			_fullIntervalFinder = _mock.StrictMock<IFullIntervalFinder>();
			_target = new SkillDayIntraIntervalFinder(_intraIntervalFinder, _countCollector, _fullIntervalFinder);
			_skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod();
			_skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod();
			_skill = SkillFactory.CreateSkill("skill");
			_skillStaffPeriods = new List<ISkillStaffPeriod>{_skillStaffPeriod1, _skillStaffPeriod2};
			_dateTimePeriods = new List<DateTimePeriod> {new DateTimePeriod()};
		}

		[Test]
		public void ShouldSetIntraIntervalIssuesWhenMinLessThenHalfMax()
		{
			using (_mock.Record())
			{
				Expect.Call(_skillDay.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(_skillStaffPeriods));
				Expect.Call(_skillDay.Skill).Return(_skill);
				Expect.Call(_intraIntervalFinder.FindForInterval(_skillStaffPeriod1.Period, _resourceCalculationDataContainer, _skill)).Return(_dateTimePeriods);
				Expect.Call(_intraIntervalFinder.FindForInterval(_skillStaffPeriod2.Period, _resourceCalculationDataContainer, _skill)).Return(_dateTimePeriods);
				Expect.Call(_countCollector.Collect(_dateTimePeriods, _skillStaffPeriod1.Period)).Return(new List<int> { 1000, 2001 });
				Expect.Call(_countCollector.Collect(_dateTimePeriods, _skillStaffPeriod2.Period)).Return(new List<int> { 1, 2 });
				Expect.Call(_fullIntervalFinder.FindForInterval(_skillStaffPeriod1.Period, _resourceCalculationDataContainer, _skill,_dateTimePeriods)).Return(0d);
				Expect.Call(_fullIntervalFinder.FindForInterval(_skillStaffPeriod2.Period, _resourceCalculationDataContainer, _skill, _dateTimePeriods)).Return(0d);
			}

			using (_mock.Playback())
			{
				_target.SetIntraIntervalIssues(_skillDay,_resourceCalculationDataContainer);
				Assert.IsTrue(_skillStaffPeriod1.HasIntraIntervalIssue);
				Assert.IsFalse(_skillStaffPeriod2.HasIntraIntervalIssue);
			}
		}

		[Test]
		public void ShouldConsiderFullIntervalActivities()
		{
			using (_mock.Record())
			{
				Expect.Call(_skillDay.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(_skillStaffPeriods));
				Expect.Call(_skillDay.Skill).Return(_skill);
				Expect.Call(_intraIntervalFinder.FindForInterval(_skillStaffPeriod1.Period, _resourceCalculationDataContainer, _skill)).Return(_dateTimePeriods);
				Expect.Call(_intraIntervalFinder.FindForInterval(_skillStaffPeriod2.Period, _resourceCalculationDataContainer, _skill)).Return(_dateTimePeriods);
				Expect.Call(_countCollector.Collect(_dateTimePeriods, _skillStaffPeriod1.Period)).Return(new List<int> { 1000, 2001 });
				Expect.Call(_countCollector.Collect(_dateTimePeriods, _skillStaffPeriod2.Period)).Return(new List<int> { 1, 1 });
				Expect.Call(_fullIntervalFinder.FindForInterval(_skillStaffPeriod1.Period, _resourceCalculationDataContainer, _skill, _dateTimePeriods)).Return(1d);
				Expect.Call(_fullIntervalFinder.FindForInterval(_skillStaffPeriod2.Period, _resourceCalculationDataContainer, _skill, _dateTimePeriods)).Return(0d);
			}

			using (_mock.Playback())
			{
				_target.SetIntraIntervalIssues(_skillDay, _resourceCalculationDataContainer);
				Assert.IsFalse(_skillStaffPeriod1.HasIntraIntervalIssue);
				Assert.IsFalse(_skillStaffPeriod2.HasIntraIntervalIssue);
			}	
		}
	}	
}
