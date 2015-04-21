using System;
using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
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
			var scenario = ScenarioFactory.CreateScenario("Default", true, true);
			var period = new DateOnlyPeriod(2015,5,1,2015,5,31);
			var dateTimePeriod = period.ToDateTimePeriod(TimeZoneInfo.Utc);
			var agent = PersonFactory.CreatePersonWithPersonPeriod(period.StartDate);
			agent.SetId(Guid.NewGuid());
			var personRepository = new FakePersonRepository(agent);
			var currentTeleoptiPrincipal = new FakeCurrentTeleoptiPrincipal(new TeleoptiPrincipal(new TeleoptiIdentity("", null, null, null),
				PersonFactory.CreatePerson(new Name("Anna", "Andersson"), TimeZoneInfo.Utc)));

			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			var dayOffTemplateRepository = MockRepository.GenerateMock<IDayOffTemplateRepository>();
			var scheduleCommand = MockRepository.GenerateMock<IScheduleCommand>();
			var stateHolder = new SchedulerStateHolder(new SchedulingResultStateHolder(), MockRepository.GenerateMock<ICommonStateHolder>(), currentTeleoptiPrincipal);
			var groupPagePerDateHolder = new GroupPagePerDateHolder();
			var requiredScheduleHelper = MockRepository.GenerateMock<IRequiredScheduleHelper>();
			var peopleAndSkillLoaderDecider = MockRepository.GenerateMock<IPeopleAndSkillLoaderDecider>();
			var schedules = new ScheduleDictionaryForTest(scenario, dateTimePeriod);
			var persister = MockRepository.GenerateMock<IScheduleRangePersister>();

			var range = new ScheduleRange(schedules, new ScheduleParameters(scenario, agent, dateTimePeriod));
			schedules.AddTestItem(agent, range);
			skillRepository.Stub(x => x.FindAllWithSkillDays(period)).Return(new[] {SkillFactory.CreateSkill("Direct Sales")});
			currentUnitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory())
				.Return(MockRepository.GenerateMock<IUnitOfWorkFactory>());
			scheduleRepository.Stub(x => x.FindSchedulesForPersons(null, scenario, null, null, null)).IgnoreArguments()
				.Return(schedules);
			dayOffTemplateRepository.Stub(x => x.FindAllDayOffsSortByDescription()).Return(new[] {DayOffFactory.CreateDayOff()});
			peopleAndSkillLoaderDecider.Stub(x => x.Execute(scenario, dateTimePeriod, new []{agent}))
				.Return(MockRepository.GenerateMock<ILoaderDeciderResult>());
			persister.Stub(x => x.Persist(range)).Return(new PersistConflict[] {});

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
				persister);
			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithFullPermission()))
			{
				var result =
					(OkNegotiatedContentResult<SchedulingResultModel>)
						target.FixedStaff(new FixedStaffSchedulingInput {StartDate = period.StartDate.Date, EndDate = period.EndDate.Date});

				result.Content.Should().Not.Be.Null();
			}
			scheduleCommand.AssertWasCalled(x => x.Execute(null,null,stateHolder,null,groupPagePerDateHolder,requiredScheduleHelper,null), o => o.IgnoreArguments());
		}
	}
}