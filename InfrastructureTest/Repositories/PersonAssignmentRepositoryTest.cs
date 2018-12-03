	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using NUnit.Framework;
	using SharpTestsEx;
	using Teleopti.Ccc.Domain.Common;
	using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
	using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
	using Teleopti.Ccc.Domain.Scheduling;
	using Teleopti.Ccc.Domain.Scheduling.Assignment;
	using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
	using Teleopti.Ccc.Domain.UnitOfWork;
	using Teleopti.Ccc.Infrastructure.Foundation;
	using Teleopti.Ccc.Infrastructure.Repositories;
	using Teleopti.Ccc.TestCommon.FakeData;
	using Teleopti.Ccc.TestCommon.TestData;
	

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class PersonAssignmentRepositoryTest : RepositoryTest<IPersonAssignment>
	{
		private IPersonAssignmentRepository _rep;
		private IActivity _dummyActivity;
		private IPerson _dummyAgent;
		private IPerson _dummyAgent2;
		private IScenario _dummyScenario;
		private IShiftCategory _dummyCat;
		private IShiftCategory _dummyCategory;
		private IMultiplicatorDefinitionSet _definitionSet;
		private ISite site;

		protected override void ConcreteSetup()
		{
			_dummyCat = ShiftCategoryFactory.CreateShiftCategory("dummyCat");
			_rep = new PersonAssignmentRepository(UnitOfWork);
			_dummyActivity = new Activity("dummy") {DisplayColor = Color.DodgerBlue};
			PersistAndRemoveFromUnitOfWork(_dummyActivity);
			_dummyAgent = PersonFactory.CreatePerson("m");
			_dummyAgent2 = PersonFactory.CreatePerson("n");
			_dummyScenario = ScenarioFactory.CreateScenarioAggregate("Default", false);
			_dummyCategory = ShiftCategoryFactory.CreateShiftCategory("Morning");
			_definitionSet = new MultiplicatorDefinitionSet("sdf", MultiplicatorType.Overtime);

			PersistAndRemoveFromUnitOfWork(_dummyCategory);
			PersistAndRemoveFromUnitOfWork(_dummyAgent2);
			PersistAndRemoveFromUnitOfWork(_dummyScenario);
			PersistAndRemoveFromUnitOfWork(_dummyCat);
			PersistAndRemoveFromUnitOfWork(_definitionSet);

			PersonFactory.AddDefinitionSetToPerson(_dummyAgent, _definitionSet);
			var per = _dummyAgent.Period(new DateOnly(2000, 1, 1));
			site = SiteFactory.CreateSimpleSite("df");
			PersistAndRemoveFromUnitOfWork(site);
			per.Team.Site = site;
			PersistAndRemoveFromUnitOfWork(per.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(per.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(per.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(per.Team);
			PersistAndRemoveFromUnitOfWork(_dummyAgent);
		}

		protected override IPersonAssignment CreateAggregateWithCorrectBusinessUnit()
		{
			var ass = new PersonAssignment(_dummyAgent, _dummyScenario, new DateOnly(2000, 1, 1));
			ass.AddActivity(_dummyActivity, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
			ass.SetShiftCategory(_dummyCat);
			ass.AddPersonalActivity(_dummyActivity, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
			ass.AddOvertimeActivity(_dummyActivity, new DateTimePeriod(2000, 1, 1, 2000, 1, 2), _definitionSet);
			return ass;
		}

		protected override void VerifyAggregateGraphProperties(IPersonAssignment loadedAggregateFromDatabase)
		{
			IPersonAssignment org = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(org.Person.Name, loadedAggregateFromDatabase.Person.Name);
			Assert.AreEqual(org.PersonalActivities().Count(), loadedAggregateFromDatabase.PersonalActivities().Count());
			Assert.AreEqual(org.OvertimeActivities().Count(), loadedAggregateFromDatabase.OvertimeActivities().Count());
		}

		[Test]
		public void VerifyLoadGraphById()
		{
			IPersonAssignment ass = CreateAggregateWithCorrectBusinessUnit();
			TestRepository(CurrUnitOfWork).Add(ass);
			Session.Flush();

			IPersonAssignment loaded = new PersonAssignmentRepository(UnitOfWork).LoadAggregate(ass.Id.Value);
			Assert.AreEqual(ass.Id, loaded.Id);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(loaded.ShiftLayers));
		}

		[Test]
		public void ShouldLoadGraphByKey()
		{
			IPersonAssignment ass = CreateAggregateWithCorrectBusinessUnit();
			new PersonAssignmentRepository(UnitOfWork).Add(ass);
			Session.Flush();
	
			IPersonAssignment loaded = new PersonAssignmentRepository(UnitOfWork).LoadAggregate(new PersonAssignmentKey
			{
				Date = ass.Date,
				Scenario = ass.Scenario,
				Person = ass.Person
			});
			Assert.AreEqual(ass.Id, loaded.Id);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(loaded.ShiftLayers));
		}

		[Test]
		public void TestFetchDatabaseVersion()
		{
			var wrongScenario = new Scenario("sdf");
			PersistAndRemoveFromUnitOfWork(wrongScenario);
			var ass = new PersonAssignment(_dummyAgent, _dummyScenario, new DateOnly(1900, 1, 1));
			PersistAndRemoveFromUnitOfWork(ass);
			var noHit1 = new PersonAssignment(_dummyAgent, _dummyScenario, new DateOnly(1800, 1, 1));
			PersistAndRemoveFromUnitOfWork(noHit1);
			var noHit2 = new PersonAssignment(_dummyAgent, wrongScenario, new DateOnly(1900, 1, 1));
			PersistAndRemoveFromUnitOfWork(noHit2);
			var noHit3 = new PersonAssignment(_dummyAgent2, _dummyScenario, new DateOnly(1900, 1, 1));
			PersistAndRemoveFromUnitOfWork(noHit3);
			var loaded = new PersonAssignmentRepository(UnitOfWork).FetchDatabaseVersions(new DateOnlyPeriod(1880, 1, 1, 1910, 1, 1), _dummyScenario, _dummyAgent);
			var assLoaded = loaded.Single();
			assLoaded.Id.Should().Be.EqualTo(ass.Id.Value);
		}

		[Test]
		public void VerifyLoadGraphByIdReturnsNullIfNotExists()
		{
			IPersonAssignment loaded = new PersonAssignmentRepository(UnitOfWork).LoadAggregate(Guid.NewGuid());
			Assert.IsNull(loaded);
		}

		[Test]
		public void CannotUseNullAsScenario()
		{
			Assert.Throws<ArgumentNullException>(() => _rep.Find(new DateOnlyPeriod(2000, 1, 1, 2001, 1, 1), null));
		}

		[Test]
		public void CanFindAssignmentsByDatesAndScenario()
		{
			var notToFindScenario = new Scenario("NotToFind");
			var searchPeriod = new DateOnlyPeriod(2007, 1, 1, 2007, 1, 2);

			////////////setup////////////////////////////////////////////////////////////////
			IPersonAssignment agAssValid = PersonAssignmentFactory.CreateAssignmentWithMainShift(_dummyAgent,
				_dummyScenario, _dummyActivity, new DateTimePeriod(new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2007, 1, 2, 0, 0, 0, DateTimeKind.Utc)), _dummyCategory);
			agAssValid.AddPersonalActivity(_dummyActivity, new DateTimePeriod(2007, 1, 1, 2007, 1, 2));
			agAssValid.AddPersonalActivity(_dummyActivity, new DateTimePeriod(2007, 1, 1, 2007, 1, 2));
			agAssValid.AddPersonalActivity(_dummyActivity, new DateTimePeriod(2007, 1, 1, 2007, 1, 2));
			agAssValid.AddOvertimeActivity(_dummyActivity, new DateTimePeriod(2007, 1, 1, 2007, 1, 2), _definitionSet);

			IPersonAssignment agAssInvalid = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(_dummyAgent,
				notToFindScenario, _dummyActivity, new DateTimePeriod(2006, 12, 31, 2007, 1, 1));

			PersistAndRemoveFromUnitOfWork(notToFindScenario);
			TestRepository(CurrUnitOfWork).Add(agAssValid);
			TestRepository(CurrUnitOfWork).Add(agAssInvalid);
			Session.Flush();
			Session.Clear();
			/////////////////////////////////////////////////////////////////////////////////

			var retList = _rep.Find(searchPeriod, _dummyScenario).ToArray();

			Assert.IsTrue(retList.Contains(agAssValid));
			Assert.AreEqual(1, retList.Length);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(retList[0].ShiftLayers));
		}

		[Test]
		public void VerifyAssignmentsCannotBeReadForDeletedPerson()
		{
			IPersonAssignment agAssValid = PersonAssignmentFactory.CreateAssignmentWithMainShift(_dummyAgent,
				_dummyScenario, _dummyActivity, new DateTimePeriod(new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2007, 1, 2, 0, 0, 0, DateTimeKind.Utc)), _dummyCategory);
			TestRepository(CurrUnitOfWork).Add(agAssValid);
			Session.Flush();
			new PersonRepository(new ThisUnitOfWork(UnitOfWork)).Remove(_dummyAgent);
			PersistAndRemoveFromUnitOfWork(_dummyAgent);

			Assert.AreEqual(0, _rep.Find(new DateOnlyPeriod(2000, 1, 1, 2010, 1, 1), _dummyScenario).Count);
		}

		/// <summary>
		/// Determines whether this instance [can find agent assignments by dates and scenario].
		/// </summary>
		/// <remarks>
		/// Created by: Sumedah
		/// Created date: 2008-03-07
		/// </remarks>
		[Test]
		public void CanFindAgentAssignmentsByDatesAndScenario()
		{
			var notToFindScenario = new Scenario("NotToFind");
			var searchPeriod = new DateOnlyPeriod(2007, 1, 1, 2007, 1, 2);

			#region setup test data

			// Assignments of _dummyAgent Data
			IPersonAssignment agAssValid = PersonAssignmentFactory.CreateAssignmentWithMainShift(_dummyAgent,
				_dummyScenario, _dummyActivity, new DateTimePeriod(new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2007, 1, 2, 0, 0, 0, DateTimeKind.Utc)), _dummyCategory);
			agAssValid.AddPersonalActivity(_dummyActivity, new DateTimePeriod(2007, 1, 1, 2007, 1, 2));
			agAssValid.AddPersonalActivity(_dummyActivity, new DateTimePeriod(2007, 1, 1, 2007, 1, 2));
			agAssValid.AddPersonalActivity(_dummyActivity, new DateTimePeriod(2007, 1, 1, 2007, 1, 2));

			IPersonAssignment agAssInvalid = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(_dummyAgent,
				notToFindScenario, _dummyActivity, new DateTimePeriod(2006, 12, 31, 2007, 1, 1));

			//Assignments of _dummyAgent2 
			IPersonAssignment agAssInvalid4 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_dummyAgent2,
				_dummyScenario, _dummyActivity, new DateTimePeriod(new DateTime(2007, 1, 1, 4, 0, 0, DateTimeKind.Utc),
					new DateTime(2007, 1, 2, 4, 0, 0, DateTimeKind.Utc)), _dummyCategory);

			PersistAndRemoveFromUnitOfWork(notToFindScenario);
			TestRepository(CurrUnitOfWork).Add(agAssValid);
			TestRepository(CurrUnitOfWork).Add(agAssInvalid);
			TestRepository(CurrUnitOfWork).Add(agAssInvalid4);
			Session.Flush();
			Session.Clear();
			

			#endregion

			IList<IPerson> persons = new List<IPerson> {_dummyAgent};
			ICollection<IPersonAssignment> retList = _rep.Find(persons, searchPeriod, _dummyScenario);
			Assert.IsTrue(retList.Contains(agAssValid));
			Assert.AreEqual(1, retList.Count);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(_dummyActivity));
			Assert.IsTrue(LazyLoadingManager.IsInitialized(_dummyCategory));
		}

		[Test, Ignore("Some nhibernate cache issue when using real datasource factory from container")]
		public void CanFindAgentAssignmentsByDatesAndScenarioAndSource()
		{
			var notToFindScenario = new Scenario("NotToFind");
			var searchPeriod = new DateOnlyPeriod(2007, 1, 1, 2007, 1, 2);

			#region setup test data

			// Assignments of _dummyAgent Data
			IPersonAssignment agAssValid = PersonAssignmentFactory.CreateAssignmentWithMainShift(_dummyAgent,
				_dummyScenario, _dummyActivity, new DateTimePeriod(new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2007, 1, 2, 0, 0, 0, DateTimeKind.Utc)), _dummyCategory);
			agAssValid.AddPersonalActivity(_dummyActivity, new DateTimePeriod(2007, 1, 1, 2007, 1, 2));
			agAssValid.AddPersonalActivity(_dummyActivity, new DateTimePeriod(2007, 1, 1, 2007, 1, 2));
			agAssValid.AddPersonalActivity(_dummyActivity, new DateTimePeriod(2007, 1, 1, 2007, 1, 2));
			agAssValid.Source = ScheduleSource.WebScheduling;

			IPersonAssignment agAssInvalid = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(_dummyAgent,
				notToFindScenario, _dummyActivity, new DateTimePeriod(2006, 12, 31, 2007, 1, 1));
			agAssInvalid.Source = ScheduleSource.WebScheduling;

			//Assignments of _dummyAgent2 
			IPersonAssignment agAssInvalid4 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_dummyAgent2,
				_dummyScenario, _dummyActivity, new DateTimePeriod(new DateTime(2007, 1, 1, 4, 0, 0, DateTimeKind.Utc),
					new DateTime(2007, 1, 2, 4, 0, 0, DateTimeKind.Utc)), _dummyCategory);

			PersistAndRemoveFromUnitOfWork(notToFindScenario);
			TestRepository(CurrUnitOfWork).Add(agAssValid);
			TestRepository(CurrUnitOfWork).Add(agAssInvalid);
			TestRepository(CurrUnitOfWork).Add(agAssInvalid4);
			Session.Flush();
			Session.Clear();

			#endregion

			IList<IPerson> persons = new List<IPerson> { _dummyAgent, _dummyAgent2 };
			ICollection<IPersonAssignment> retList = _rep.Find(persons, searchPeriod, _dummyScenario, ScheduleSource.WebScheduling);
			Assert.IsTrue(retList.Contains(agAssValid));
			Assert.AreEqual(1, retList.Count);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(_dummyActivity));
			Assert.IsTrue(LazyLoadingManager.IsInitialized(_dummyCategory));
		}

		/// <summary>
		/// Verifies that version works with no reloading.
		/// </summary>
		[Test]
		public void VerifyVersionWorksWithNoReloading()
		{
			try
			{
				CleanUpAfterTest();

				//setup
				IPersonAssignment org = CreateAggregateWithCorrectBusinessUnit();
				IVersioned casted = org;
				_rep.Add(org);
				UnitOfWork.PersistAll();
				Assert.AreEqual(1, casted.Version);

				//Do change
				var loaded = _rep.Load(org.Id.Value);
				loaded.AddPersonalActivity(_dummyActivity, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
				UnitOfWork.PersistAll();
				loaded = _rep.Load(org.Id.Value);
				Assert.AreEqual(2, casted.Version);

				//Check history
				Assert.IsNotNull(loaded.UpdatedOn);
				Assert.AreSame(LoggedOnPerson, loaded.UpdatedBy);

				UnitOfWork.PersistAll();

			}
			finally
			{
				SetupFixtureForAssembly.RestoreCcc7Database();
			}
		}

		/// <summary>
		/// Verifies that version works when aggregate reloaded.
		/// </summary>
		[Test]
		public void VerifyVersionWorksWhenAggregateReloaded()
		{
			try
			{
				CleanUpAfterTest();

				//setup
				IPersonAssignment org = CreateAggregateWithCorrectBusinessUnit();
				_rep.Add(org);
				UnitOfWork.PersistAll();
				UnitOfWork.Remove(org);
				Assert.AreEqual(1, (org).Version);

				//Do change
				var loaded = _rep.LoadAggregate(org.Id.Value);
				loaded.AddPersonalActivity(_dummyActivity, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
				UnitOfWork.PersistAll();
				UnitOfWork.Remove(loaded);
				loaded = _rep.LoadAggregate(org.Id.Value);
				Assert.AreEqual(2, (loaded).Version);

				Assert.IsNotNull(loaded.UpdatedOn);
				Assert.AreEqual(LoggedOnPerson, loaded.UpdatedBy);
			}
			finally
			{
				SetupFixtureForAssembly.RestoreCcc7Database();
			}
		}

		[Test]
		public void ShouldCreateRepositoryWithUnitOfWorkFactory()
		{
			IPersonAssignmentRepository personAssignmentRepository = new PersonAssignmentRepository(UnitOfWork);
			Assert.IsNotNull(personAssignmentRepository);
		}

		[Test]
		public void ShouldPersistDayoff()
		{
			var template = new DayOffTemplate(new Description("hej"));
			PersistAndRemoveFromUnitOfWork(template);
			var ass = new PersonAssignment(_dummyAgent, _dummyScenario, new DateOnly(2000, 1, 1));
			ass.SetDayOff(template);
			PersistAndRemoveFromUnitOfWork(ass);
			_rep.Get(ass.Id.Value).DayOff().Description.Should().Be.EqualTo(new Description("hej"));
		}

		[Test]
		public void ShouldNotFetchIfScenarioIsNotLoggedOnBusinessUnit()
		{
			var bu = new BusinessUnit("wrong bu");
			PersistAndRemoveFromUnitOfWork(bu);
			var scenarioWrongBu = new Scenario("wrong");
			scenarioWrongBu.SetBusinessUnit(bu);
			PersistAndRemoveFromUnitOfWork(scenarioWrongBu);
			var ass = new PersonAssignment(_dummyAgent, scenarioWrongBu, new DateOnly(2000, 1, 1));
			PersistAndRemoveFromUnitOfWork(ass);

			_rep.Find(new DateOnlyPeriod(1900, 1, 1, 2100, 1, 1), scenarioWrongBu).Should().Be.Empty();
			_rep.Find(new[] {_dummyAgent}, new DateOnlyPeriod(1900, 1, 1, 2100, 1, 1), scenarioWrongBu).Should().Be.Empty();
		}

		[Test]
		public void ShouldQueryScheduleLoadedTime()
		{
			var timestamp = _rep.GetScheduleLoadedTime();
			timestamp.Should().Not.Be.EqualTo(new DateTime());
		}

		[Test]
		public void ShouldCheckIfNoAgentsScheduled()
		{
			_rep.IsThereScheduledAgents(site.BusinessUnit.Id.Value, new DateOnlyPeriod()).Should().Be.False();
		}
		
		[Test]
		public void ShouldCheckIfAnyAgentsScheduled()
		{
			var ass = CreateAggregateWithCorrectBusinessUnit();
			new PersonAssignmentRepository(UnitOfWork).Add(ass);

			var buWithoutAgentScheduled = new BusinessUnit("businessUnit");
			PersistAndRemoveFromUnitOfWork(buWithoutAgentScheduled);

			Session.Flush();

			var period1 = new DateOnlyPeriod(new DateOnly(2000, 1, 1), new DateOnly(2000, 1, 3));
			_rep.IsThereScheduledAgents(site.BusinessUnit.Id.Value, period1).Should().Be.True();
			_rep.IsThereScheduledAgents(buWithoutAgentScheduled.Id.Value, period1).Should().Be.False();

			var period2 = new DateOnlyPeriod(new DateOnly(2000, 1, 3), new DateOnly(2000, 1, 5));
			_rep.IsThereScheduledAgents(site.BusinessUnit.Id.Value, period2).Should().Be.False();
			_rep.IsThereScheduledAgents(buWithoutAgentScheduled.Id.Value, period2).Should().Be.False();
		}

		protected override Repository<IPersonAssignment> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new PersonAssignmentRepository(currentUnitOfWork);
		}
	}
}

