using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.IntraIntervalOptimization
{
	[TestFixture]
	public class IntraIntervalIssuesTest
	{
		private MockRepository _mock;
		private IntraIntervalIssues _target;
		private ISkillDayIntraIntervalIssueExtractor _skillDayIntraIntervalIssueExtractor;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private ISkill _skill;
		private DateOnly _dateOnly;
		private IList<ISkillDay> _skillDaysBefore;
		private IList<ISkillDay> _skillDays;
		private IList<ISkillDay> _skillDaysAfter;
		private ISkillDay _skillDayBefore;
		private ISkillDay _skillDay;
		private ISkillDay _skillDayAfter;
		private IList<ISkillStaffPeriod> _skillStaffPeriodsBefore;
		private IList<ISkillStaffPeriod> _skillStaffPeriods;
		private IList<ISkillStaffPeriod> _skillStaffPeriodsAfter;
		private ISkillStaffPeriod _skillStaffPeriodBefore;
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
			_skillDayBefore = _mock.StrictMock<ISkillDay>();
			_skillDay = _mock.StrictMock<ISkillDay>();
			_skillDayAfter = _mock.StrictMock<ISkillDay>();
			_skillDaysBefore = new List<ISkillDay>{_skillDayBefore};
			_skillDays = new List<ISkillDay> { _skillDay };
			_skillDaysAfter = new List<ISkillDay> { _skillDayAfter };
			_skillStaffPeriodBefore = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriod = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriodAfter = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriodsBefore = new List<ISkillStaffPeriod>{_skillStaffPeriodBefore};
			_skillStaffPeriods = new List<ISkillStaffPeriod> { _skillStaffPeriod };
			_skillStaffPeriodsAfter = new List<ISkillStaffPeriod> { _skillStaffPeriodAfter };
			_target = new IntraIntervalIssues(_skillDayIntraIntervalIssueExtractor);
		}

		[Test]
		public void ShouldCalculateAndClearIssues()
		{
			using (_mock.Record())
			{
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly.AddDays(-1) })).Return(_skillDaysBefore);
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> {_dateOnly})).Return(_skillDays);
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly.AddDays(1) })).Return(_skillDaysAfter);

				Expect.Call(_skillDayIntraIntervalIssueExtractor.Extract(_skillDaysBefore, _skill)).Return(_skillStaffPeriodsBefore);
				Expect.Call(_skillDayIntraIntervalIssueExtractor.Extract(_skillDays, _skill)).Return(_skillStaffPeriods);
				Expect.Call(_skillDayIntraIntervalIssueExtractor.Extract(_skillDaysAfter, _skill)).Return(_skillStaffPeriodsAfter);
			}

			using (_mock.Playback())
			{
				_target.CalculateIssues(_schedulingResultStateHolder, _skill, _dateOnly);
				Assert.AreEqual(1, _target.IssuesOnDayBefore.Count);
				Assert.AreEqual(1, _target.IssuesOnDay.Count);
				Assert.AreEqual(1, _target.IssuesOnDayAfter.Count);

				Assert.AreEqual(_skillStaffPeriodBefore, _target.IssuesOnDayBefore[0]);
				Assert.AreEqual(_skillStaffPeriod, _target.IssuesOnDay[0]);
				Assert.AreEqual(_skillStaffPeriodAfter, _target.IssuesOnDayAfter[0]);

				_target.Clear();
				Assert.AreEqual(0, _target.IssuesOnDayBefore.Count);
				Assert.AreEqual(0, _target.IssuesOnDay.Count);
				Assert.AreEqual(0, _target.IssuesOnDayAfter.Count);
			}
		}

		[Test]
		public void ShouldCloneIssues()
		{
			using (_mock.Record())
			{
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly.AddDays(-1) })).Return(_skillDaysBefore);
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly })).Return(_skillDays);
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly.AddDays(1) })).Return(_skillDaysAfter);

				Expect.Call(_skillDayIntraIntervalIssueExtractor.Extract(_skillDaysBefore, _skill)).Return(_skillStaffPeriodsBefore);
				Expect.Call(_skillDayIntraIntervalIssueExtractor.Extract(_skillDays, _skill)).Return(_skillStaffPeriods);
				Expect.Call(_skillDayIntraIntervalIssueExtractor.Extract(_skillDaysAfter, _skill)).Return(_skillStaffPeriodsAfter);

				Expect.Call(_skillStaffPeriodBefore.NoneEntityClone()).Return(_skillStaffPeriodBefore);
				Expect.Call(_skillStaffPeriod.NoneEntityClone()).Return( _skillStaffPeriod);
				Expect.Call(_skillStaffPeriodAfter.NoneEntityClone()).Return(_skillStaffPeriodAfter);
			}

			using (_mock.Playback())
			{
				_target.CalculateIssues(_schedulingResultStateHolder, _skill, _dateOnly);
				var clone = _target.Clone();

				Assert.AreEqual(1, clone.IssuesOnDayBefore.Count);
				Assert.AreEqual(1, clone.IssuesOnDay.Count);
				Assert.AreEqual(1, clone.IssuesOnDayAfter.Count);

				Assert.AreEqual(_skillStaffPeriodBefore, clone.IssuesOnDayBefore[0]);
				Assert.AreEqual(_skillStaffPeriod, clone.IssuesOnDay[0]);
				Assert.AreEqual(_skillStaffPeriodAfter, clone.IssuesOnDayAfter[0]);
			}
		}
	}
}
