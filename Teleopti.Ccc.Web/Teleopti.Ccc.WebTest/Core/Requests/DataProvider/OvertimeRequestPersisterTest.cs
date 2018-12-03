using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;


namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[DomainTest]
	[WebTest]
	public class OvertimeRequestPersisterTest : IIsolateSystem
	{
		public IOvertimeRequestPersister Target;
		public FakeLoggedOnUser LoggedOnUser;
		public INow Now;
		public IMultiplicatorDefinitionSetRepository MultiplicatorDefinitionSetRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakeSkillTypeRepository SkillTypeRepository;
		public ICurrentScenario Scenario;
		public SkillIntradayStaffingFactory SkillIntradayStaffingFactory;
		public FakeToggleManager ToggleManager;

		private IPerson _person;
		private readonly DateTime _currentDateTime = new DateTime(2017, 11, 07);
		private readonly IMultiplicatorDefinitionSet _multiplicatorDefinitionSet 
			= new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime).WithId();
		private TimeSpan[] _intervals;
		readonly ISkillType skillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony)
			.WithId();
		public void Isolate(IIsolate isolate)
		{
			_person = PersonFactory.CreatePerson();
			_intervals = createIntervals();

			isolate.UseTestDouble(new FakeLoggedOnUser(_person)).For<ILoggedOnUser>();

			var fakeMultiplicatorDefinitionSetRepository = new FakeMultiplicatorDefinitionSetRepository();
			fakeMultiplicatorDefinitionSetRepository.Has(_multiplicatorDefinitionSet);
			isolate.UseTestDouble(fakeMultiplicatorDefinitionSetRepository).For<IMultiplicatorDefinitionSetRepository>();

			isolate.UseTestDouble(new FakeScenarioRepository(new Scenario("default") { DefaultScenario = true }))
				.For<IScenarioRepository>();
			isolate.UseTestDouble<FakeTimeZoneGuard>().For<ITimeZoneGuard>();
			isolate.UseTestDouble(new FakeLinkProvider()).For<ILinkProvider>();
			isolate.UseTestDouble<FakeScheduleDictionary>().For<IScheduleDictionary>();
			isolate.UseTestDouble<FakeActivityRepository>().For<IActivityRepository>();
			isolate.UseTestDouble<FakeActivityRepository>().For<IProxyForId<IActivity>>();
			isolate.UseTestDouble<FakePersonRepository>().For<IProxyForId<IPerson>>();
			isolate.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			isolate.UseTestDouble<FakeSkillTypeRepository>().For<ISkillTypeRepository>();
			isolate.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>();
			isolate.UseTestDouble<FakeSkillCombinationResourceRepository>().For<ISkillCombinationResourceRepository>();

			isolate.UseTestDouble<RequestApprovalServiceFactory>().For<IRequestApprovalServiceFactory>();
			isolate.UseTestDouble(new ThisIsNow(_currentDateTime)).For<INow>();
			isolate.UseTestDouble<SkillIntradayStaffingFactory>().For<SkillIntradayStaffingFactory>();

		}

		[Test]
		public void ShouldChangeStatusToPendingWhenPersistOvertimeRequest()
		{
			SkillTypeRepository.Add(new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId());
			_person.WorkflowControlSet = new WorkflowControlSet();
			_person.WorkflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { skillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				BetweenDays = new MinMax<int>(0, 13)
			});
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var overtimeRequestForm = new OvertimeRequestForm
			{
				Period = new DateTimePeriodForm
				{
					StartDate = new DateOnly(_currentDateTime),
					StartTime = new TimeOfDay(_currentDateTime.AddHours(9).TimeOfDay), 
					EndDate = new DateOnly(_currentDateTime),
					EndTime = new TimeOfDay(_currentDateTime.AddHours(10).TimeOfDay)
				},
				MultiplicatorDefinitionSet = _multiplicatorDefinitionSet.Id.Value
			};

			var result = Target.Persist(overtimeRequestForm);
			Assert.AreEqual(result.IsPending, true);
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldReProcessWhenRequestPeriodIsChanged()
		{
			SkillTypeRepository.Add(new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId());
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);
			var now = new DateOnly(_currentDateTime);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { skillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(now, now.AddDays(8)),
				OrderIndex = 0
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { skillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				BetweenDays = new MinMax<int>(0, 2),
				OrderIndex = 1
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var requestStartDateTime = _currentDateTime.AddDays(3);
			var overtimeRequestForm = new OvertimeRequestForm
			{
				Period = new DateTimePeriodForm
				{
					StartDate = new DateOnly(requestStartDateTime),
					StartTime = new TimeOfDay(requestStartDateTime.AddHours(9).TimeOfDay),
					EndDate = new DateOnly(requestStartDateTime),
					EndTime = new TimeOfDay(requestStartDateTime.AddHours(10).TimeOfDay)
				},
				MultiplicatorDefinitionSet = _multiplicatorDefinitionSet.Id.Value
			};

			var result = Target.Persist(overtimeRequestForm);

			Assert.AreEqual(result.IsPending, true);

			var newRequestStartDateTime = _currentDateTime.AddDays(1);
			overtimeRequestForm.Id = Guid.Parse(result.Id);
			overtimeRequestForm.Period = new DateTimePeriodForm
			{
				StartDate = new DateOnly(newRequestStartDateTime),
				StartTime = new TimeOfDay(newRequestStartDateTime.AddHours(9).TimeOfDay),
				EndDate = new DateOnly(newRequestStartDateTime),
				EndTime = new TimeOfDay(newRequestStartDateTime.AddHours(10).TimeOfDay)
			};

			Target.Persist(overtimeRequestForm);

			var overtimeRequest = PersonRequestRepository.Get(overtimeRequestForm.Id.Value);
			Assert.IsTrue(overtimeRequest.IsDenied);
			Assert.AreEqual(overtimeRequest.DenyReason, string.Format(Resources.OvertimeRequestDenyReasonNoPeriod, "11/10/2017 - 11/15/2017"));
		}

		private void setupIntradayStaffingForSkill(ISkill skill, double forecastedStaffing,
			double scheduledStaffing)
		{
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var period = getAvailablePeriod();
			period.DayCollection().ToList().ForEach(day =>
			{
				var utcDate = TimeZoneHelper.ConvertToUtc(day.Date,
					LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone());
				var staffingPeriodList = new List<StaffingPeriodData>();
				for (var i = 0; i < _intervals.Length; i++)
				{
					staffingPeriodList.Add(new StaffingPeriodData
					{
						ForecastedStaffing = forecastedStaffing,
						ScheduledStaffing = scheduledStaffing,
						Period = new DateTimePeriod(utcDate.Add(_intervals[i]), utcDate.Add(_intervals[i]).AddMinutes(15))
					});
				}

				SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(skill, new DateOnly(utcDate),
					staffingPeriodList, timeZone);
			});
		}

		private DateOnlyPeriod getAvailablePeriod()
		{
			var today = Now.ServerDate_DontUse();
			var period = new DateOnlyPeriod(today, today.AddDays(13)).Inflate(1);
			return period;
		}

		private ISkill setupPersonSkill()
		{
			var activity1 = createActivity("activity1");
			var skill1 = createSkill("skill1");
			var personSkill1 = createPersonSkill(activity1, skill1);
			addPersonSkillsToPersonPeriod(personSkill1);
			return skill1;
		}

		private IActivity createActivity(string name)
		{
			var activity = ActivityFactory.CreateActivity(name);
			activity.RequiresSkill = true;
			ActivityRepository.Add(activity);
			return activity;
		}

		private ISkill createSkill(string name)
		{
			var skill = SkillFactory.CreateSkill(name).WithId();
			skill.SkillType = skillType;
			skill.StaffingThresholds = new StaffingThresholds(new Percent(-0.3), new Percent(-0.1), new Percent(0.1));
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, new TimePeriod(8, 00, 21, 00));
			SkillRepository.Has(skill);
			return skill;
		}

		private IPersonSkill createPersonSkill(IActivity activity, ISkill skill)
		{
			skill.Activity = activity;
			var personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);
			return personSkill;
		}

		private void addPersonSkillsToPersonPeriod(params IPersonSkill[] personSkills)
		{
			var personPeriod = getOrAddPersonPeriod(Now.ServerDate_DontUse());
			foreach (var personSkill in personSkills)
			{
				personPeriod.AddPersonSkill(personSkill);
			}
		}

		private PersonPeriod getOrAddPersonPeriod(DateOnly startDate)
		{
			var personPeriod = (PersonPeriod)LoggedOnUser.CurrentUser().PersonPeriods(startDate.ToDateOnlyPeriod()).FirstOrDefault();
			if (personPeriod != null) return personPeriod;
			var team = TeamFactory.CreateTeam("team1", "site1");
			personPeriod =
				(PersonPeriod)
				PersonPeriodFactory.CreatePersonPeriod(startDate, PersonContractFactory.CreatePersonContract(), team);
			LoggedOnUser.CurrentUser().AddPersonPeriod(personPeriod);
			return personPeriod;
		}

		private TimeSpan[] createIntervals()
		{
			var intervals = new List<TimeSpan>();
			for (var i = 00; i < 1440; i += 15)
			{
				intervals.Add(TimeSpan.FromMinutes(i));
			}
			return intervals.ToArray();
		}
	}
}