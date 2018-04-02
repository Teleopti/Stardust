using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[RequestsTest]
	public class OvertimeRequestPersisterTest : ISetup
	{
		public IOvertimeRequestPersister Target;
		public FakeLoggedOnUser LoggedOnUser;
		public INow Now;
		public IMultiplicatorDefinitionSetRepository MultiplicatorDefinitionSetRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillCombinationResourceRepository CombinationRepository;
		public FakePersonRequestRepository PersonRequestRepository;
		public ICurrentScenario Scenario;
		public FakeToggleManager ToggleManager;

		private IPerson _person;
		private DateTime _currentDateTime = new DateTime(2017, 11, 07);
		private readonly IMultiplicatorDefinitionSet _multiplicatorDefinitionSet 
			= new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime).WithId();
		private TimeSpan[] _intervals;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			_person = PersonFactory.CreatePerson();
			_intervals = createIntervals();

			system.UseTestDouble(new FakeLoggedOnUser(_person)).For<ILoggedOnUser>();

			var fakeMultiplicatorDefinitionSetRepository = new FakeMultiplicatorDefinitionSetRepository();
			fakeMultiplicatorDefinitionSetRepository.Has(_multiplicatorDefinitionSet);
			system.UseTestDouble(fakeMultiplicatorDefinitionSetRepository).For<IMultiplicatorDefinitionSetRepository>();

			system.UseTestDouble(new FakeScenarioRepository(new Scenario("default") { DefaultScenario = true }))
				.For<IScenarioRepository>();
			system.UseTestDouble<FakeTimeZoneGuard>().For<ITimeZoneGuard>();
			system.UseTestDouble(new FakeLinkProvider()).For<ILinkProvider>();
			system.UseTestDouble<FakeScheduleDictionary>().For<IScheduleDictionary>();
			system.UseTestDouble<FakeActivityRepository>().For<IActivityRepository>();
			system.UseTestDouble<FakeActivityRepository>().For<IProxyForId<IActivity>>();
			system.UseTestDouble<FakePersonRepository>().For<IProxyForId<IPerson>>();
			system.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			system.UseTestDouble<FakeSkillTypeRepository>().For<ISkillTypeRepository>();
			system.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>();
			system.UseTestDouble<FakeSkillCombinationResourceRepository>().For<ISkillCombinationResourceRepository>();

			system.UseTestDouble<RequestApprovalServiceFactory>().For<IRequestApprovalServiceFactory>();
			system.UseTestDouble(new ThisIsNow(_currentDateTime)).For<INow>();
		}

		[Test]
		public void ShouldChangeStatusToPendingWhenPersistOvertimeRequest()
		{
			_person.WorkflowControlSet = new WorkflowControlSet
			{
				AutoGrantOvertimeRequest = false
			};
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
			ToggleManager.Enable(Toggles.OvertimeRequestPeriodSetting_46417);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);
			var now = new DateOnly(_currentDateTime);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(now, now.AddDays(8)),
				OrderIndex = 0
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
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
			Assert.AreEqual(overtimeRequest.DenyReason, Resources.OvertimeRequestDenyReasonAutodeny);
		}

		private void setupIntradayStaffingForSkill(ISkill skill, double forecastedStaffing,
			double scheduledStaffing)
		{
			var period = getAvailablePeriod();
			period.DayCollection().ToList().ForEach(day =>
			{
				var utcDate = TimeZoneHelper.ConvertToUtc(day.Date,
					LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone());

				for (var i = 0; i < _intervals.Length; i++)
				{
					CombinationRepository.AddSkillCombinationResource(new DateTime(),
						new[]
						{
							new SkillCombinationResource
							{
								StartDateTime = utcDate.Add(_intervals[i]),
								EndDateTime = utcDate.Add(_intervals[i]).AddMinutes(15),
								Resource = scheduledStaffing,
								SkillCombination = new[] {skill.Id.Value}
							}
						});
				}

				var timePeriodTuples = new List<Tuple<TimePeriod, double>>();
				for (var i = 0; i < _intervals.Length; i++)
				{
					timePeriodTuples.Add(new Tuple<TimePeriod, double>(
						new TimePeriod(_intervals[i], _intervals[i].Add(TimeSpan.FromMinutes(15))),
						forecastedStaffing));
				}
				var skillDay = skill.CreateSkillDayWithDemandOnInterval(Scenario.Current(), day, 0,
					timePeriodTuples.ToArray());
				skillDay.SkillDataPeriodCollection.ForEach(s => { s.Shrinkage = new Percent(0.5); });
				SkillDayRepository.Has(skillDay);
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
			skill.SkillType.Description = new Description("SkillTypeInboundTelephony");
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