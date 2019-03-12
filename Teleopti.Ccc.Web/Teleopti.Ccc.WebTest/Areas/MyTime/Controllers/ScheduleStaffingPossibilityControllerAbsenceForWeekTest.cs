using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Requests;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.WebTest.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	[MyTimeWebTest]
	public class ScheduleStaffingPossibilityControllerAbsenceForWeekTest : IIsolateSystem, ITestInterceptor
	{
		public ScheduleStaffingPossibilityController Target;
		public ICurrentScenario Scenario;
		public ILoggedOnUser User;
		public MutableNow Now;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillTypeRepository SkillTypeRepository;
		public FakeToggleManager ToggleManager;
		public SkillIntradayStaffingFactory SkillIntradayStaffingFactory;

		private readonly TimePeriod _defaultSiteOpenHour = new TimePeriod(8, 0, 17, 0);
		private readonly TimePeriod _defaultSkillOpenHour = new TimePeriod(8, 00, 9, 30);
		private DateTimePeriod _defaultAssignmentPeriod;
		private int _defaultSkillStaffingIntervalNumber;
		private TimeZoneInfo _defaultTimezone;
		private IPerson _loggedUser;
		private DateOnly _today;

		private readonly ISkillType phoneSkillType =
			new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony)
				.WithId();

		public void OnBefore()
		{
			_today = Now.UtcDateTime().ToDateOnly();
			_loggedUser = User.CurrentUser();
			_defaultTimezone = User.CurrentUser().PermissionInformation.DefaultTimeZone();

			_defaultSkillStaffingIntervalNumber =
				(int)_defaultSkillOpenHour.EndTime.Subtract(_defaultSkillOpenHour.StartTime).TotalMinutes / 15;


			_defaultAssignmentPeriod =
				new DateTimePeriod(TimeZoneHelper.ConvertToUtc(_today.Date.AddHours(8), _defaultTimezone),
					TimeZoneHelper.ConvertToUtc(_today.Date.AddHours(17), _defaultTimezone));
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>();
			isolate.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			isolate.UseTestDouble<FakeSkillCombinationResourceRepository>().For<ISkillCombinationResourceRepository>();
			isolate.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			isolate.UseTestDouble<FakeTimeZoneGuard>().For<ITimeZoneGuard>();
			isolate.UseTestDouble<FakeSkillTypeRepository>().For<ISkillTypeRepository>();
		}

		[Test]
		public void ShouldReturnEmptyPossibilities()
		{
			var possibilities = Target.GetPossibilityViewModelsForWeek(null).ToList();
			Assert.AreEqual(0, possibilities.Count);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForAbsenceWhenUnderStaffing()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var skillStaffingDataList = new List<SkillStaffingData>
			{
				new SkillStaffingData
				{
					Date = _today,
					ForecastedStaffing = new List<double> {10d, 10d},
					ScheduledStaffing = new List<double> {8d, 8d},
					TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
				}
			};
			setupStaffingForSkill(skill, skillStaffingDataList);

			var possibilities = Target.GetPossibilityViewModelsForWeek(_today, StaffingPossibilityType.Absence)
				.Where(d => d.Date == _today.ToFixedClientDateOnlyFormat()).ToList();

			possibilities.Count.Should().Be(_defaultSkillStaffingIntervalNumber);
			possibilities.ElementAt(0).Possibility.Should().Be(0);
			possibilities.ElementAt(1).Possibility.Should().Be(0);
		}

		[Test]
		public void ShouldGetGoodPossibilitiesForAbsenceWhenNotUnderStaffing()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var skillStaffingDataList = new List<SkillStaffingData>
			{
				new SkillStaffingData
				{
					Date = _today,
					ForecastedStaffing = new List<double> {10d, 10d},
					ScheduledStaffing = new List<double> {11d, 11d},
					TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
				}
			};
			setupStaffingForSkill(skill, skillStaffingDataList);

			var possibilities = Target.GetPossibilityViewModelsForWeek(_today, StaffingPossibilityType.Absence)
				.Where(d => d.Date == _today.ToFixedClientDateOnlyFormat()).ToList();

			possibilities.Count.Should().Be(_defaultSkillStaffingIntervalNumber);
			possibilities.ElementAt(0).Possibility.Should().Be(1);
			possibilities.ElementAt(1).Possibility.Should().Be(1);
		}

		[Test]
		public void ShouldGetAllGoodPossibilitiesWhenStaffingLevelIsEqualToUnderStaffingThresholds()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			skill.StaffingThresholds = new StaffingThresholds(new Percent(-0.96), new Percent(-0.96), new Percent(0.1));
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var skillStaffingDataList = new List<SkillStaffingData>
			{
				new SkillStaffingData
				{
					Date = _today,
					ForecastedStaffing = new List<double> {100d, 100d},
					ScheduledStaffing = new List<double> {5d, 5d},
					TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
				}
			};
			setupStaffingForSkill(skill, skillStaffingDataList);

			var possibilities =
				Target.GetPossibilityViewModelsForWeek(_today, StaffingPossibilityType.Absence)
					.Where(d => d.Date == _today.ToFixedClientDateOnlyFormat())
					.ToList();

			possibilities.Count.Should().Be(_defaultSkillStaffingIntervalNumber);
			possibilities.ElementAt(0).Possibility.Should().Be(1);
			possibilities.ElementAt(1).Possibility.Should().Be(1);
		}

		[Test]
		public void ShouldGetFairAndGoodPossibilitiesForAbsence()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var skillStaffingDataList = new List<SkillStaffingData>
			{
				new SkillStaffingData
				{
					Date = _today,
					ForecastedStaffing = new List<double> {10d, 10d},
					ScheduledStaffing = new List<double> {8d, 11d},
					TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
				}
			};
			setupStaffingForSkill(skill, skillStaffingDataList);

			var possibilities = Target.GetPossibilityViewModelsForWeek(_today, StaffingPossibilityType.Absence)
				.Where(d => d.Date == _today.ToFixedClientDateOnlyFormat()).ToList();

			possibilities.Count.Should().Be(_defaultSkillStaffingIntervalNumber);
			possibilities.ElementAt(0).Possibility.Should().Be(0);
			possibilities.ElementAt(1).Possibility.Should().Be(1);
		}

		[Test]
		public void ShouldNotReturnPossibilitiesForFarFutureDays()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var skillStaffingDataList = new List<SkillStaffingData>
			{
				new SkillStaffingData
				{
					Date = _today,
					ForecastedStaffing = new List<double> {10d, 10d},
					ScheduledStaffing = new List<double> {8d, 8d},
					TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
				}
			};
			setupStaffingForSkill(skill, skillStaffingDataList);

			var result = Target.GetPossibilityViewModelsForWeek(_today.AddWeeks(8), StaffingPossibilityType.Absence);

			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotReturnPossibilitiesForDaysNotInAbsenceOpenPeriod()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var skillStaffingDataList = new List<SkillStaffingData>
			{
				new SkillStaffingData
				{
					Date = _today,
					ForecastedStaffing = new List<double> {10d, 10d},
					ScheduledStaffing = new List<double> {8d, 8d},
					TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
				}
			};

			setupStaffingForSkill(skill, skillStaffingDataList);
			setupWorkFlowControlSet();

			var absenceRequestOpenDatePeriod =
				(AbsenceRequestOpenDatePeriod) _loggedUser.WorkflowControlSet.AbsenceRequestOpenPeriods[0];

			absenceRequestOpenDatePeriod.Period = new DateOnlyPeriod(_today.AddDays(6), _today.AddDays(7));
			absenceRequestOpenDatePeriod.OpenForRequestsPeriod = new DateOnlyPeriod(_today.AddDays(1), _today.AddDays(7));

			var result = Target.GetPossibilityViewModelsForWeek(_today, StaffingPossibilityType.Absence);

			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetPossibilitiesWhenUsingShrinkageValidator()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);

			var activity = createActivity();
			createAssignment(_loggedUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var skillStaffingDataList = new List<SkillStaffingData>
			{
				new SkillStaffingData
				{
					Date = _today,
					ForecastedStaffing = new List<double> {10d, 10d},
					ScheduledStaffing = new List<double> {8d, 8d},
					TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
				}
			};
			setupStaffingForSkill(skill, skillStaffingDataList);

			var workflowControlSet = WorkflowControlSetFactory.CreateWorkFlowControlSet(
				AbsenceFactory.CreateAbsenceWithId(), new PendingAbsenceRequest(),
				false);
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(_today.AddDays(-20),
					_today.AddDays(20)),
				OpenForRequestsPeriod = new DateOnlyPeriod(_today.AddDays(-20),
					_today.AddDays(20)),
				StaffingThresholdValidator = new StaffingThresholdWithShrinkageValidator()
			});
			_loggedUser.WorkflowControlSet = workflowControlSet;

			var possibilities =
				Target.GetPossibilityViewModelsForWeek(_today, StaffingPossibilityType.Absence);

			possibilities.Count().Should().Be(_defaultSkillStaffingIntervalNumber);
		}

		[Test]
		public void ShouldGetProperPossibilitiesWhenUsingBothIntradayAndIntradayWithShrinkageValidators()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);

			var activity = createActivity();

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var dayInRollingPeriod = _today.AddDays(19);
			var dayInShrinkagePeriod = _today.AddDays(22);

			var period1 = new DateTimePeriod(
				TimeZoneHelper.ConvertToUtc(dayInRollingPeriod.Date.AddHours(8), _defaultTimezone),
				TimeZoneHelper.ConvertToUtc(dayInRollingPeriod.Date.AddHours(17), _defaultTimezone));
			var period2 =
				new DateTimePeriod(TimeZoneHelper.ConvertToUtc(dayInShrinkagePeriod.Date.AddHours(8), _defaultTimezone),
					TimeZoneHelper.ConvertToUtc(dayInShrinkagePeriod.Date.AddHours(17), _defaultTimezone));

			createAssignment(_loggedUser, period1, activity);
			createAssignment(_loggedUser, period2, activity);

			var skillStaffingDataList = new List<SkillStaffingData>
			{
				new SkillStaffingData
				{
					Date = dayInShrinkagePeriod,
					ForecastedStaffing = new List<double> {10d, 9.5d},
					ScheduledStaffing = new List<double> {10d, 9.5d},
					TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
				},
				new SkillStaffingData
				{
					Date = dayInRollingPeriod,
					ForecastedStaffing = new List<double> {10d, 9.5d},
					ScheduledStaffing = new List<double> {10d, 9.5d},
					TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
				}
			};
			setupStaffingForSkill(skill, skillStaffingDataList);

			var rollingPeriod = new AbsenceRequestOpenRollingPeriod
			{
				OrderIndex = 0,
				BetweenDays = new MinMax<int>(0, 20),
				OpenForRequestsPeriod = new DateOnlyPeriod(_today.AddDays(-20),
					_today.AddDays(40)),
				StaffingThresholdValidator = new StaffingThresholdValidator()
			};

			var intradayWithShrinkagePeriod = new AbsenceRequestOpenDatePeriod
			{
				OrderIndex = 1,
				Period = new DateOnlyPeriod(_today.AddDays(21),
					_today.AddDays(40)),
				OpenForRequestsPeriod = new DateOnlyPeriod(_today.AddDays(-20),
					_today.AddDays(40)),
				StaffingThresholdValidator = new StaffingThresholdWithShrinkageValidator()
			};

			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AddOpenAbsenceRequestPeriod(rollingPeriod);
			workFlowControlSet.AddOpenAbsenceRequestPeriod(intradayWithShrinkagePeriod);
			_loggedUser.WorkflowControlSet = workFlowControlSet;

			var possibilitiesIntraday = Target
				.GetPossibilityViewModelsForWeek(dayInRollingPeriod, StaffingPossibilityType.Absence)
				.Where(x => x.Date == dayInRollingPeriod.ToFixedClientDateOnlyFormat()).ToList();

			var possibilitiesIntradayWithShrinkage = Target
				.GetPossibilityViewModelsForWeek(dayInShrinkagePeriod, StaffingPossibilityType.Absence)
				.Where(x => x.Date == dayInShrinkagePeriod.ToFixedClientDateOnlyFormat()).ToList();

			var count = _defaultSkillStaffingIntervalNumber;

			possibilitiesIntraday.Count.Should().Be(count);
			possibilitiesIntraday[0].Possibility.Should().Be(1);
			possibilitiesIntraday[1].Possibility.Should().Be(0);

			possibilitiesIntradayWithShrinkage.Count.Should().Be(count);
			possibilitiesIntradayWithShrinkage[0].Possibility.Should().Be(0);
			possibilitiesIntradayWithShrinkage[1].Possibility.Should().Be(0);
		}

		[Test]
		public void ShouldGetPossibilitiesWhenSomeStaffingDataIsNotAvailable()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var period = getAvailablePeriod();
			period.DayCollection().ToList().ForEach(day =>
			{
				var utcDate = TimeZoneHelper.ConvertToUtc(day.Date, _defaultTimezone);
				var skillStaffingDataList = new List<SkillStaffingData>
				{
					new SkillStaffingData
					{
						Date = utcDate.ToDateOnly(),
						ForecastedStaffing = new List<double> {10d, 9.5d},
						ScheduledStaffing = new List<double> {11d, 11d},
						TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
					}
				};

				setupStaffingForSkill(skill, skillStaffingDataList);
			});

			var possibilities =
				Target.GetPossibilityViewModelsForWeek(null, StaffingPossibilityType.Absence)
					.Where(d => d.Date == _today.ToFixedClientDateOnlyFormat())
					.ToList();
			possibilities.Count.Should().Be(_defaultSkillStaffingIntervalNumber);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForAbsenceWhenOneOfSkillIsUnderStaffingEvenIfTheRelatedActivityIsUnderlying()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity1 = createActivity("under staff activity 1");
			var assignment = createAssignment(_loggedUser, _defaultAssignmentPeriod, activity1);

			var skill1 = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity1, skill1);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var skillStaffingDataList1 = new List<SkillStaffingData>
			{
				new SkillStaffingData
				{
					Date = _today,
					ForecastedStaffing = new List<double> {10d, 10d},
					ScheduledStaffing = new List<double> {8d, 8d},
					TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
				}
			};
			setupStaffingForSkill(skill1, skillStaffingDataList1);

			var activity2 = createActivity("over staff activity 2");
			assignment.AddActivity(activity2, _defaultAssignmentPeriod);

			var skill2 = createSkill("skill for test 2", _defaultSkillOpenHour);
			var personSkill2 = createPersonSkill(activity2, skill2);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill2);

			var skillStaffingDataList2 = new List<SkillStaffingData>
			{
				new SkillStaffingData
				{
					Date = _today,
					ForecastedStaffing = new List<double> {10d, 10d},
					ScheduledStaffing = new List<double> {11d, 11d},
					TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
				}
			};
			setupStaffingForSkill(skill2, skillStaffingDataList2);

			var possibilities = Target.GetPossibilityViewModelsForWeek(_today, StaffingPossibilityType.Absence)
				.Where(d => d.Date == _today.ToFixedClientDateOnlyFormat()).ToList();

			possibilities.Count.Should().Be(_defaultSkillStaffingIntervalNumber);
			possibilities.ElementAt(0).Possibility.Should().Be(0);
			possibilities.ElementAt(1).Possibility.Should().Be(0);
		}

		[Test]
		public void ShouldSubtractCurrentUsersMainShiftWhenCalculatingAbsenceProbability()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var skillStaffingDataList = new List<SkillStaffingData>
			{
				new SkillStaffingData
				{
					Date = _today,
					ForecastedStaffing = new List<double> {1d, 1d},
					ScheduledStaffing = new List<double> {1d, 1d},
					TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
				}
			};
			setupStaffingForSkill(skill, skillStaffingDataList);

			var possibilities = Target.GetPossibilityViewModelsForWeek(null, StaffingPossibilityType.Absence)
				.Where(d => d.Date == _today.ToFixedClientDateOnlyFormat())
				.ToList();

			possibilities.Count.Should().Be(_defaultSkillStaffingIntervalNumber);
			possibilities.ElementAt(0).Possibility.Should().Be(0);
			possibilities.ElementAt(1).Possibility.Should().Be(0);
		}

		[Test]
		public void ShouldSubtractCurrentUsersOvertimeShiftWhenCalculatingAbsenceProbability()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var skillStaffingDataList = new List<SkillStaffingData>
			{
				new SkillStaffingData
				{
					Date = _today,
					ForecastedStaffing = new List<double> {0.1d, 0.1d},
					ScheduledStaffing = new List<double> {1d, 1d},
					TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
				}
			};
			setupStaffingForSkill(skill, skillStaffingDataList);

			var possibilities =
				Target.GetPossibilityViewModelsForWeek(_today, StaffingPossibilityType.Absence)
					.Where(d => d.Date == _today.ToFixedClientDateOnlyFormat())
					.ToList();

			possibilities.Count.Should().Be(_defaultSkillStaffingIntervalNumber);
			possibilities.ElementAt(0).Possibility.Should().Be(0);
			possibilities.ElementAt(1).Possibility.Should().Be(0);
		}

		[Test]
		public void ShouldNotUsePrimarySkillsWhenCalculatingAbsenceProbability()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity1 = createActivity("activity 1");
			var assignment = createAssignment(_loggedUser, _defaultAssignmentPeriod, activity1);

			var skill1 = createSkill("skill for test 1", _defaultSkillOpenHour);
			skill1.SetCascadingIndex(1);
			var personSkill = createPersonSkill(activity1, skill1);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var skillStaffingDataList1 = new List<SkillStaffingData>
			{
				new SkillStaffingData
				{
					Date = _today,
					ForecastedStaffing = new List<double> {5d, 5d},
					ScheduledStaffing = new List<double> {5.8d, 5.8d},
					TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
				}
			};
			setupStaffingForSkill(skill1, skillStaffingDataList1);

			var activity2 = createActivity("activity 2");
			assignment.AddActivity(activity2, _defaultAssignmentPeriod);

			var skill2 = createSkill("skill for test 2", _defaultSkillOpenHour);
			var personSkill2 = createPersonSkill(activity2, skill2);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill2);

			var skillStaffingDataList2 = new List<SkillStaffingData>
			{
				new SkillStaffingData
				{
					Date = _today,
					ForecastedStaffing = new List<double> {5d, 5d},
					ScheduledStaffing = new List<double> {1d, 1d},
					TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
				}
			};
			setupStaffingForSkill(skill2, skillStaffingDataList2);

			var possibilities =
				Target.GetPossibilityViewModelsForWeek(null, StaffingPossibilityType.Absence)
					.Where(d => d.Date == _today.ToFixedClientDateOnlyFormat())
					.ToList();

			possibilities.Count.Should().Be(_defaultSkillStaffingIntervalNumber);
			possibilities.ElementAt(0).Possibility.Should().Be(0);
			possibilities.ElementAt(1).Possibility.Should().Be(0);
		}

		[Test]
		public void ShouldNotRoundStaffingDataForAbsenceProbability()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			skill.StaffingThresholds = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0.1));
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var skillStaffingDataList = new List<SkillStaffingData>
			{
				new SkillStaffingData
				{
					Date = _today,
					ForecastedStaffing = new List<double> {1.03d, 1.03d},
					ScheduledStaffing = new List<double> {1d, 1d},
					TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
				}
			};
			setupStaffingForSkill(skill, skillStaffingDataList);

			var possibilities =
				Target.GetPossibilityViewModelsForWeek(_today, StaffingPossibilityType.Absence)
					.Where(d => d.Date == _today.ToFixedClientDateOnlyFormat()).ToList();

			possibilities.Count.Should().Be(_defaultSkillStaffingIntervalNumber);
			possibilities.ElementAt(0).Possibility.Should().Be(0);
			possibilities.ElementAt(1).Possibility.Should().Be(0);
		}

		private void setupWorkFlowControlSet()
		{
			var absenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Absence = AbsenceFactory.CreateAbsence("absence for test"),
				Period = new DateOnlyPeriod(_today.AddDays(-20),
					_today.AddDays(20)),
				OpenForRequestsPeriod = new DateOnlyPeriod(_today.AddDays(-20),
					_today.AddDays(20)),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};

			var overtimeRequestOpenDatePeriod = new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 13)
			};

			var workFlowControlSet = new WorkflowControlSet();

			workFlowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenDatePeriod);
			workFlowControlSet.AddOpenOvertimeRequestPeriod(overtimeRequestOpenDatePeriod);

			_loggedUser.WorkflowControlSet = workFlowControlSet;
		}

		private PersonPeriod getOrAddPersonPeriod(DateOnly startDate, IPerson person)
		{
			var personPeriod =
				(PersonPeriod) person.PersonPeriods(startDate.ToDateOnlyPeriod())
					.FirstOrDefault();
			if (personPeriod != null) return personPeriod;
			var team = TeamFactory.CreateTeam("team1", "site1");
			personPeriod =
				(PersonPeriod)
				PersonPeriodFactory.CreatePersonPeriod(startDate,
					PersonContractFactory.CreatePersonContract(), team);
			person.AddPersonPeriod(personPeriod);
			return personPeriod;
		}

		private void setupSiteOpenHour(TimePeriod timePeriod, ISite site)
		{
			site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = timePeriod,
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});
		}

		private void addPersonSkillsToPersonPeriod(PersonPeriod personPeriod, params IPersonSkill[] personSkills)
		{
			foreach (var personSkill in personSkills)
			{
				personPeriod.AddPersonSkill(personSkill);
			}
		}

		private IPersonSkill createPersonSkill(IActivity activity, ISkill skill)
		{
			skill.Activity = activity;
			var personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);
			return personSkill;
		}

		private ISkill createSkill(string name, TimePeriod openHour)
		{
			var skill = SkillFactory.CreateSkill(name).WithId();
			skill.SkillType = phoneSkillType;
			skill.StaffingThresholds = createStaffingThresholds();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHour);
			SkillRepository.Has(skill);
			return skill;
		}

		private static StaffingThresholds createStaffingThresholds()
		{
			return new StaffingThresholds(new Percent(-0.3), new Percent(-0.1), new Percent(0.1));
		}

		private static IActivity createActivity(string name = "activity1")
		{
			var activity = ActivityFactory.CreateActivity(name);
			activity.RequiresSkill = true;
			return activity;
		}

		private IPersonAssignment createAssignment(IPerson person, DateTimePeriod dateTimePeriod,
			params IActivity[] activities)
		{
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				Scenario.Current(),
				dateTimePeriod,
				ShiftCategoryFactory.CreateShiftCategory(), activities);
			PersonAssignmentRepository.Has(assignment);

			return assignment;
		}

		private DateOnlyPeriod getAvailablePeriod()
		{
			var staffingInfoAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(ToggleManager);
			var period = new DateOnlyPeriod(_today, _today.AddDays(staffingInfoAvailableDays)).Inflate(1);
			return period;
		}

		private void setupStaffingForSkill(ISkill skill, List<SkillStaffingData> staffingDataList)
		{
			var staffingPeriodDataList = new List<StaffingPeriodData>();
			staffingDataList.ForEach(staffingData =>
			{
				for (var i = 0; i < staffingData.ForecastedStaffing.Count; i++)
				{
					var staffingPeriodData = new StaffingPeriodData
					{
						ForecastedStaffing = staffingData.ForecastedStaffing[i],
						ScheduledStaffing = staffingData.ScheduledStaffing[i],
						Period = new DateTimePeriod(staffingData.Date.Utc().Add(staffingData.TimePeriods[i].StartTime),
							staffingData.Date.Utc().Date.Add(staffingData.TimePeriods[i].EndTime))
					};
					staffingPeriodDataList.Add(staffingPeriodData);
				}

				SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(skill, staffingData.Date,
					staffingPeriodDataList,
					_loggedUser.PermissionInformation.DefaultTimeZone());

				staffingPeriodDataList.Clear();
			});
		}
	}

	class SkillStaffingData
	{
		public DateOnly Date;
		public List<double> ForecastedStaffing { get; set; }
		public List<double> ScheduledStaffing { get; set; }
		public List<TimePeriod> TimePeriods { get; set; }
	}
}
