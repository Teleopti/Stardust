using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.TestCommon.FakeData;


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
		private ISkillStaffPeriod[] _skillStaffPeriods;
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
			_skillStaffPeriods = new []{_skillStaffPeriod1, _skillStaffPeriod2};
			_dateTimePeriods = new List<DateTimePeriod> {new DateTimePeriod()};
		}

		[Test]
		public void ShouldSetIntraIntervalIssuesWhenMinLessThenHalfMax()
		{
			var samples1 = new List<int> {1000, 2001};
			var samples2 = new List<int> { 1, 2 };

			using (_mock.Record())
			{
				Expect.Call(_skillDay.SkillStaffPeriodCollection).Return(_skillStaffPeriods);
				Expect.Call(_skillDay.Skill).Return(_skill);
				Expect.Call(_intraIntervalFinder.FindForInterval(_skillStaffPeriod1.Period, _resourceCalculationDataContainer, _skill)).Return(_dateTimePeriods);
				Expect.Call(_intraIntervalFinder.FindForInterval(_skillStaffPeriod2.Period, _resourceCalculationDataContainer, _skill)).Return(_dateTimePeriods);
				Expect.Call(_countCollector.Collect(_dateTimePeriods, _skillStaffPeriod1.Period)).Return(samples1);
				Expect.Call(_countCollector.Collect(_dateTimePeriods, _skillStaffPeriod2.Period)).Return(samples2);
				Expect.Call(_fullIntervalFinder.FindForInterval(_skillStaffPeriod1.Period, _resourceCalculationDataContainer, _skill,_dateTimePeriods)).Return(0d);
				Expect.Call(_fullIntervalFinder.FindForInterval(_skillStaffPeriod2.Period, _resourceCalculationDataContainer, _skill, _dateTimePeriods)).Return(0d);
			}

			using (_mock.Playback())
			{
				_target.SetIntraIntervalIssues(_skillDay,_resourceCalculationDataContainer, 0.5);
				Assert.IsTrue(_skillStaffPeriod1.HasIntraIntervalIssue);
				Assert.AreEqual(_skillStaffPeriod1.IntraIntervalSamples, samples1);
				Assert.IsFalse(_skillStaffPeriod2.HasIntraIntervalIssue);
				Assert.AreEqual(_skillStaffPeriod2.IntraIntervalSamples, samples2);
			}
		}

		[Test]
		public void ShouldSetDefaultValuesWhenNoIntraIntervalActivities()
		{
			using (_mock.Record())
			{
				Expect.Call(_skillDay.SkillStaffPeriodCollection).Return(_skillStaffPeriods);
				Expect.Call(_skillDay.Skill).Return(_skill);
				Expect.Call(_intraIntervalFinder.FindForInterval(_skillStaffPeriod1.Period, _resourceCalculationDataContainer, _skill)).Return(new List<DateTimePeriod>());
				Expect.Call(_intraIntervalFinder.FindForInterval(_skillStaffPeriod2.Period, _resourceCalculationDataContainer, _skill)).Return(new List<DateTimePeriod>());
			}

			using (_mock.Playback())
			{
				_target.SetIntraIntervalIssues(_skillDay, _resourceCalculationDataContainer, 0.5);
				Assert.IsFalse(_skillStaffPeriod1.HasIntraIntervalIssue);
				Assert.IsFalse(_skillStaffPeriod2.HasIntraIntervalIssue);
				
				Assert.AreEqual(1.0, _skillStaffPeriod1.IntraIntervalValue);
				Assert.AreEqual(1.0, _skillStaffPeriod2.IntraIntervalValue);

				Assert.IsEmpty(_skillStaffPeriod1.IntraIntervalSamples);
				Assert.IsEmpty(_skillStaffPeriod2.IntraIntervalSamples);
			}	
		}

		[Test]
		public void ShouldConsiderFullIntervalActivities()
		{
			using (_mock.Record())
			{
				Expect.Call(_skillDay.SkillStaffPeriodCollection).Return(_skillStaffPeriods);
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
				_target.SetIntraIntervalIssues(_skillDay, _resourceCalculationDataContainer, 0.5);
				Assert.IsFalse(_skillStaffPeriod1.HasIntraIntervalIssue);
				Assert.IsFalse(_skillStaffPeriod2.HasIntraIntervalIssue);
			}	
		}
	}	
}
