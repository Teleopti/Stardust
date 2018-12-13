using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class StudentSchedulingServiceTest
	{
		private StudentSchedulingService _studentSchedulingService;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private MockRepository _mocks;
		private TimeZoneInfo _timeZoneInfo;
		private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private IEffectiveRestriction _effectiveRestriction;
		private IScheduleService _scheduleService;
		private SchedulingOptions _schedulingOptions;
		private IResourceCalculation _resourceOptimizationHelper;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private ISkill _skill1;
		private ISkill _skill2;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
			_scheduleService = _mocks.StrictMock<IScheduleService>();
			_schedulingOptions = new SchedulingOptions();
			_resourceOptimizationHelper = _mocks.StrictMock<IResourceCalculation>();
			_skill1 = SkillFactory.CreateSkill("Skill 1");
			_skill2 = SkillFactory.CreateSkill("Skill 2");
			_studentSchedulingService = new StudentSchedulingService(_schedulingResultStateHolder,
				_effectiveRestrictionCreator, _scheduleService, _resourceOptimizationHelper, UserTimeZone.Make());
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
			_effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																			  new EndTimeLimitation(),
																			  new WorkTimeLimitation()
																			  , null, null, null,
																			  new List<IActivityRestriction>());
		}

		[Test]
		public void VerifySetup()
		{
			Assert.IsNotNull(_studentSchedulingService);
		}

		[Test]
		public void VerifyTheSchedulingCycle()
		{
			var start = new DateTime(2009, 2, 2, 0, 0, 0, DateTimeKind.Utc);
			var date11 = new DateOnly(2009, 2, 2);
			var date12 = new DateOnly(2009, 2, 3);
			IList<DateOnly> dateOnlys = new List<DateOnly> { date11, date12 };

			var skillDay1 = _mocks.StrictMock<ISkillDay>();
			var skillDay2 = _mocks.StrictMock<ISkillDay>();

			IList<ISkillDay> lst = new List<ISkillDay> { skillDay1, skillDay2 };

			var part1 = _mocks.StrictMock<IScheduleDay>();
			var part2 = _mocks.StrictMock<IScheduleDay>();
			var part3 = _mocks.StrictMock<IScheduleDay>();
			var part4 = _mocks.StrictMock<IScheduleDay>();

			var person = _mocks.StrictMock<IPerson>();
			var person2 = _mocks.StrictMock<IPerson>();

			var period1 = new DateOnlyAsDateTimePeriod(date11, _timeZoneInfo);
			var period2 = new DateOnlyAsDateTimePeriod(date12, _timeZoneInfo);

			var skillStaffPeriod = _mocks.StrictMock<ISkillStaffPeriod>();
			var payload = _mocks.StrictMock<ISkillStaff>();

			var periods = new [] { skillStaffPeriod };
			var period = new DateTimePeriod(start.AddHours(2), start.AddHours(2).AddMinutes(5));

			var personSkill = _mocks.StrictMock<IPersonSkill>();
			var personSkill2 = _mocks.StrictMock<IPersonSkill>();
			IList<IPersonSkill> personSkills = new List<IPersonSkill> { personSkill, personSkill2 };
			IList<IPersonSkill> personSkills2 = new List<IPersonSkill> { personSkill2 };

			var personPeriod = _mocks.StrictMock<IPersonPeriod>();
			var personPeriod2 = _mocks.StrictMock<IPersonPeriod>();
			IList<IPersonPeriod> personPeriods = new List<IPersonPeriod> { personPeriod };
			IList<IPersonPeriod> personPeriods2 = new List<IPersonPeriod> { personPeriod2 };

			var preferences = new SchedulingOptions {UseShiftCategoryLimitations = false};

			var virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();

			using (_mocks.Record())
			{
				Expect.Call(_scheduleService.SchedulePersonOnDay(null, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, _rollbackService)).IgnoreArguments().Return(true)
						  .Repeat.AtLeastOnce();

				Expect.Call(person.VirtualSchedulePeriod(date11)).Return(virtualSchedulePeriod).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(person2.VirtualSchedulePeriod(date11)).Return(virtualSchedulePeriod).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(virtualSchedulePeriod.MinTimeSchedulePeriod).Return(TimeSpan.Zero).Repeat.AtLeastOnce();

				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(dateOnlys)).IgnoreArguments().Return(lst).Repeat.AtLeastOnce();

				Expect.Call(part1.Person).Return(person).Repeat.AtLeastOnce();
				Expect.Call(part2.Person).Return(person).Repeat.AtLeastOnce();
				Expect.Call(part3.Person).Return(person2).Repeat.AtLeastOnce();
				Expect.Call(part4.Person).Return(person2).Repeat.AtLeastOnce();
				Expect.Call(part1.DateOnlyAsPeriod).Return(period1).Repeat.AtLeastOnce();
				Expect.Call(part2.DateOnlyAsPeriod).Return(period2).Repeat.AtLeastOnce();
				Expect.Call(part3.DateOnlyAsPeriod).Return(period1).Repeat.AtLeastOnce();
				Expect.Call(part4.DateOnlyAsPeriod).Return(period2).Repeat.AtLeastOnce();

				Expect.Call(person.Equals(person)).Return(true).Repeat.Any();
				Expect.Call(person2.Equals(person2)).Return(true).Repeat.Any();
				Expect.Call(person2.Equals(person)).Return(false).Repeat.Any();
				Expect.Call(person.Equals(person2)).Return(false).Repeat.Any();
				Expect.Call(skillDay1.CurrentDate).Return(date11).Repeat.AtLeastOnce();
				Expect.Call(skillDay2.CurrentDate).Return(date12).Repeat.AtLeastOnce();
				Expect.Call(skillDay1.Skill).Return(_skill1).Repeat.AtLeastOnce();
				Expect.Call(skillDay2.Skill).Return(_skill2).Repeat.AtLeastOnce();
				Expect.Call(skillDay1.SkillStaffPeriodCollection).Return(periods).Repeat.AtLeastOnce();
				Expect.Call(skillDay2.SkillStaffPeriodCollection).Return(periods).Repeat.AtLeastOnce();

				Expect.Call(skillStaffPeriod.Payload).Return(payload).Repeat.AtLeastOnce();
				Expect.Call(payload.CalculatedResource).Return(10).Repeat.AtLeastOnce();
				Expect.Call(skillStaffPeriod.Period).Return(period).Repeat.AtLeastOnce();

				Expect.Call(skillDay1.ForecastedIncomingDemand).Return(new TimeSpan(100, 0, 0)).Repeat.AtLeastOnce();
				Expect.Call(skillDay2.ForecastedIncomingDemand).Return(new TimeSpan(200, 0, 0)).Repeat.AtLeastOnce();

				Expect.Call(person.PersonPeriods(new DateOnlyPeriod())).Return(personPeriods).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(person2.PersonPeriods(new DateOnlyPeriod())).Return(personPeriods2).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(personPeriod.PersonSkillCollection).Return(personSkills).Repeat.AtLeastOnce();
				Expect.Call(personPeriod2.PersonSkillCollection).Return(personSkills2).Repeat.AtLeastOnce();

				Expect.Call(personSkill.Skill).Return(_skill1).Repeat.AtLeastOnce();
				Expect.Call(personSkill2.Skill).Return(_skill2).Repeat.AtLeastOnce();
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(part1, preferences)).Return(
				  _effectiveRestriction).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.SkipResourceCalculation).Return(false).Repeat.Any();
				Expect.Call(_schedulingResultStateHolder.Skills).Return(new HashSet<ISkill> { _skill1 }).Repeat.AtLeastOnce();
			}

			_studentSchedulingService = new StudentSchedulingService(_schedulingResultStateHolder,
						_effectiveRestrictionCreator, _scheduleService, _resourceOptimizationHelper, UserTimeZone.Make());

			using (_mocks.Playback())
			{
				_studentSchedulingService.DoTheScheduling(new List<IScheduleDay> { part4, part3, part2, part1 }, preferences, true, _rollbackService);
				_studentSchedulingService.DoTheScheduling(new List<IScheduleDay> { part4, part3, part2, part1 }, preferences, false, _rollbackService);
			}
		}

		[Test]
		public void VerifyFindAllPersonsExcludesStudentsWithEnoughHours()
		{
			var dateOnly = new DateOnly(2009, 2, 2);
			var part1 = _mocks.StrictMock<IScheduleDay>();
			var part2 = _mocks.StrictMock<IScheduleDay>();
			var part3 = _mocks.StrictMock<IScheduleDay>();
			var part4 = _mocks.StrictMock<IScheduleDay>();
			var virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();

			var schedules = _mocks.StrictMock<IScheduleDictionary>();
			var range1 = _mocks.StrictMock<IScheduleRange>();
			var range2 = _mocks.StrictMock<IScheduleRange>();

			var part5 = _mocks.StrictMock<IScheduleDay>();
			var part6 = _mocks.StrictMock<IScheduleDay>();

			var projectionService1 = _mocks.StrictMock<IProjectionService>();
			var projectionService2 = _mocks.StrictMock<IProjectionService>();
			var layerCollection1 = _mocks.StrictMock<IVisualLayerCollection>();
			var layerCollection2 = _mocks.StrictMock<IVisualLayerCollection>();

			var person = _mocks.StrictMock<IPerson>();
			var person2 = _mocks.StrictMock<IPerson>();

			using (_mocks.Record())
			{
				Expect.Call(part1.Person).Return(person).Repeat.AtLeastOnce();
				Expect.Call(part2.Person).Return(person).Repeat.AtLeastOnce();
				Expect.Call(part3.Person).Return(person2).Repeat.AtLeastOnce();
				Expect.Call(part4.Person).Return(person2).Repeat.AtLeastOnce();

				Expect.Call(person.VirtualSchedulePeriod(dateOnly)).Return(virtualSchedulePeriod);
				Expect.Call(person2.VirtualSchedulePeriod(dateOnly)).Return(virtualSchedulePeriod);
				Expect.Call(virtualSchedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(dateOnly, dateOnly)).Repeat.
					 Twice();

				Expect.Call(virtualSchedulePeriod.MinTimeSchedulePeriod).Return(new TimeSpan(5, 0, 0)).Repeat.AtLeastOnce();

				Expect.Call(_schedulingResultStateHolder.Schedules).Return(schedules).Repeat.Twice();

				Expect.Call(schedules[person]).Return(range1);
				Expect.Call(schedules[person2]).Return(range2);

				Expect.Call(range1.ScheduledDayCollection(dateOnly.ToDateOnlyPeriod())).Return(new[] { part5 });
				Expect.Call(range2.ScheduledDayCollection(dateOnly.ToDateOnlyPeriod())).Return(new[] { part6 });

				Expect.Call(part5.ProjectionService()).Return(projectionService1);
				Expect.Call(part6.ProjectionService()).Return(projectionService2);
				Expect.Call(projectionService1.CreateProjection()).Return(layerCollection1);
				Expect.Call(projectionService2.CreateProjection()).Return(layerCollection2);
				Expect.Call(layerCollection1.ContractTime()).Return(new TimeSpan(6, 0, 0));
				Expect.Call(layerCollection2.ContractTime()).Return(new TimeSpan(4, 0, 0));
			}

			_studentSchedulingService = new StudentSchedulingService(_schedulingResultStateHolder,
									_effectiveRestrictionCreator, _scheduleService, _resourceOptimizationHelper, UserTimeZone.Make());

			using (_mocks.Playback())
			{
				var ret = _studentSchedulingService.GetAllPersons(new List<IScheduleDay> { part4, part3, part2, part1 }, true, dateOnly);
				Assert.AreEqual(1, ret.Count);
				Assert.IsTrue(ret.Contains(person2));
			}
		}

		[Test]
		public void VerifyGetRandomPerson()
		{
			IPerson person1 = PersonFactory.CreatePerson();
			IPerson person2 = PersonFactory.CreatePerson();
			IPerson person3 = PersonFactory.CreatePerson();
			IPerson person4 = PersonFactory.CreatePerson();
			IPerson person5 = PersonFactory.CreatePerson();

			IList<IPerson> persons = new List<IPerson> { person5, person4, person3, person2, person1 };

			IPerson person = _studentSchedulingService.GetRandomPerson(persons);
			Assert.IsNotNull(person);
		}

		[Test]
		public void VerifyCanCancelTheSchedulingCycle()
		{
			var start = new DateTime(2009, 2, 1, 23, 0, 0, DateTimeKind.Utc);
			var date11 = new DateOnly(2009, 2, 2);
			var date12 = new DateOnly(2009, 2, 3);
			IList<DateOnly> dateOnlies = new List<DateOnly> { date11, date12 };

			var skillDay1 = _mocks.StrictMock<ISkillDay>();
			var skillDay2 = _mocks.StrictMock<ISkillDay>();

			IList<ISkillDay> lst = new List<ISkillDay> { skillDay1, skillDay2 };

			var part1 = _mocks.StrictMock<IScheduleDay>();
			var part2 = _mocks.StrictMock<IScheduleDay>();
			var person = _mocks.StrictMock<IPerson>();
			var skillStaffPeriod = _mocks.StrictMock<ISkillStaffPeriod>();
			var payload = _mocks.StrictMock<ISkillStaff>();
			var periods = new [] { skillStaffPeriod };
			var period = new DateTimePeriod(start.AddHours(2), start.AddHours(2).AddMinutes(5));
			var personSkill = _mocks.StrictMock<IPersonSkill>();
			var personSkill2 = _mocks.StrictMock<IPersonSkill>();
			IList<IPersonSkill> personSkills = new List<IPersonSkill> { personSkill, personSkill2 };
			var personPeriod = _mocks.StrictMock<IPersonPeriod>();
			IList<IPersonPeriod> personPeriods = new List<IPersonPeriod> { personPeriod };
			var preferences = new SchedulingOptions {UseShiftCategoryLimitations = false};

			var virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();

			using (_mocks.Record())
			{
				Expect.Call(_scheduleService.SchedulePersonOnDay(null, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, _rollbackService)).IgnoreArguments().Return(true)
						  .Repeat.AtLeastOnce();

				Expect.Call(person.VirtualSchedulePeriod(date11)).Return(virtualSchedulePeriod).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(virtualSchedulePeriod.MinTimeSchedulePeriod).Return(new TimeSpan(0, 0, 0)).Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(dateOnlies)).IgnoreArguments().Return(lst).Repeat.AtLeastOnce();
				Expect.Call(part1.Person).Return(person).Repeat.AtLeastOnce();
				Expect.Call(part2.Person).Return(person).Repeat.AtLeastOnce();
				Expect.Call(part1.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date11, _timeZoneInfo)).Repeat.AtLeastOnce();
				Expect.Call(part2.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date12, _timeZoneInfo)).Repeat.AtLeastOnce();
				Expect.Call(person.Equals(person)).Return(true).Repeat.AtLeastOnce();
				Expect.Call(skillDay2.CurrentDate).Return(date12).Repeat.AtLeastOnce();
				Expect.Call(skillDay2.Skill).Return(_skill2).Repeat.AtLeastOnce();
				Expect.Call(skillDay1.SkillStaffPeriodCollection).Return(periods).Repeat.AtLeastOnce();
				Expect.Call(skillDay2.SkillStaffPeriodCollection).Return(periods).Repeat.AtLeastOnce();
				Expect.Call(skillStaffPeriod.Payload).Return(payload).Repeat.AtLeastOnce();
				Expect.Call(payload.CalculatedResource).Return(10).Repeat.AtLeastOnce();
				Expect.Call(skillStaffPeriod.Period).Return(period).Repeat.AtLeastOnce();
				Expect.Call(skillDay1.ForecastedIncomingDemand).Return(new TimeSpan(100, 0, 0)).Repeat.AtLeastOnce();
				Expect.Call(skillDay2.ForecastedIncomingDemand).Return(new TimeSpan(200, 0, 0)).Repeat.AtLeastOnce();
				Expect.Call(person.PersonPeriods(new DateOnlyPeriod())).Return(personPeriods).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(personPeriod.PersonSkillCollection).Return(personSkills).Repeat.AtLeastOnce();
				Expect.Call(personSkill.Skill).Return(_skill1).Repeat.AtLeastOnce();
				Expect.Call(personSkill2.Skill).Return(_skill2).Repeat.AtLeastOnce();
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(part1, preferences)).Return(
			  _effectiveRestriction).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.SkipResourceCalculation).Return(false).Repeat.Any();
				Expect.Call(_schedulingResultStateHolder.Skills).Return(new HashSet<ISkill> { _skill1 });
			}

			_studentSchedulingService = new StudentSchedulingService(_schedulingResultStateHolder,
											_effectiveRestrictionCreator, _scheduleService, _resourceOptimizationHelper, UserTimeZone.Make());

			using (_mocks.Playback())
			{
				_studentSchedulingService.DayScheduled += (sender, e) => { e.Cancel = true; };
				_studentSchedulingService.DoTheScheduling(new List<IScheduleDay> { part2, part1 }, preferences, false, _rollbackService);
			}
		}

		[Test]
		public void VerifyFindCorrectOnEmptyList()
		{
			var date1 = new DateOnly(2009, 2, 2);
			IPerson person = PersonFactory.CreatePerson();
			var part = _studentSchedulingService.GetSchedulePartOnDateAndPerson(new List<IScheduleDay>(), person, date1);
			Assert.IsNull(part);
		}

		[Test]
		public void VerifyFindCorrectSchedulePart()
		{
			var part1 = _mocks.StrictMock<IScheduleDay>();
			var part2 = _mocks.StrictMock<IScheduleDay>();
			var part3 = _mocks.StrictMock<IScheduleDay>();
			var part4 = _mocks.StrictMock<IScheduleDay>();

			var person = _mocks.StrictMock<IPerson>();

			var date1 = new DateOnly(2009, 2, 2);

			using (_mocks.Record())
			{
				Expect.Call(part1.Person).Return(person).Repeat.AtLeastOnce();
				Expect.Call(part1.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date1, _timeZoneInfo)).Repeat.AtLeastOnce();
				Expect.Call(person.Equals(person)).Return(true).Repeat.AtLeastOnce();
			}
			_studentSchedulingService = new StudentSchedulingService(_schedulingResultStateHolder, _effectiveRestrictionCreator, _scheduleService, _resourceOptimizationHelper, UserTimeZone.Make());

			using (_mocks.Playback())
			{
				var part = _studentSchedulingService.GetSchedulePartOnDateAndPerson(new List<IScheduleDay> { part1, part2, part3, part4 }, person, date1);
				Assert.IsNotNull(part);
				Assert.AreEqual(part, part1);
			}
		}

		[Test]
		public void VerifyFindDates()
		{
			var date1 = new DateOnly(2009, 2, 2);
			var date2 = new DateOnly(2009, 2, 3);

			var part1 = _mocks.StrictMock<IScheduleDay>();
			var part2 = _mocks.StrictMock<IScheduleDay>();
			var part3 = _mocks.StrictMock<IScheduleDay>();
			var part4 = _mocks.StrictMock<IScheduleDay>();
			IList<IScheduleDay> parts = new List<IScheduleDay> { part1, part2, part3, part4 };
			_studentSchedulingService = new StudentSchedulingService(_schedulingResultStateHolder, _effectiveRestrictionCreator, _scheduleService, _resourceOptimizationHelper, UserTimeZone.Make());

			using (_mocks.Record())
			{
				Expect.Call(part1.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date1, _timeZoneInfo)).Repeat.Any();
				Expect.Call(part2.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date1, _timeZoneInfo)).Repeat.Any();
				Expect.Call(part3.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date2, _timeZoneInfo)).Repeat.Any();
				Expect.Call(part4.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date2, _timeZoneInfo)).Repeat.Any();
			}
			using (_mocks.Playback())
			{
				var lst = _studentSchedulingService.GetAllDates(parts);
				Assert.AreEqual(2, lst.Count);
				Assert.AreEqual(date1, lst[0]);
				Assert.AreEqual(date2, lst[1]);
			}
		}

		[Test]
		public void VerifyCalculate()
		{
			var start = new DateTime(2009, 2, 2, 0, 0, 0, DateTimeKind.Utc);
			var date1 = new DateOnly(2009, 2, 2);
			var date2 = new DateOnly(2009, 2, 3);
			IList<DateOnly> dates = new List<DateOnly> { date1, date2 };

			var skillDay1 = _mocks.StrictMock<ISkillDay>();
			var skillDay2 = _mocks.StrictMock<ISkillDay>();

			IList<ISkillDay> lst = new List<ISkillDay> { skillDay1, skillDay2 };

			_studentSchedulingService = new StudentSchedulingService(_schedulingResultStateHolder, _effectiveRestrictionCreator, _scheduleService, _resourceOptimizationHelper, UserTimeZone.Make());

			IList<ISkillDay> lstToExclude = new List<ISkillDay>();

			var period = new DateTimePeriod(start.AddHours(2), start.AddHours(2).AddMinutes(5));
			var skillStaffPeriod = _mocks.StrictMock<ISkillStaffPeriod>();
			var payload = _mocks.StrictMock<ISkillStaff>();

			var periods = new [] { skillStaffPeriod };
			
			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(dates)).Return(lst).Repeat.Twice().IgnoreArguments();

				Expect.Call(skillDay1.SkillStaffPeriodCollection).Return(periods).Repeat.AtLeastOnce();
				Expect.Call(skillDay2.SkillStaffPeriodCollection).Return(periods).Repeat.AtLeastOnce();

				Expect.Call(skillStaffPeriod.Payload).Return(payload).Repeat.Times(4);
				Expect.Call(payload.CalculatedResource).Return(10).Repeat.Times(4);
				Expect.Call(skillStaffPeriod.Period).Return(period).Repeat.Times(4);

				Expect.Call(skillDay1.ForecastedIncomingDemand).Return(new TimeSpan(100, 0, 0)).Repeat.Twice();
				Expect.Call(skillDay2.ForecastedIncomingDemand).Return(new TimeSpan(200, 0, 0)).Repeat.Twice();
			}

			using (_mocks.Playback())
			{
				ISkillDay ret = _studentSchedulingService.FindMostUnderstaffedSkillDay(dates, lstToExclude);
				Assert.AreSame(skillDay2, ret);
				lstToExclude.Add(ret);

				ret = _studentSchedulingService.FindMostUnderstaffedSkillDay(dates, lstToExclude);
				Assert.AreSame(skillDay1, ret);
			}
		}

		[Test]
		public void VerifyFilterPersonsOnSkill()
		{
			var dateTime = new DateOnly(2009, 2, 2);

			var skill = _mocks.StrictMock<ISkill>();
			var skill2 = _mocks.StrictMock<ISkill>();

			var personSkill = _mocks.StrictMock<IPersonSkill>();
			var personSkill2 = _mocks.StrictMock<IPersonSkill>();
			IList<IPersonSkill> personSkills = new List<IPersonSkill> { personSkill, personSkill2 };
			IList<IPersonSkill> personSkills2 = new List<IPersonSkill> { personSkill2 };

			var personPeriod = _mocks.StrictMock<IPersonPeriod>();
			var personPeriod2 = _mocks.StrictMock<IPersonPeriod>();
			IList<IPersonPeriod> personPeriods = new List<IPersonPeriod> { personPeriod };
			IList<IPersonPeriod> personPeriods2 = new List<IPersonPeriod> { personPeriod2 };

			var person = _mocks.StrictMock<IPerson>();
			var person2 = _mocks.StrictMock<IPerson>();

			IList<IPerson> persons = new List<IPerson> { person, person2 };

			using (_mocks.Record())
			{
				Expect.Call(person.PersonPeriods(new DateOnlyPeriod())).IgnoreArguments().Return(personPeriods);
				Expect.Call(person2.PersonPeriods(new DateOnlyPeriod())).IgnoreArguments().Return(personPeriods2);
				Expect.Call(personPeriod.PersonSkillCollection).Return(personSkills);
				Expect.Call(personPeriod2.PersonSkillCollection).Return(personSkills2);

				Expect.Call(personSkill.Skill).Return(skill);
				Expect.Call(skill.Equals(skill)).Return(true).Repeat.AtLeastOnce();
				Expect.Call(skill2.Equals(skill)).Return(false).Repeat.AtLeastOnce();

				Expect.Call(personSkill2.Skill).Return(skill2).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				IList<IPerson> ret = FilterPersonsOnSkill.Filter(dateTime, persons, skill);
				Assert.IsNotNull(ret);
				Assert.AreEqual(1, ret.Count);
				Assert.AreSame(person, ret[0]);
			}
		}

		[Test]
		public void ShouldSkipSchedulingWhenNoSkillAvailable()
		{
			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.Skills).Return(new HashSet<ISkill>());
			}

			_studentSchedulingService = new StudentSchedulingService(_schedulingResultStateHolder,
				 _effectiveRestrictionCreator, _scheduleService, _resourceOptimizationHelper, UserTimeZone.Make());

			using (_mocks.Playback())
			{
				var result =
					 _studentSchedulingService.DoTheScheduling(new List<IScheduleDay>(),
						  null, true, _rollbackService);
				result.Should().Be(false);
			}
		}
	}
}
