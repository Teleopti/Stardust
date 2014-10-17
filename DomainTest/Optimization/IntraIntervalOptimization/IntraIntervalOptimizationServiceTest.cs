using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.IntraIntervalOptimization
{
	[TestFixture]
	public class IntraIntervalOptimizationServiceTest
	{
		private IntraIntervalOptimizationService _target;
		private MockRepository _mock;
		private ISkillDayIntraIntervalIssueExtractor _skillDayIntraIntervalIssueExtractor;
		private IScheduleDayIntraIntervalIssueExtractor _scheduleDayIntraIntervalIssueExtractor;
		private IIntraIntervalOptimizer _intraIntervalOptimizer;
		private ISchedulingOptions _schedulingOptions;
		private DateOnlyPeriod _dateOnlyPeriod;
		private IScheduleDay _scheduleDay;
		private List<IScheduleDay> _scheduleDays;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IList<IScheduleMatrixPro> _allScheduleMatrixPros;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private IPerson _person;
		private ISkill _skill;
		private IList<ISkill> _skills;
		private ISkillDay _skillDay;
		private IList<ISkillDay> _skillDays;
		private DateOnly _dateOnly;
		private ISkillStaffPeriod _skillStaffPeriod;
		private ISkillStaffPeriod _skillStaffPeriodAfter;
		private IList<ISkillStaffPeriod> _issuesBefore;
		private IScheduleDictionary _scheduleDictionary;
		private IList<ISkillStaffPeriod> _issuesAfter;
		
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_skillDayIntraIntervalIssueExtractor = _mock.StrictMock<ISkillDayIntraIntervalIssueExtractor>();
			_scheduleDayIntraIntervalIssueExtractor = _mock.StrictMock<IScheduleDayIntraIntervalIssueExtractor>();
			_intraIntervalOptimizer = _mock.StrictMock<IIntraIntervalOptimizer>();
			_target = new IntraIntervalOptimizationService(_skillDayIntraIntervalIssueExtractor, _scheduleDayIntraIntervalIssueExtractor, _intraIntervalOptimizer);

			_schedulingOptions = new SchedulingOptions();
			_dateOnly = new DateOnly(2014, 1, 1);
			_dateOnlyPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_scheduleDays = new List<IScheduleDay>{_scheduleDay};
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
			_allScheduleMatrixPros = new List<IScheduleMatrixPro>();
			_rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_resourceCalculateDelayer = _mock.StrictMock<IResourceCalculateDelayer>();
			_person = PersonFactory.CreatePerson("person");
			_skill = SkillFactory.CreateSkill("skill");
			_skills = new List<ISkill>{_skill};
			_skillDay = _mock.StrictMock<ISkillDay>();
			_skillDays = new List<ISkillDay>{_skillDay};
			_skillStaffPeriod = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriodAfter = _mock.StrictMock<ISkillStaffPeriod>();
			_issuesBefore = new List<ISkillStaffPeriod> { _skillStaffPeriod };
			_issuesAfter = new List<ISkillStaffPeriod>{_skillStaffPeriodAfter};
			_scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
		}

		[Test]
		public void ShouldExecute()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_schedulingResultStateHolder.Skills).Return(_skills);
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> {_dateOnly})).Return(_skillDays);
				Expect.Call(_skillDayIntraIntervalIssueExtractor.Extract(_skillDays, _skill)).Return(_issuesBefore);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDayIntraIntervalIssueExtractor.Extract(_scheduleDictionary, _dateOnly, _issuesBefore, _skill)).Return(_scheduleDays);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_intraIntervalOptimizer.Optimize(_schedulingOptions, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issuesBefore)).Return(_issuesAfter);
				Expect.Call(_skillStaffPeriodAfter.NoneEntityClone()).Return(_skillStaffPeriodAfter);
			}

			using (_mock.Playback())
			{
				_target.Execute(_schedulingOptions,_dateOnlyPeriod,_scheduleDays, _schedulingResultStateHolder, _allScheduleMatrixPros,_rollbackService, _resourceCalculateDelayer);
				Assert.AreEqual(_skillStaffPeriodAfter, _issuesBefore[0]);
			}
		}

		[Test]
		public void ShouldSkipMaxSeatSkills()
		{
			var maxSeatSkill = SkillFactory.CreateSiteSkill("maxSeat");
			_skills = new List<ISkill>{maxSeatSkill};

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_schedulingResultStateHolder.Skills).Return(_skills);
			}

			using (_mock.Playback())
			{
				_target.Execute(_schedulingOptions, _dateOnlyPeriod, _scheduleDays, _schedulingResultStateHolder, _allScheduleMatrixPros, _rollbackService, _resourceCalculateDelayer);
			}
		}

		[Test]
		public void ShouldSkipWhenNoIssuesFromStart()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_schedulingResultStateHolder.Skills).Return(_skills);
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly })).Return(_skillDays);
				Expect.Call(_skillDayIntraIntervalIssueExtractor.Extract(_skillDays, _skill)).Return(new List<ISkillStaffPeriod>());
			}

			using (_mock.Playback())
			{
				_target.Execute(_schedulingOptions, _dateOnlyPeriod, _scheduleDays, _schedulingResultStateHolder, _allScheduleMatrixPros, _rollbackService, _resourceCalculateDelayer);
			}
		}

		[Test]
		public void ShouldStopWhenIssuesSolved()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_schedulingResultStateHolder.Skills).Return(_skills);
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly })).Return(_skillDays);
				Expect.Call(_skillDayIntraIntervalIssueExtractor.Extract(_skillDays, _skill)).Return(_issuesBefore);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDayIntraIntervalIssueExtractor.Extract(_scheduleDictionary, _dateOnly, _issuesBefore, _skill)).Return(_scheduleDays);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_intraIntervalOptimizer.Optimize(_schedulingOptions, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issuesBefore)).Return(new List<ISkillStaffPeriod>());
			}

			using (_mock.Playback())
			{
				_target.Execute(_schedulingOptions, _dateOnlyPeriod, _scheduleDays, _schedulingResultStateHolder, _allScheduleMatrixPros, _rollbackService, _resourceCalculateDelayer);
				Assert.IsEmpty(_issuesBefore);
			}
		}
	}
}
