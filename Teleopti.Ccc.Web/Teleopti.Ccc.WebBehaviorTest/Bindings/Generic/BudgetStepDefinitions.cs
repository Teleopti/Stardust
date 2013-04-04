using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class BudgetStepDefinitions
	{
		[Given(@"there is a budgetgroup '(.*)'")]
		public void GivenThereIsABudgetgroup(string name)
		{
			var budgetConfigurable = new BudgetGroupConfigurable(name);
			UserFactory.User().Setup(budgetConfigurable);
		}

		[Given(@"there is a budgetday")]
		public void GivenThereIsAnBudgetday(Table table)
		{
			var budgetday = table.CreateInstance<BudgetdayConfigurable>();
			UserFactory.User().Setup(budgetday);
		}


		//henke hitta på ett bättre namn
		[Given(@"there is a \(readonly\) PersonScheduleDayModel")]
		public void GivenThereIsAReadonlyPersonScheduleDayModel(Table table)
		{
			var scheduleReadOnlyProjection = table.CreateInstance<ReadModelScheduleProjectionConfigurable>();
			UserFactory.User().Setup(scheduleReadOnlyProjection);
		}



		//henke
		[Then(@"I should see stuff for '(.*)' to '(.*)'")]
		public void ThenIShouldSeeStuffForTo(DateTime from, DateTime to)
		{

			//
		}

		[Given(@"I have stuff for '(.*)' to '(.*)'")]
		public void GivenIHaveStuffForTo(DateTime from, DateTime to)
		{
			var removeMe = new RemoveMe() {From = from, To = to};
			UserFactory.User().Setup(removeMe);
		}
	}

	public class RemoveMe : IPostSetup
	{
		public DateTime From { get; set; }
		public DateTime To { get; set; }
		public static ISession Session { get; set; }
    	private IState stateMock;
		protected IPerson LoggedOnPerson { get; private set; }
    	protected IRepositoryFactory RepositoryFactory { get; private set; }
    	protected MockRepository Mocks { get; private set; }

		protected virtual void SetupForRepositoryTest() { }

		protected void PersistAndRemoveFromUnitOfWork(IEntity obj)
		{
			Session.SaveOrUpdate(obj);
			Session.Flush();
			Session.Evict(obj);
		}

		public void Apply(IPerson user, IUnitOfWork uow)
		{
			IScheduleProjectionReadOnlyRepository target;
			IScenario scenario;
			IPerson person;
			IAbsence absence;
			IBudgetGroup budgetGroup;
			Guid scenarioId;

			BusinessUnitFactory.SetBusinessUnitUsedInTestToNull();
			RepositoryFactory = new RepositoryFactory();

			Mocks = new MockRepository();
			stateMock = Mocks.StrictMock<IState>();

			var buGuid = Guid.NewGuid();
			BusinessUnitFactory.BusinessUnitUsedInTest.SetId(buGuid);
			LoggedOnPerson = PersonFactory.CreatePersonWithBasicPermissionInfo(string.Concat("logOnName", Guid.NewGuid().ToString()), string.Empty);

			StateHolderProxyHelper.ClearAndSetStateHolder(Mocks,
													 LoggedOnPerson,
													 BusinessUnitFactory.BusinessUnitUsedInTest,
													 null,
													 stateMock);

			Session =
				(ISession)
				uow.GetType().GetProperty("Session", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(
					uow, null);

			((IDeleteTag)LoggedOnPerson).SetDeleted();
			Session.Save(LoggedOnPerson);

			//force a insert
			BusinessUnitFactory.BusinessUnitUsedInTest.SetId(null);
			Session.Save(BusinessUnitFactory.BusinessUnitUsedInTest, buGuid);
			Session.Flush();

			SetupForRepositoryTest();

			//scenario = ScenarioFactory.CreateScenarioAggregate();
			//scenario = GlobalDataContext.Data().Data<CommonScenario>().Scenario;
			var scenarioRepository = new ScenarioRepository(uow);
			scenario = scenarioRepository.LoadAll().First();
			PersistAndRemoveFromUnitOfWork(scenario);
			scenarioId = scenario.Id.GetValueOrDefault();

			person = PersonFactory.CreatePerson();
			PersistAndRemoveFromUnitOfWork(person);

			var absenceRepository = new AbsenceRepository(uow);
			absence = absenceRepository.LoadAll().First();
			//absence = AbsenceFactory.CreateAbsence("Vacation");
			PersistAndRemoveFromUnitOfWork(absence);

			budgetGroup = new BudgetGroup { Name = "My Budget", TimeZone = TeleoptiPrincipal.Current.Regional.TimeZone };
			//var budgetGroupRepository = new BudgetGroupRepository(uow);
			//budgetGroup = budgetGroupRepository.LoadAll().First();

			var shrinkage = new CustomShrinkage("test",true);
			shrinkage.AddAbsence(absence);
			budgetGroup.AddCustomShrinkage(shrinkage);
			PersistAndRemoveFromUnitOfWork(budgetGroup);

			//var site = SiteFactory.CreateSimpleSite();
			var siteRepository = new SiteRepository(uow);
			var site = siteRepository.LoadAll().First();
			PersistAndRemoveFromUnitOfWork(site);

			//var team = TeamFactory.CreateSimpleTeam("team test");
			var teamRepository = new TeamRepository(uow);
			var team = teamRepository.LoadAll().First();
			team.Site = site;
			PersistAndRemoveFromUnitOfWork(team);

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(-1), team, budgetGroup);
			person.AddPersonPeriod(personPeriod);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(person);

			target = new ScheduleProjectionReadOnlyRepository(GlobalUnitOfWorkState.UnitOfWorkFactory);
		
			var period =
				new DateOnlyPeriod(DateOnly.Today, DateOnly.Today).ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var layer = new DenormalizedScheduleProjectionLayer
			{
				ContractTime = TimeSpan.FromHours(8),
				WorkTime = TimeSpan.FromHours(8),
				DisplayColor = Color.Bisque.ToArgb(),
				Name = "holiday",
				ShortName = "ho",
				StartDateTime = period.StartDateTime,
				EndDateTime = period.EndDateTime,
				PayloadId = absence.Id.GetValueOrDefault()
			};

			target.AddProjectedLayer(DateOnly.Today, scenarioId, person.Id.GetValueOrDefault(), layer);

			var result = target.AbsenceTimePerBudgetGroup(new DateOnlyPeriod(DateOnly.Today.AddDays(-1), DateOnly.Today.AddDays(1)),
													 budgetGroup, scenario);
			Assert.That(result, Is.Not.Empty);
		}
	}
}