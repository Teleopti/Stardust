using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class ScheduleControllerTest
	{
		[Test]
		public void ShouldScheduleFixedStaff()
		{
			var period = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			var agent = PersonFactory.CreatePersonWithPersonPeriod(period.StartDate);
			agent.SetId(Guid.NewGuid());
			var personRepository = new FakePersonRepository(agent);

			FakeCurrentTeleoptiPrincipal currentTeleoptiPrincipal;
			ISkillRepository skillRepository;
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;
			IScheduleRepository scheduleRepository;
			IDayOffTemplateRepository dayOffTemplateRepository;
			IScheduleCommand scheduleCommand;
			SchedulerStateHolder stateHolder;
			GroupPagePerDateHolder groupPagePerDateHolder;
			IRequiredScheduleHelper requiredScheduleHelper;
			IPeopleAndSkillLoaderDecider peopleAndSkillLoaderDecider;
			IScheduleRangePersister persister;
			var scenario = commonMocks(period, agent, out currentTeleoptiPrincipal, out skillRepository, out currentUnitOfWorkFactory, out scheduleRepository, out dayOffTemplateRepository, out scheduleCommand, out stateHolder, out groupPagePerDateHolder, out requiredScheduleHelper, out peopleAndSkillLoaderDecider, out persister);

			var target = new ScheduleController(new SetupStateHolderForWebScheduling(new FakeScenarioRepository(scenario),
				MockRepository.GenerateMock<ISkillDayLoadHelper>(), skillRepository,
				scheduleRepository, MockRepository.GenerateMock<IPersonAbsenceAccountRepository>(),
				peopleAndSkillLoaderDecider,
				currentTeleoptiPrincipal,
				currentUnitOfWorkFactory, () => stateHolder),new FixedStaffLoader(personRepository), 
				dayOffTemplateRepository,MockRepository.GenerateMock<IActivityRepository>(),
				() => MockRepository.GenerateMock<IFixedStaffSchedulingService>(),
				() => scheduleCommand,
				() => stateHolder,
				()=> requiredScheduleHelper, () => groupPagePerDateHolder,
				() => new ScheduleTagSetter(NullScheduleTag.Instance),
				()=> new PersonSkillProvider(),
				persister, MockRepository.GenerateMock<IDayOffsInPeriodCalculator>(), new VoilatedSchedulePeriodBusinessRule());
			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithFullPermission()))
			{
				var result =
					(OkNegotiatedContentResult<SchedulingResultModel>)
						target.FixedStaff(new FixedStaffSchedulingInput {StartDate = period.StartDate.Date, EndDate = period.EndDate.Date});

				result.Content.Should().Not.Be.Null();
			}
			scheduleCommand.AssertWasCalled(x => x.Execute(null,null,stateHolder,null,groupPagePerDateHolder,requiredScheduleHelper,null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldReturnPersonOutofPlanningPeriodResult()
		{
			var period = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			IPerson agent1 = new Person();
			agent1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(agent1, period.StartDate.AddDays(3));
			agent1.SetId(Guid.NewGuid());
			var personRepository = new FakePersonRepository(agent1);
			int targetDaysOff;
			IList<IScheduleDay> dayOffNow;

			FakeCurrentTeleoptiPrincipal currentTeleoptiPrincipal;
			ISkillRepository skillRepository;
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;
			IScheduleRepository scheduleRepository;
			IDayOffTemplateRepository dayOffTemplateRepository;
			IScheduleCommand scheduleCommand;
			SchedulerStateHolder stateHolder;
			GroupPagePerDateHolder groupPagePerDateHolder;
			IRequiredScheduleHelper requiredScheduleHelper;
			IPeopleAndSkillLoaderDecider peopleAndSkillLoaderDecider;
			IScheduleRangePersister persister;
			var scenario = commonMocks(period, agent1, out currentTeleoptiPrincipal, out skillRepository, out currentUnitOfWorkFactory, out scheduleRepository, out dayOffTemplateRepository, out scheduleCommand, out stateHolder, out groupPagePerDateHolder, out requiredScheduleHelper, out peopleAndSkillLoaderDecider, out persister);
			var dayOffInPeriodCalc = MockRepository.GenerateMock<IDayOffsInPeriodCalculator>();
			var target = new ScheduleController(new SetupStateHolderForWebScheduling(new FakeScenarioRepository(scenario),
				MockRepository.GenerateMock<ISkillDayLoadHelper>(), skillRepository,
				scheduleRepository, MockRepository.GenerateMock<IPersonAbsenceAccountRepository>(),
				peopleAndSkillLoaderDecider,
				currentTeleoptiPrincipal,
				currentUnitOfWorkFactory, () => stateHolder), new FixedStaffLoader(personRepository),
				dayOffTemplateRepository, MockRepository.GenerateMock<IActivityRepository>(),
				() => MockRepository.GenerateMock<IFixedStaffSchedulingService>(),
				() => scheduleCommand,
				() => stateHolder,
				() => requiredScheduleHelper, () => groupPagePerDateHolder,
				() => new ScheduleTagSetter(NullScheduleTag.Instance),
				() => new PersonSkillProvider(),
				persister, dayOffInPeriodCalc, new VoilatedSchedulePeriodBusinessRule());
			
			dayOffInPeriodCalc.Stub(x => x.HasCorrectNumberOfDaysOff(agent1.VirtualSchedulePeriod(period.StartDate),out targetDaysOff,out dayOffNow))
				.Return(true);
			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithFullPermission()))
			{
				var result =
					(OkNegotiatedContentResult<SchedulingResultModel>)
						target.FixedStaff(new FixedStaffSchedulingInput { StartDate = period.StartDate.Date, EndDate = period.EndDate.Date });

				result.Content.Should().Not.Be.Null();
				result.Content.BusinessRulesValidationResults.ToList().Count.Should().Be.EqualTo(1);
			}
			
		}

		[Test]
		public void ShouldReturnTargetDayOffNotFullfilled()
		{
			var period = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			IPerson agent1 = new Person();
			agent1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(agent1, period.StartDate);
			agent1.SetId(Guid.NewGuid());
			var personRepository = new FakePersonRepository(agent1);

			FakeCurrentTeleoptiPrincipal currentTeleoptiPrincipal;
			ISkillRepository skillRepository;
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;
			IScheduleRepository scheduleRepository;
			IDayOffTemplateRepository dayOffTemplateRepository;
			IScheduleCommand scheduleCommand;
			SchedulerStateHolder stateHolder;
			GroupPagePerDateHolder groupPagePerDateHolder;
			IRequiredScheduleHelper requiredScheduleHelper;
			IPeopleAndSkillLoaderDecider peopleAndSkillLoaderDecider;
			IScheduleRangePersister persister;
			var scenario = commonMocks(period, agent1, out currentTeleoptiPrincipal, out skillRepository, out currentUnitOfWorkFactory, out scheduleRepository, out dayOffTemplateRepository, out scheduleCommand, out stateHolder, out groupPagePerDateHolder, out requiredScheduleHelper, out peopleAndSkillLoaderDecider, out persister);

			var target = new ScheduleController(new SetupStateHolderForWebScheduling(new FakeScenarioRepository(scenario),
				MockRepository.GenerateMock<ISkillDayLoadHelper>(), skillRepository,
				scheduleRepository, MockRepository.GenerateMock<IPersonAbsenceAccountRepository>(),
				peopleAndSkillLoaderDecider,
				currentTeleoptiPrincipal,
				currentUnitOfWorkFactory, () => stateHolder), new FixedStaffLoader(personRepository),
				dayOffTemplateRepository, MockRepository.GenerateMock<IActivityRepository>(),
				() => MockRepository.GenerateMock<IFixedStaffSchedulingService>(),
				() => scheduleCommand,
				() => stateHolder,
				() => requiredScheduleHelper, () => groupPagePerDateHolder,
				() => new ScheduleTagSetter(NullScheduleTag.Instance),
				() => new PersonSkillProvider(),
				persister, MockRepository.GenerateMock<IDayOffsInPeriodCalculator>(), new VoilatedSchedulePeriodBusinessRule());
			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithFullPermission()))
			{
				var result =
					(OkNegotiatedContentResult<SchedulingResultModel>)
						target.FixedStaff(new FixedStaffSchedulingInput { StartDate = period.StartDate.Date, EndDate = period.EndDate.Date });

				result.Content.Should().Not.Be.Null();
				result.Content.BusinessRulesValidationResults.ToList().Count.Should().Be.EqualTo(1);
			}

		}

		[Test]
		public void ShouldReturnNumberOfScheduledAgents()
		{
			var period = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			IPerson agent1 = new Person();
			agent1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(agent1, period.StartDate);
			agent1.SetId(Guid.NewGuid());
			var personRepository = new FakePersonRepository(agent1);
			int targetDaysOff;
			IList<IScheduleDay> dayOffNow;

			FakeCurrentTeleoptiPrincipal currentTeleoptiPrincipal;
			ISkillRepository skillRepository;
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;
			IScheduleRepository scheduleRepository;
			IDayOffTemplateRepository dayOffTemplateRepository;
			IScheduleCommand scheduleCommand;
			SchedulerStateHolder stateHolder;
			GroupPagePerDateHolder groupPagePerDateHolder;
			IRequiredScheduleHelper requiredScheduleHelper;
			IPeopleAndSkillLoaderDecider peopleAndSkillLoaderDecider;
			IScheduleRangePersister persister;
			var scenario = commonMocks(period, agent1, out currentTeleoptiPrincipal, out skillRepository, out currentUnitOfWorkFactory, out scheduleRepository, out dayOffTemplateRepository, out scheduleCommand, out stateHolder, out groupPagePerDateHolder, out requiredScheduleHelper, out peopleAndSkillLoaderDecider, out persister);
			var dayOffInPeriodCalc = MockRepository.GenerateMock<IDayOffsInPeriodCalculator>();
			var target = new ScheduleController(new SetupStateHolderForWebScheduling(new FakeScenarioRepository(scenario),
				MockRepository.GenerateMock<ISkillDayLoadHelper>(), skillRepository,
				scheduleRepository, MockRepository.GenerateMock<IPersonAbsenceAccountRepository>(),
				peopleAndSkillLoaderDecider,
				currentTeleoptiPrincipal,
				currentUnitOfWorkFactory, () => stateHolder), new FixedStaffLoader(personRepository),
				dayOffTemplateRepository, MockRepository.GenerateMock<IActivityRepository>(),
				() => MockRepository.GenerateMock<IFixedStaffSchedulingService>(),
				() => scheduleCommand,
				() => stateHolder,
				() => requiredScheduleHelper, () => groupPagePerDateHolder,
				() => new ScheduleTagSetter(NullScheduleTag.Instance),
				() => new PersonSkillProvider(),
				persister, dayOffInPeriodCalc, new VoilatedSchedulePeriodBusinessRule());

			dayOffInPeriodCalc.Stub(x => x.HasCorrectNumberOfDaysOff(agent1.VirtualSchedulePeriod(period.StartDate), out targetDaysOff, out dayOffNow))
				.Return(true);
			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithFullPermission()))
			{
				var result =
					(OkNegotiatedContentResult<SchedulingResultModel>)
						target.FixedStaff(new FixedStaffSchedulingInput { StartDate = period.StartDate.Date, EndDate = period.EndDate.Date });

				result.Content.Should().Not.Be.Null();
				result.Content.ScheduledAgentsCount.Should().Be.EqualTo(1);
			}

		}

		private static Scenario commonMocks(DateOnlyPeriod period, IPerson agent,
			out FakeCurrentTeleoptiPrincipal currentTeleoptiPrincipal, out ISkillRepository skillRepository,
			out ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, out IScheduleRepository scheduleRepository,
			out IDayOffTemplateRepository dayOffTemplateRepository, out IScheduleCommand scheduleCommand,
			out SchedulerStateHolder stateHolder, out GroupPagePerDateHolder groupPagePerDateHolder,
			out IRequiredScheduleHelper requiredScheduleHelper, out IPeopleAndSkillLoaderDecider peopleAndSkillLoaderDecider,
			out IScheduleRangePersister persister)
		{
			var scenario = ScenarioFactory.CreateScenario("Default", true, true);
			var dateTimePeriod = period.ToDateTimePeriod(TimeZoneInfo.Utc);
			currentTeleoptiPrincipal =
				new FakeCurrentTeleoptiPrincipal(new TeleoptiPrincipal(new TeleoptiIdentity("", null, null, null),
					PersonFactory.CreatePerson(new Name("Anna", "Andersson"), TimeZoneInfo.Utc)));

			skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			dayOffTemplateRepository = MockRepository.GenerateMock<IDayOffTemplateRepository>();
			scheduleCommand = MockRepository.GenerateMock<IScheduleCommand>();
			stateHolder = new SchedulerStateHolder(new SchedulingResultStateHolder(),
				MockRepository.GenerateMock<ICommonStateHolder>(), currentTeleoptiPrincipal);
			groupPagePerDateHolder = new GroupPagePerDateHolder();
			requiredScheduleHelper = MockRepository.GenerateMock<IRequiredScheduleHelper>();
			peopleAndSkillLoaderDecider = MockRepository.GenerateMock<IPeopleAndSkillLoaderDecider>();
			var schedules = new ScheduleDictionaryForTest(scenario, dateTimePeriod);
			persister = MockRepository.GenerateMock<IScheduleRangePersister>();

			var range = new ScheduleRange(schedules, new ScheduleParameters(scenario, agent, dateTimePeriod));
			schedules.AddTestItem(agent, range);
			skillRepository.Stub(x => x.FindAllWithSkillDays(period)).Return(new[] {SkillFactory.CreateSkill("Direct Sales")});
			currentUnitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory())
				.Return(MockRepository.GenerateMock<IUnitOfWorkFactory>());
			scheduleRepository.Stub(x => x.FindSchedulesForPersons(null, scenario, null, null, null)).IgnoreArguments()
				.Return(schedules);
			dayOffTemplateRepository.Stub(x => x.FindAllDayOffsSortByDescription()).Return(new[] {DayOffFactory.CreateDayOff()});
			peopleAndSkillLoaderDecider.Stub(x => x.Execute(scenario, dateTimePeriod, new[] {agent}))
				.Return(MockRepository.GenerateMock<ILoaderDeciderResult>());
			persister.Stub(x => x.Persist(range)).Return(new PersistConflict[] {});
			return scenario;
		}
	}
}