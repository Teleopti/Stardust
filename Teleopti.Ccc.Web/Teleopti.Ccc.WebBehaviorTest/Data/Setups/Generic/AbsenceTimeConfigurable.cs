using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class AbsenceTimeConfigurable : IPostSetup
	{
		public DateTime Date { get; set; }
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

			var shrinkage = new CustomShrinkage("test", true);
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

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(Date).AddDays(-1), team, budgetGroup);
			person.AddPersonPeriod(personPeriod);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(person);
			//target = new ScheduleProjectionReadOnlyRepository(new CurrentUnitOfWork(GlobalUnitOfWorkState.CurrentUnitOfWorkFactory));
			target = new ScheduleProjectionReadOnlyRepository(new FixedCurrentUnitOfWork(uow));

			var period =
				new DateOnlyPeriod(new DateOnly(Date), new DateOnly(Date)).ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
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

			target.AddProjectedLayer(new DateOnly(Date), scenarioId, person.Id.GetValueOrDefault(), layer);

			//var result = target.AbsenceTimePerBudgetGroup(new DateOnlyPeriod(new DateOnly(Date).AddDays(-1), new DateOnly(Date).AddDays(1)),
			//										 budgetGroup, scenario);

			var usedAbsenceMinutes = TimeSpan.FromTicks(
				   target.AbsenceTimePerBudgetGroup(new DateOnlyPeriod(new DateOnly(Date).AddDays(-1), new DateOnly(Date).AddDays(1)),
													 budgetGroup, scenario).Sum(p => p.TotalContractTime)).TotalMinutes;
			Assert.That(usedAbsenceMinutes, Is.GreaterThan(0));
		}
	}
}