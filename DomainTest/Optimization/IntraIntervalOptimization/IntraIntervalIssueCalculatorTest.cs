using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization.IntraIntervalOptimization
{
	[TestFixture]
	public class IntraIntervalIssueCalculatorTest
	{
		private MockRepository _mock;
		private ISkillDayIntraIntervalIssueExtractor _skillDayIntraIntervalIssueExtractor;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private ISkill _skill;
		private DateOnly _dateOnly;
		private IntraIntervalIssueCalculator _target;
		private ISkillDay _skillDay;
		private ISkillDay _skillDayAfter;
		private IList<ISkillDay> _skillDays;
		private IList<ISkillDay> _skillDaysAfter;
		private IList<ISkillStaffPeriod> _skillStaffPeriods;
		private IList<ISkillStaffPeriod> _skillStaffPeriodsAfter;
		private ISkillStaffPeriod _skillStaffPeriod;
		private ISkillStaffPeriod _skillStaffPeriodAfter;
		
		
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_skillDayIntraIntervalIssueExtractor = _mock.StrictMock<ISkillDayIntraIntervalIssueExtractor>();
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
			_skill = SkillFactory.CreateSkill("skill");
			_dateOnly = new DateOnly(2014, 1, 1);
			_skillDay = _mock.StrictMock<ISkillDay>();
			_skillDayAfter = _mock.StrictMock<ISkillDay>();
			_skillDays = new List<ISkillDay> { _skillDay };
			_skillDaysAfter = new List<ISkillDay> { _skillDayAfter };
			_skillStaffPeriod = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriodAfter = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriods = new List<ISkillStaffPeriod> { _skillStaffPeriod };
			_skillStaffPeriodsAfter = new List<ISkillStaffPeriod> { _skillStaffPeriodAfter };
			_target = new IntraIntervalIssueCalculator(_skillDayIntraIntervalIssueExtractor);	
		}

		[Test]
		public void ShouldCalculate()
		{
			using (_mock.Record())
			{
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly })).Return(_skillDays);
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly.AddDays(1) })).Return(_skillDaysAfter);

				Expect.Call(_skillDayIntraIntervalIssueExtractor.ExtractOnIssues(_skillDays, _skill)).Return(_skillStaffPeriods);
				Expect.Call(_skillDayIntraIntervalIssueExtractor.ExtractOnIssues(_skillDaysAfter, _skill)).Return(_skillStaffPeriodsAfter);
			}

			using (_mock.Playback())
			{
				var result = _target.CalculateIssues(_schedulingResultStateHolder, _skill, _dateOnly);
				Assert.AreEqual(_skillStaffPeriods, result.IssuesOnDay);
				Assert.AreEqual(_skillStaffPeriodsAfter, result.IssuesOnDayAfter);
			}
		}
	}
}
