using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.IntraIntervalOptimization
{
	[TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	public class IntraIntervalOptimizationServiceTest
	{
		private IntraIntervalOptimizationService _target;
		private MockRepository _mock;
		private IScheduleDayIntraIntervalIssueExtractor _scheduleDayIntraIntervalIssueExtractor;
		private IIntraIntervalOptimizer _intraIntervalOptimizer;
		private SchedulingOptions _schedulingOptions;
		private IOptimizationPreferences _optimizationPreferences;
		private DateOnlyPeriod _dateOnlyPeriod;
		private IScheduleDay _scheduleDay;
		private List<IScheduleDay> _scheduleDays;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IList<IScheduleMatrixPro> _allScheduleMatrixPros;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private IPerson _person;
		private ISkill _skill;
		private ISkill[] _skills;
		private DateOnly _dateOnly;
		private ISkillStaffPeriod _skillStaffPeriodAfter;
		private IIntraIntervalIssues _issuesBefore;
		private IScheduleDictionary _scheduleDictionary;
		private IList<ISkillStaffPeriod> _skillStaffPeriodIssuesAfter;
		private IIntraIntervalIssueCalculator _intervalIssueCalculator;
		private List<ISkillStaffPeriod> _skillStaffPeriodIssuesBefore;
		private ISkillStaffPeriod _skillStaffPeriodBefore;
		private IntraIntervalIssues _issuesAfter;
		private IPersonAssignment _personAssignment;
		private ISchedulingOptionsCreator _schedulingOptionsCreator;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_scheduleDayIntraIntervalIssueExtractor = _mock.StrictMock<IScheduleDayIntraIntervalIssueExtractor>();
			_intraIntervalOptimizer = _mock.StrictMock<IIntraIntervalOptimizer>();
			_intervalIssueCalculator = _mock.StrictMock<IIntraIntervalIssueCalculator>();
			_schedulingOptionsCreator = _mock.StrictMock<ISchedulingOptionsCreator>();
			_target = new IntraIntervalOptimizationService(_scheduleDayIntraIntervalIssueExtractor, _intraIntervalOptimizer,
				_intervalIssueCalculator, _schedulingOptionsCreator);

			_schedulingOptions = new SchedulingOptions();
			_optimizationPreferences = new OptimizationPreferences();
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
			_skills = new []{_skill};
			_skillStaffPeriodAfter = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriodBefore = _mock.StrictMock<ISkillStaffPeriod>();
			_issuesBefore = new IntraIntervalIssues();
			_issuesAfter = new IntraIntervalIssues();
			_skillStaffPeriodIssuesAfter = new List<ISkillStaffPeriod>{_skillStaffPeriodAfter};
			_skillStaffPeriodIssuesBefore = new List<ISkillStaffPeriod> { _skillStaffPeriodBefore };
			_scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
			_personAssignment = _mock.StrictMock<IPersonAssignment>();
		}

		[Test]
		public void ShouldExecute()
		{
			_issuesBefore.IssuesOnDay = _skillStaffPeriodIssuesBefore;
			_issuesBefore.IssuesOnDayAfter = _skillStaffPeriodIssuesBefore;
			_issuesAfter.IssuesOnDay = _skillStaffPeriodIssuesAfter;
			_issuesAfter.IssuesOnDayAfter = _skillStaffPeriodIssuesAfter;
			var dateTimePeriod = new DateTimePeriod(2014, 1, 1, 10, 2014, 1, 1, 12);

			using (_mock.Record())
			{
				Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
				Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.Skills).Return(_skills);
				Expect.Call(_intervalIssueCalculator.CalculateIssues(_schedulingResultStateHolder, _skill, _dateOnly)).Return(_issuesBefore);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				
				Expect.Call(_scheduleDayIntraIntervalIssueExtractor.Extract(_scheduleDictionary, _dateOnly, _issuesBefore.IssuesOnDay)).Return(_scheduleDays);
				Expect.Call(_scheduleDayIntraIntervalIssueExtractor.Extract(_scheduleDictionary, _dateOnly, _issuesBefore.IssuesOnDayAfter)).Return(_scheduleDays);
				
				Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_personAssignment.Period).Return(dateTimePeriod);
				Expect.Call(_skillStaffPeriodBefore.Period).Return(dateTimePeriod);
				Expect.Call(_intraIntervalOptimizer.Optimize(_schedulingOptions, _optimizationPreferences, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issuesBefore, false)).Return(_issuesAfter);


				Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_personAssignment.Period).Return(dateTimePeriod);
				Expect.Call(_skillStaffPeriodAfter.Period).Return(dateTimePeriod);
				Expect.Call(_intraIntervalOptimizer.Optimize(_schedulingOptions, _optimizationPreferences, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issuesAfter, true)).Return(_issuesAfter);
			}

			using (_mock.Playback())
			{
				_target.Execute(_optimizationPreferences, _dateOnlyPeriod, _scheduleDays, _schedulingResultStateHolder, _allScheduleMatrixPros, _rollbackService, _resourceCalculateDelayer);
			}
		}

		[Test]
		public void ShouldSkipMaxSeatEmailBackofficeSkills()
		{
			var maxSeatSkill = SkillFactory.CreateSiteSkill("maxSeat");
			var emailSkill = SkillFactory.CreateSkill("email", SkillTypeFactory.CreateSkillTypeEmail(), 15, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone, TimeSpan.FromHours(1));
			var backOfficeSkill = SkillFactory.CreateSkill("email", SkillTypeFactory.CreateSkillTypeBackoffice(), 15, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone, TimeSpan.FromHours(1));
			_skills = new [] { maxSeatSkill, emailSkill, backOfficeSkill };

			using (_mock.Record())
			{
				Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_schedulingResultStateHolder.Skills).Return(_skills);
			}

			using (_mock.Playback())
			{
				_target.Execute(_optimizationPreferences, _dateOnlyPeriod, _scheduleDays, _schedulingResultStateHolder, _allScheduleMatrixPros, _rollbackService, _resourceCalculateDelayer);
			}
		}

		[Test]
		public void ShouldSkipWhenNoIssuesFromStart()
		{
			using (_mock.Record())
			{
				Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_schedulingResultStateHolder.Skills).Return(_skills);
				Expect.Call(_intervalIssueCalculator.CalculateIssues(_schedulingResultStateHolder, _skill, _dateOnly)).Return(_issuesBefore);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDayIntraIntervalIssueExtractor.Extract(_scheduleDictionary, _dateOnly, _issuesBefore.IssuesOnDay)).Return(new List<IScheduleDay>());
				Expect.Call(_scheduleDayIntraIntervalIssueExtractor.Extract(_scheduleDictionary, _dateOnly, _issuesBefore.IssuesOnDayAfter)).Return(new List<IScheduleDay>());
			}

			using (_mock.Playback())
			{
				_target.Execute(_optimizationPreferences, _dateOnlyPeriod, _scheduleDays, _schedulingResultStateHolder, _allScheduleMatrixPros, _rollbackService, _resourceCalculateDelayer);
			}
		}

		[Test]
		public void ShouldSkipWhenNotAffectingIssueOnDay()
		{
			_issuesBefore.IssuesOnDay = _skillStaffPeriodIssuesBefore;
			_issuesBefore.IssuesOnDayAfter = _skillStaffPeriodIssuesBefore;
			_issuesAfter.IssuesOnDay = _skillStaffPeriodIssuesAfter;
			_issuesAfter.IssuesOnDayAfter = _skillStaffPeriodIssuesAfter;
			var dateTimePeriod = new DateTimePeriod(2014, 1, 1, 10, 2014, 1, 1, 12);

			using (_mock.Record())
			{
				Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
				Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.Skills).Return(_skills);
				Expect.Call(_intervalIssueCalculator.CalculateIssues(_schedulingResultStateHolder, _skill, _dateOnly)).Return(_issuesBefore);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);

				Expect.Call(_scheduleDayIntraIntervalIssueExtractor.Extract(_scheduleDictionary, _dateOnly, _issuesBefore.IssuesOnDay)).Return(_scheduleDays);
				Expect.Call(_scheduleDayIntraIntervalIssueExtractor.Extract(_scheduleDictionary, _dateOnly, _issuesBefore.IssuesOnDayAfter)).Return(new List<IScheduleDay>());

				Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_personAssignment.Period).Return(dateTimePeriod);
				Expect.Call(_skillStaffPeriodBefore.Period).Return(dateTimePeriod.MovePeriod(TimeSpan.FromHours(5)));
			}

			using (_mock.Playback())
			{
				_target.Execute(_optimizationPreferences, _dateOnlyPeriod, _scheduleDays, _schedulingResultStateHolder, _allScheduleMatrixPros, _rollbackService, _resourceCalculateDelayer);	
			}
		}

		[Test]
		public void ShouldSkipWhenNotAffectingIssueOnDayAfter()
		{
			_issuesBefore.IssuesOnDay = _skillStaffPeriodIssuesBefore;
			_issuesBefore.IssuesOnDayAfter = _skillStaffPeriodIssuesBefore;
			_issuesAfter.IssuesOnDay = _skillStaffPeriodIssuesAfter;
			_issuesAfter.IssuesOnDayAfter = _skillStaffPeriodIssuesAfter;
			var dateTimePeriod = new DateTimePeriod(2014, 1, 1, 10, 2014, 1, 1, 12);

			using (_mock.Record())
			{
				Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
				Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.Skills).Return(_skills);
				Expect.Call(_intervalIssueCalculator.CalculateIssues(_schedulingResultStateHolder, _skill, _dateOnly)).Return(_issuesBefore);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);

				Expect.Call(_scheduleDayIntraIntervalIssueExtractor.Extract(_scheduleDictionary, _dateOnly, _issuesBefore.IssuesOnDay)).Return(new List<IScheduleDay>());
				Expect.Call(_scheduleDayIntraIntervalIssueExtractor.Extract(_scheduleDictionary, _dateOnly, _issuesBefore.IssuesOnDayAfter)).Return(_scheduleDays);

				Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_personAssignment.Period).Return(dateTimePeriod);
				Expect.Call(_skillStaffPeriodBefore.Period).Return(dateTimePeriod.MovePeriod(TimeSpan.FromHours(5)));
			}

			using (_mock.Playback())
			{
				_target.Execute(_optimizationPreferences, _dateOnlyPeriod, _scheduleDays, _schedulingResultStateHolder, _allScheduleMatrixPros, _rollbackService, _resourceCalculateDelayer);
			}
		}

		[Test]
		public void ShouldBreakWhenNoIssuesLeftOnDay()
		{
			_issuesBefore.IssuesOnDay = _skillStaffPeriodIssuesBefore;
			_issuesBefore.IssuesOnDayAfter = _skillStaffPeriodIssuesBefore;
			var dateTimePeriod = new DateTimePeriod(2014, 1, 1, 10, 2014, 1, 1, 12);

			_scheduleDays.Add(_scheduleDay);

			using (_mock.Record())
			{
				Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
				Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.Skills).Return(_skills);
				Expect.Call(_intervalIssueCalculator.CalculateIssues(_schedulingResultStateHolder, _skill, _dateOnly)).Return(_issuesBefore);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);

				Expect.Call(_scheduleDayIntraIntervalIssueExtractor.Extract(_scheduleDictionary, _dateOnly, _issuesBefore.IssuesOnDay)).Return(_scheduleDays);
				Expect.Call(_scheduleDayIntraIntervalIssueExtractor.Extract(_scheduleDictionary, _dateOnly, _issuesBefore.IssuesOnDayAfter)).Return(new List<IScheduleDay>());

				Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_personAssignment.Period).Return(dateTimePeriod);
				Expect.Call(_skillStaffPeriodBefore.Period).Return(dateTimePeriod);
				Expect.Call(_intraIntervalOptimizer.Optimize(_schedulingOptions, _optimizationPreferences, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issuesBefore, false)).Return(_issuesAfter);
			}

			using (_mock.Playback())
			{
				_target.Execute(_optimizationPreferences, _dateOnlyPeriod, _scheduleDays, _schedulingResultStateHolder, _allScheduleMatrixPros, _rollbackService, _resourceCalculateDelayer);
			}
		}

		[Test]
		public void ShouldBreakWhenNoIssuesLeftOnDayAfter()
		{
			_issuesBefore.IssuesOnDay = _skillStaffPeriodIssuesBefore;
			_issuesBefore.IssuesOnDayAfter = _skillStaffPeriodIssuesBefore;
			var dateTimePeriod = new DateTimePeriod(2014, 1, 1, 10, 2014, 1, 1, 12);

			_scheduleDays.Add(_scheduleDay);

			using (_mock.Record())
			{
				Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
				Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.Skills).Return(_skills);
				Expect.Call(_intervalIssueCalculator.CalculateIssues(_schedulingResultStateHolder, _skill, _dateOnly)).Return(_issuesBefore);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);

				Expect.Call(_scheduleDayIntraIntervalIssueExtractor.Extract(_scheduleDictionary, _dateOnly, _issuesBefore.IssuesOnDay)).Return(new List<IScheduleDay>());
				Expect.Call(_scheduleDayIntraIntervalIssueExtractor.Extract(_scheduleDictionary, _dateOnly, _issuesBefore.IssuesOnDayAfter)).Return(_scheduleDays);

				Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_personAssignment.Period).Return(dateTimePeriod);
				Expect.Call(_skillStaffPeriodBefore.Period).Return(dateTimePeriod);
				Expect.Call(_intraIntervalOptimizer.Optimize(_schedulingOptions, _optimizationPreferences, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issuesBefore, true)).Return(_issuesAfter);
			}

			using (_mock.Playback())
			{
				_target.Execute(_optimizationPreferences, _dateOnlyPeriod, _scheduleDays, _schedulingResultStateHolder, _allScheduleMatrixPros, _rollbackService, _resourceCalculateDelayer);
			}
		}

		[Test]
		public void ShouldCancel()
		{
			_issuesBefore.IssuesOnDay = _skillStaffPeriodIssuesBefore;
			_issuesBefore.IssuesOnDayAfter = _skillStaffPeriodIssuesBefore;
			_issuesAfter.IssuesOnDay = _skillStaffPeriodIssuesAfter;
			_issuesAfter.IssuesOnDayAfter = _skillStaffPeriodIssuesAfter;
			var dateTimePeriod = new DateTimePeriod(2014, 1, 1, 10, 2014, 1, 1, 12);

			using (_mock.Record())
			{
				Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
				Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.Skills).Return(_skills);
				Expect.Call(_intervalIssueCalculator.CalculateIssues(_schedulingResultStateHolder, _skill, _dateOnly)).Return(_issuesBefore);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);

				Expect.Call(_scheduleDayIntraIntervalIssueExtractor.Extract(_scheduleDictionary, _dateOnly, _issuesBefore.IssuesOnDay)).Return(_scheduleDays);
				Expect.Call(_scheduleDayIntraIntervalIssueExtractor.Extract(_scheduleDictionary, _dateOnly, _issuesBefore.IssuesOnDayAfter)).Return(_scheduleDays);

				Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_personAssignment.Period).Return(dateTimePeriod);
				Expect.Call(_skillStaffPeriodBefore.Period).Return(dateTimePeriod);
				Expect.Call(_intraIntervalOptimizer.Optimize(_schedulingOptions, _optimizationPreferences, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issuesBefore, false)).Return(_issuesAfter);
			}

			using (_mock.Playback())
			{
				_target.ReportProgress += OnReportProgress;
				_target.Execute(_optimizationPreferences, _dateOnlyPeriod, _scheduleDays, _schedulingResultStateHolder, _allScheduleMatrixPros, _rollbackService, _resourceCalculateDelayer);
				_target.ReportProgress -= OnReportProgress;
			}
		}


		void OnReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.Cancel = true;
			e.CancelAction();
		}
	}
}
