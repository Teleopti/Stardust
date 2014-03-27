using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using ConstraintViolationException = Teleopti.Ccc.Infrastructure.Foundation.ConstraintViolationException;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	/// Also includes tests for LazyLoadingManager
	/// Easier that way, because db is involved in those tests
	[TestFixture]
	[Category("LongRunning")]
	public class PersonRepositoryTest : RepositoryTest<IPerson>
	{
		private PersonRepository target;
		private IAbsence absence;
		private IWorkflowControlSet _workflowControlSet;
	    private MockRepository _mockRepository;
	    private IPersonAccountUpdater _personAccountUpdater;

		protected override void ConcreteSetup()
		{
		    _mockRepository = new MockRepository();
		    _personAccountUpdater = _mockRepository.StrictMock<IPersonAccountUpdater>();

			target = new PersonRepository(UnitOfWork);

			absence = AbsenceFactory.CreateAbsence("for test");
			PersistAndRemoveFromUnitOfWork(absence);

			_workflowControlSet = new WorkflowControlSet("A WCS");
			PersistAndRemoveFromUnitOfWork(_workflowControlSet);
		}

	    protected override IPerson CreateAggregateWithCorrectBusinessUnit()
		{
			IPerson person = PersonFactory.CreatePerson("sdgf");
			person.Name = new Name("Roger", "Msdfr");
	        person.Email = "roger.kratz@teleopti.com";
			person.WorkflowControlSet = _workflowControlSet;
			return person;
		}

		protected override void VerifyAggregateGraphProperties(IPerson loadedAggregateFromDatabase)
		{
			IPerson person = CreateAggregateWithCorrectBusinessUnit();
		    Assert.AreEqual(person.Name, loadedAggregateFromDatabase.Name);
			Assert.AreEqual(person.Email, loadedAggregateFromDatabase.Email);
			Assert.AreEqual(0, loadedAggregateFromDatabase.PermissionInformation.ApplicationRoleCollection.Count);
			Assert.That(loadedAggregateFromDatabase.ApplicationAuthenticationInfo, Is.Null);
			Assert.AreEqual(0, loadedAggregateFromDatabase.PersonPeriodCollection.Count());

			Assert.AreEqual(person.WorkflowControlSet, loadedAggregateFromDatabase.WorkflowControlSet);           
		}

		/// <summary>
		/// Determines whether this instance can be created.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void CanCreate()
		{
			new PersonRepository(UnitOfWorkFactory.Current).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldIncludeSuperAdminThatIsAgentInAnotherBu()
		{
			var builtInRole = new ApplicationRole {BuiltIn = true, Name = "dummy role simulating super user"};
			var site = SiteFactory.CreateSimpleSite("for test");
			var team = TeamFactory.CreateSimpleTeam("for test");
			site.AddTeam(team);

			var fakeBu = new BusinessUnit("fake BU");

			var p = PersonFactory.CreatePerson();
			var c = new Contract("sdf");
			var pt = new PartTimePercentage("sdf");
			var cc = new ContractSchedule("sdf");
			var csw = new ContractScheduleWeek();
			cc.AddContractScheduleWeek(csw);
			p.AddPersonPeriod(new PersonPeriod(new DateOnly(1801, 1, 1), new PersonContract(c, pt, cc), team));

			p.PermissionInformation.AddApplicationRole(builtInRole);

			PersistAndRemoveFromUnitOfWork(fakeBu);
			PersistAndRemoveFromUnitOfWork(builtInRole);
			PersistAndRemoveFromUnitOfWork(cc);
			PersistAndRemoveFromUnitOfWork(pt);
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(c);
			PersistAndRemoveFromUnitOfWork(p);

			using (FakeLogon.ToBusinessUnit(fakeBu))
			{
				target.FindAllSortByName(true).Should().Contain(p);
				target.FindAllSortByName().Should().Not.Contain(p);				
			}
		}

		[Test]
		public void Bug9958()
		{
			var site = SiteFactory.CreateSimpleSite("for test");
			var team = TeamFactory.CreateSimpleTeam("for test");
			site.AddTeam(team);
		   
			var p = PersonFactory.CreatePerson();
			var c = new Contract("sdf");
			var pt = new PartTimePercentage("sdf");
			var cc = new ContractSchedule("sdf");
			var csw = new ContractScheduleWeek();
			csw.Add(DayOfWeek.Monday, true);
			csw.Add(DayOfWeek.Tuesday, true);
			csw.Add(DayOfWeek.Wednesday, true);
			csw.Add(DayOfWeek.Thursday, true);
			var csw2 = new ContractScheduleWeek();
			csw2.Add(DayOfWeek.Monday, true);
			csw2.Add(DayOfWeek.Tuesday, true);
			csw2.Add(DayOfWeek.Wednesday, true);
			csw2.Add(DayOfWeek.Thursday, true);
			cc.AddContractScheduleWeek(csw);
			cc.AddContractScheduleWeek(csw2);
			p.AddPersonPeriod(new PersonPeriod(new DateOnly(1801,1,1), new PersonContract(c, pt ,cc), team));
			p.AddPersonPeriod(new PersonPeriod(new DateOnly(1802,1,1), new PersonContract(c, pt ,cc), team));

			PersistAndRemoveFromUnitOfWork(cc);
			PersistAndRemoveFromUnitOfWork(pt);
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(c);
			PersistAndRemoveFromUnitOfWork(p);

			var snubbar = target.FindPeopleInOrganization(new DateOnlyPeriod(1800, 1, 1, 2100, 1, 1), true);

			Assert.AreEqual(2, snubbar.First().PersonPeriodCollection.First().PersonContract.ContractSchedule.ContractScheduleWeeks.Count());
		}

		[Test]
		public void VerifyWriteProtectionInfo()
		{
		    IPerson person = PersonFactory.CreatePerson("for", "test");
			person.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2000,1,1);
			PersistAndRemoveFromUnitOfWork(person);

			IPerson loaded = Session.Get<Person>(person.Id);
			Assert.AreEqual(person, loaded);
			Assert.AreEqual(new DateOnly(2000,1,1), loaded.PersonWriteProtection.PersonWriteProtectedDate);
			Assert.AreEqual(((IUnsafePerson)TeleoptiPrincipal.Current).Person, loaded.PersonWriteProtection.UpdatedBy);
			Assert.IsNotNull(loaded.PersonWriteProtection.UpdatedOn);
			Assert.AreSame(loaded, loaded.PersonWriteProtection.BelongsTo);
		    var version = ((IVersioned) loaded).Version;
            loaded.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2010,1,1);
            PersistAndRemoveFromUnitOfWork(loaded);
		    loaded = Session.Get<Person>(person.Id);
		    Assert.That(loaded.PersonWriteProtection.PersonWriteProtectedDate, Is.EqualTo(new DateOnly(2010, 1, 1)));
            Assert.That(version, Is.EqualTo(((IVersioned)loaded).Version));
            
		}

        
		[Test]
		public void VerifyWriteProtectionInfoWhenAllColumnsIsNull()
		{
			IPerson person = PersonFactory.CreatePerson("for", "test");
			PersistAndRemoveFromUnitOfWork(person);

			IPerson loaded = Session.Get<Person>(person.Id);
			Assert.AreEqual(person, loaded);
			Assert.IsNotNull(loaded.PersonWriteProtection);
			Assert.IsNull(loaded.PersonWriteProtection.PersonWriteProtectedDate);

		}

		[Test]
		public void VerifyCorrectWindowsUser()
		{
			var person = PersonFactory.CreatePerson("test person");
            person.WindowsAuthenticationInfo = new WindowsAuthenticationInfo { DomainName = "domain", WindowsLogOnName = "username" };
			PersistAndRemoveFromUnitOfWork(person);

			target.DoesWindowsUserExists("domain", "username").Should().Be.True();
			target.DoesWindowsUserExists("domain1", "username").Should().Be.False();
			target.DoesWindowsUserExists("domain", "usrname").Should().Be.False();
		}

		[Test]
		public void DeadPersonShouldNotBeFound()
		{
			var person = PersonFactory.CreatePerson("test person");
		    person.WindowsAuthenticationInfo = new WindowsAuthenticationInfo
		                                           {DomainName = "domain", WindowsLogOnName = "username"};

            person.TerminatePerson(DateOnly.Today.AddDays(-2), _personAccountUpdater);
			PersistAndRemoveFromUnitOfWork(person);

			target.DoesWindowsUserExists("domain", "username").Should().Be.False();
		}

		[Test]
		public void VerifyNoPeopleWithIncorrectBusinessUnit()
		{
			MockRepository newMock = new MockRepository();
			IState newStateMock = newMock.StrictMock<IState>();
			IBusinessUnit buTemp = BusinessUnitFactory.CreateSimpleBusinessUnit("dummy");
			PersistAndRemoveFromUnitOfWork(buTemp);

			PartTimePercentage percentage = new PartTimePercentage("sdf");
			ContractSchedule ctrSched = new ContractSchedule("sdfsdf");
			IPerson per = PersonFactory.CreatePerson("vjioo");
			IContract ctr = new Contract("contract");
			ITeam team = TeamFactory.CreateSimpleTeam();
			ISite site = SiteFactory.CreateSimpleSite("sdf");
			team.Site = site;
			team.Description = new Description("sdf");


			PersistAndRemoveFromUnitOfWork(ctrSched);
			PersistAndRemoveFromUnitOfWork(percentage);
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(ctr);

			StateHolderProxyHelper.ClearAndSetStateHolder(newMock, LoggedOnPerson, buTemp, SetupFixtureForAssembly.ApplicationData,
													 newStateMock);
			ITeam teamBu = TeamFactory.CreateSimpleTeam();
			ISite siteBu = SiteFactory.CreateSimpleSite("sdf");
			teamBu.Site = siteBu;
			teamBu.Description = new Description("sdf");
			PersistAndRemoveFromUnitOfWork(siteBu);
			PersistAndRemoveFromUnitOfWork(teamBu);

			per.AddPersonPeriod(new PersonPeriod(new DateOnly(1900,1,1), new PersonContract(ctr, percentage, ctrSched), teamBu));
			per.AddPersonPeriod(new PersonPeriod(new DateOnly(1999,1,1), new PersonContract(ctr, percentage, ctrSched), team));
			PersistAndRemoveFromUnitOfWork(per);
			Assert.AreEqual(0, new PersonRepository(UnitOfWork).FindPeopleInOrganization(new DateOnlyPeriod(2000, 1, 1, 2001, 1, 1), false).Count());
		}

		[Test]
		public void VerifyNoPeopleWithIncorrectBusinessUnitForLoadAllPeopleWithHierarchyDataSortByName()
		{
			MockRepository newMock = new MockRepository();
			IState newStateMock = newMock.StrictMock<IState>();
			IBusinessUnit buTemp = BusinessUnitFactory.CreateSimpleBusinessUnit("dummy");
			PersistAndRemoveFromUnitOfWork(buTemp);

			PartTimePercentage percentage = new PartTimePercentage("sdf");
			ContractSchedule ctrSched = new ContractSchedule("sdfsdf");
			IPerson per = PersonFactory.CreatePerson("vjioo");
			IContract ctr = new Contract("contract");
			ITeam team = TeamFactory.CreateSimpleTeam();
			ISite site = SiteFactory.CreateSimpleSite("sdf");
			team.Site = site;
			team.Description = new Description("sdf");


			PersistAndRemoveFromUnitOfWork(ctrSched);
			PersistAndRemoveFromUnitOfWork(percentage);
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(ctr);

			StateHolderProxyHelper.ClearAndSetStateHolder(newMock, LoggedOnPerson, buTemp, SetupFixtureForAssembly.ApplicationData,
													 newStateMock);
			ITeam teamBu = TeamFactory.CreateSimpleTeam();
			ISite siteBu = SiteFactory.CreateSimpleSite("sdf");
			teamBu.Site = siteBu;
			teamBu.Description = new Description("sdf");
			PersistAndRemoveFromUnitOfWork(siteBu);
			PersistAndRemoveFromUnitOfWork(teamBu);

			per.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(ctr, percentage, ctrSched), team));
			PersistAndRemoveFromUnitOfWork(per);
            Assert.AreEqual(0, new PersonRepository(UnitOfWork).LoadAllPeopleWithHierarchyDataSortByName(new DateOnly(1800, 1, 1)).Count());
		}

		/// <summary>
		/// Verifies the person period part of aggregate.
		/// </summary>
		/// <remarks>
		/// Created by: sumeda herath
		/// Created date: 2008-01-14
		/// </remarks>
		[Test]
		public void VerifyPersonPeriodPartOfAggregate()
		{
			#region  setup Person

			IPerson personToTest = PersonFactory.CreatePerson("dummyAgent1");
  
			// CreateProjection Team belong to a  site 
			ITeam team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);

			//  CreateProjection Activity with GroupActivity

			IActivity activity = new Activity("dummy activity");
			PersistAndRemoveFromUnitOfWork(activity);
					
			// CreateProjection Skill Type
			ISkill skill = SkillFactory.CreateSkill("dummy skill");
			skill.Activity = activity;
			var skill2 = skill.NoneEntityClone();
			PersistAndRemoveFromUnitOfWork(skill.SkillType);
			PersistAndRemoveFromUnitOfWork(skill);
			PersistAndRemoveFromUnitOfWork(skill2);

			IRuleSetBag rsBag = new RuleSetBag();
			rsBag.Description = new Description("for test");
			PersistAndRemoveFromUnitOfWork(rsBag);

			// create Person Contract for testing person
			IPersonContract personContract = PersonContractFactory.CreatePersonContract();

			IPersonPeriod personPeriod = new PersonPeriod(new DateOnly(2000, 1, 1),
												personContract,
												team);
			personToTest.AddPersonPeriod(personPeriod);
			personToTest.AddSkill(new PersonSkill(skill, new Percent(0.44)),personPeriod);
			personToTest.AddSkill(new PersonSkill(skill2, new Percent(0.54)),personPeriod);
			personPeriod.RuleSetBag = rsBag;
			
			personPeriod.BudgetGroup = new BudgetGroup{Name = "BG", TimeZone = personToTest.PermissionInformation.DefaultTimeZone()};
	   
			PersistAndRemoveFromUnitOfWork(personContract.Contract);
			PersistAndRemoveFromUnitOfWork(personContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod.BudgetGroup);

			#endregion

			// Persist the Person
			PersistAndRemoveFromUnitOfWork(personToTest);

			//load person
			IPerson personLoaded = new PersonRepository(UnitOfWork).Load(personToTest.Id.Value);
			var personPeriodLoaded = personLoaded.PersonPeriodCollection.Single();
			//asserts
			Assert.AreEqual(personPeriod.StartDate, personPeriodLoaded.StartDate);
			Assert.AreEqual(2, personPeriodLoaded.PersonSkillCollection.Count());
			Assert.AreEqual("for test", personPeriodLoaded.RuleSetBag.Description.Name);
			Assert.AreEqual(personPeriod.BudgetGroup, personPeriodLoaded.BudgetGroup);
		}

		[Test]
		public void ShouldThrowIfTwoPeriodsWithSameSkill()
		{
			var activity = new Activity("dummy activity");
			PersistAndRemoveFromUnitOfWork(activity);

			var skill = SkillFactory.CreateSkill("dummy skill");
			skill.Activity = activity;
			PersistAndRemoveFromUnitOfWork(skill.SkillType);
			PersistAndRemoveFromUnitOfWork(skill);

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2000, 1, 1), personContract, team);
			personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(0.44)));
			Assert.Throws<ArgumentException>(() => personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(0.54))));
		}

		[Test]
		public void VerifyNoHitOnBasicAuthenticationIfTerminalDate()
		{
			var okPass = Guid.NewGuid().ToString();
			var okLogon = Guid.NewGuid().ToString();
			var person = PersonFactory.CreatePersonWithBasicPermissionInfo(okLogon, okPass);
			person.TerminatePerson(new DateOnly(1800,1,1), _personAccountUpdater);
			PersistAndRemoveFromUnitOfWork(person);
			Assert.IsNull(target.TryFindBasicAuthenticatedPerson(okLogon));
		}

        [Test]
        public void ShouldLoadPermissionData()
        {
            string okDomain = "okDomain";
            string okLogon = "ok";
            IPerson userRetOk;
            IPerson userOk = PersonFactory.CreatePersonWithWindowsPermissionInfo(okLogon, okDomain);
            userOk.PermissionInformation.AddApplicationRole(createAndPersistApplicationRole());

            // CreateProjection Team belong to a  site 
			ITeam team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);

			//  CreateProjection Activity with GroupActivity

			IActivity activity = new Activity("dummy activity");
			PersistAndRemoveFromUnitOfWork(activity);
					
			// CreateProjection Skill Type
			ISkill skill = SkillFactory.CreateSkill("dummy skill");
			skill.Activity = activity;
        	var skill2 = skill.NoneEntityClone();
			PersistAndRemoveFromUnitOfWork(skill.SkillType);
			PersistAndRemoveFromUnitOfWork(skill);
			PersistAndRemoveFromUnitOfWork(skill2);

			IRuleSetBag rsBag = new RuleSetBag();
			rsBag.Description = new Description("for test");
			PersistAndRemoveFromUnitOfWork(rsBag);

			// create Person Contract for testing person
			IPersonContract personContract = PersonContractFactory.CreatePersonContract();

			IPersonPeriod personPeriod = new PersonPeriod(new DateOnly(2000, 1, 1),
												personContract,
												team);
			userOk.AddPersonPeriod(personPeriod);
			userOk.AddSkill(new PersonSkill(skill, new Percent(0.44)),personPeriod);
			userOk.AddSkill(new PersonSkill(skill2, new Percent(0.54)),personPeriod);
			personPeriod.RuleSetBag = rsBag;
            
            personPeriod.BudgetGroup = new BudgetGroup() { Name = "BG", TimeZone = userOk.PermissionInformation.DefaultTimeZone() };
	   
			PersistAndRemoveFromUnitOfWork(personContract.Contract);
			PersistAndRemoveFromUnitOfWork(personContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod.BudgetGroup);

			// Persist the Person
            PersistAndRemoveFromUnitOfWork(userOk);
            
            Assert.IsTrue(target.TryFindWindowsAuthenticatedPerson(okDomain, okLogon, out userRetOk));
            Session.Close();
            Assert.AreEqual(userOk, userRetOk);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(userRetOk.PermissionInformation));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(userRetOk.PermissionInformation.ApplicationRoleCollection));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(userRetOk.PermissionInformation.ApplicationRoleCollection[0].ApplicationFunctionCollection));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(userRetOk.PermissionInformation.ApplicationRoleCollection[0].BusinessUnit));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(userRetOk.PersonPeriodCollection));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(userRetOk.PersonPeriodCollection[0].Team));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(userRetOk.PersonPeriodCollection[0].Team.Site));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(userRetOk.PersonPeriodCollection[0].Team.BusinessUnitExplicit));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(userRetOk.PersonPeriodCollection[0].Team.Site.TeamCollection));

            Assert.AreEqual(1, userRetOk.PermissionInformation.ApplicationRoleCollection.Count);
            Assert.AreEqual(2, userRetOk.PermissionInformation.ApplicationRoleCollection[0].ApplicationFunctionCollection.Count);
            Assert.AreEqual(1, userRetOk.PersonPeriodCollection[0].Team.Site.TeamCollection.Count);
            Assert.AreEqual(1, userRetOk.PersonPeriodCollection.Count);
        }

		[Test]
		public void VerifyNoHitOnWindowsAuthenticationIfTerminalDate()
		{
			IPerson retPer;
			var okDomain = Guid.NewGuid().ToString();
			var okLogon = Guid.NewGuid().ToString();
			var person = PersonFactory.CreatePersonWithWindowsPermissionInfo(okLogon, okDomain);
			person.TerminatePerson(new DateOnly(1800, 1, 1), _personAccountUpdater);
			PersistAndRemoveFromUnitOfWork(person);
			Assert.IsFalse(target.TryFindWindowsAuthenticatedPerson(okDomain, okLogon, out retPer));
		}

		/// <summary>
		/// Verifies that Find based on logonname and password works when win auth is used.
		/// </summary>
		[Test]
		public void VerifyFindLogOnNameAndPasswordWorksUsingWindowsAuthentication()
		{
			string okDomain = "okDomain";
			string okLogon = "ok";
			string falseDomain = "hejhej";
			string falseLogon = "sdfklj";
			IPerson userRetOk, userRetNo;
			IPerson userOk = PersonFactory.CreatePersonWithWindowsPermissionInfo(okLogon, okDomain);
			userOk.PermissionInformation.AddApplicationRole(createAndPersistApplicationRole());
			createSiteAndTeam();
			PersistAndRemoveFromUnitOfWork(userOk);

			Assert.IsFalse(target.TryFindWindowsAuthenticatedPerson(falseDomain, falseLogon, out userRetNo));
			Assert.IsNull(userRetNo);
			Assert.IsFalse(target.TryFindWindowsAuthenticatedPerson(falseDomain, okLogon, out userRetNo));
			Assert.IsNull(userRetNo);
			Assert.IsFalse(target.TryFindWindowsAuthenticatedPerson(okDomain, falseLogon, out userRetNo));
			Assert.IsNull(userRetNo);

			Assert.IsTrue(target.TryFindWindowsAuthenticatedPerson(okDomain, okLogon, out userRetOk));
			Assert.AreEqual(userOk, userRetOk);

			IList<IBusinessUnit> buAccess = userRetOk.PermissionInformation.BusinessUnitAccessCollection();
			Assert.AreEqual(1, userRetOk.PermissionInformation.ApplicationRoleCollection.Count);
			Assert.AreEqual(2, userRetOk.PermissionInformation.ApplicationRoleCollection[0].ApplicationFunctionCollection.Count);
			Assert.AreEqual(1, buAccess.Count);
			Assert.AreEqual(2, buAccess[0].TeamCollection().Count);
			Assert.AreEqual(2, buAccess[0].SiteCollection.Count);
			verifyPermissionInfoIsLazy(true, userRetOk);
 
		}

		[Test]
		public void VerifyFindWhoBelongToTeam()
		{
			//setup
			ISite site = SiteFactory.CreateSimpleSite("for test");
		  
			ITeam team = TeamFactory.CreateSimpleTeam("for test");
			site.AddTeam(team);
		   
			IPerson per1 = PersonFactory.CreatePerson("roger", "kratz");
			IPersonPeriod personPeriod1 =
				PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), team);
			per1.AddPersonPeriod(personPeriod1);
			IPersonPeriod personPeriod2 =
				PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2001, 1, 1), team);
			per1.AddPersonPeriod(personPeriod2);

			IPerson per2 = PersonFactory.CreatePerson("tamas", "balog");

			IExternalLogOn login1 = ExternalLogOnFactory.CreateExternalLogOn();
			per1.AddExternalLogOn(login1, personPeriod1);
			per1.AddExternalLogOn(login1, personPeriod2);

			//Persist
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);

			PersistAndRemoveFromUnitOfWork(login1);

			PersistAndRemoveFromUnitOfWork(personPeriod1.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod1.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod1.PersonContract.Contract);
			//PersistAndRemoveFromUnitOfWork(personPeriod1.PersonContract);

			PersistAndRemoveFromUnitOfWork(personPeriod2.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod2.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod2.PersonContract.Contract);
			//PersistAndRemoveFromUnitOfWork(personPeriod2.PersonContract);

			PersistAndRemoveFromUnitOfWork(per1);
			PersistAndRemoveFromUnitOfWork(per2);

			//load
            IList<IPerson> testList = new List<IPerson>(new PersonRepository(UnitOfWork).LoadAllPeopleWithHierarchyDataSortByName(new DateOnly(1800, 1, 1)));
			//verify
			testList.Remove(LoggedOnPerson);
			Assert.AreEqual(2, testList.Count);
			Assert.AreEqual(testList[0], per2);
			Assert.AreEqual(testList[1], per1);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(testList[1].PersonPeriodCollection));
			Assert.IsTrue(LazyLoadingManager.IsInitialized(testList[1].PersonPeriodCollection.First().Team));
			Assert.AreEqual(0, testList[0].PersonPeriodCollection.Count());
			Assert.AreEqual(2, testList[1].PersonPeriodCollection.Count());
			IList<IPersonPeriod> wrappedPersonPeriod = new List<IPersonPeriod>(testList[1].PersonPeriodCollection);
			Assert.AreEqual(1, wrappedPersonPeriod[0].ExternalLogOnCollection.Count);
			Assert.AreEqual(1, wrappedPersonPeriod[1].ExternalLogOnCollection.Count);
		}

		[Test]
		public void VerifyLoadPermissionData()
		{
			//setup
			ISite site = SiteFactory.CreateSimpleSite("for test");
			ITeam team = TeamFactory.CreateSimpleTeam("for test");
			site.AddTeam(team);
		   
			//Persist
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);

			IApplicationFunction func1 = new ApplicationFunction("APP1");
			PersistAndRemoveFromUnitOfWork(func1);
			IApplicationFunction func2 = new ApplicationFunction("APP2");
			PersistAndRemoveFromUnitOfWork(func2);
			IApplicationRole role1 = ApplicationRoleFactory.CreateRole("ROLE1", "ROLEDESC1");
			role1.AddApplicationFunction(func1);
			role1.AddApplicationFunction(func2);
			PersistAndRemoveFromUnitOfWork(role1);
			IPerson per = PersonFactory.CreatePerson("Test", "Testorson");
			IPersonPeriod personPeriod =
				PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), team);
			per.AddPersonPeriod(personPeriod);
			per.PermissionInformation.AddApplicationRole(role1);

			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(per);
			//load
			IPersonRepository repository = new PersonRepository(UnitOfWork);
            IList<IPerson> personList = new List<IPerson>(repository.LoadAllPeopleWithHierarchyDataSortByName(new DateOnly(1800, 1, 1)));
			//verify
			personList.Remove(LoggedOnPerson);
			Assert.AreEqual(1, personList.Count);
			IPerson retPerson = personList[0];

			verifyPermissionInfoIsLazy(false, retPerson);
		}

        [Test]
        public void PersonWithTerminalDateBeforeSpecificDateShouldNotBeLoadedByLoadAllPeopleWithHierarchyDataSortByName()
        {
            //setup
            ISite site = SiteFactory.CreateSimpleSite("for test");
            ITeam team = TeamFactory.CreateSimpleTeam("for test");
            site.AddTeam(team);

            //Persist
            PersistAndRemoveFromUnitOfWork(site);
            PersistAndRemoveFromUnitOfWork(team);

						IApplicationFunction func1 = new ApplicationFunction("APP1");
            PersistAndRemoveFromUnitOfWork(func1);
            IApplicationRole role1 = ApplicationRoleFactory.CreateRole("ROLE1", "ROLEDESC1");
            role1.AddApplicationFunction(func1);
            PersistAndRemoveFromUnitOfWork(role1);

            IPerson per = PersonFactory.CreatePerson("Test", "Testorson");
            per.TerminatePerson(DateOnly.Today.AddDays(-1), _personAccountUpdater);
            IPersonPeriod personPeriod =
                PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), team);
            per.AddPersonPeriod(personPeriod);

            PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.ContractSchedule);
            PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.PartTimePercentage);
            PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.Contract);
            PersistAndRemoveFromUnitOfWork(per);

            //load
            IPersonRepository repository = new PersonRepository(UnitOfWork);
            IList<IPerson> personList = new List<IPerson>(repository.LoadAllPeopleWithHierarchyDataSortByName(DateOnly.Today));
            //verify
            personList.Remove(LoggedOnPerson);
            Assert.AreEqual(0, personList.Count);
            //IPerson retPerson = personList[0];
        }

		[Test]
		public void VerifyLoadPermissionDataWithoutReassociate()
		{
			//setup
			IApplicationFunction func1 = new ApplicationFunction("APP1");
			PersistAndRemoveFromUnitOfWork(func1);
			IApplicationFunction func2 = new ApplicationFunction("APP2");
			PersistAndRemoveFromUnitOfWork(func2);
			IApplicationRole role1 = ApplicationRoleFactory.CreateRole("ROLE1", "ROLEDESC1");
			role1.AddApplicationFunction(func1);
			role1.AddApplicationFunction(func2);
			PersistAndRemoveFromUnitOfWork(role1);
			IPerson per = PersonFactory.CreatePerson("Test", "Testorson");
			per.PermissionInformation.AddApplicationRole(role1);
			PersistAndRemoveFromUnitOfWork(per);
			//load
			IPersonRepository repository = new PersonRepository(UnitOfWork);
            IList<IPerson> personList = new List<IPerson>(repository.LoadAllPeopleWithHierarchyDataSortByName(new DateOnly(1800, 1, 1)));
			//verify
			personList.Remove(LoggedOnPerson);
			Assert.AreEqual(1, personList.Count);
			IPerson retPerson = personList[0];

			verifyPermissionInfoIsLazy(false, retPerson);

			retPerson = repository.LoadPermissionDataWithoutReassociate(retPerson);

			verifyPermissionInfoIsLazy(true, retPerson);
		}

		/// <summary>
		/// Can create a user repository without being logged on.
		/// </summary>
		[Test]
		public void VerifyCanCreateUserRepositoryWithoutBeingLoggedOn()
		{
			//No calls on this mock is allowed
			MockRepository mocks = new MockRepository();
			IState stateMockTemp = mocks.StrictMock<IState>();
			StateHolderProxyHelper.ClearAndInitializeStateHolder(stateMockTemp);
			Expect.On(stateMockTemp)
				.Call(stateMockTemp.IsLoggedIn)
				.Return(false)
				.Repeat.Any();
			mocks.ReplayAll();
			justForTest justForTest1 = new justForTest(UnitOfWork);
			Assert.IsNotNull(justForTest1.InternalSession);
			mocks.VerifyAll();
		}
	   
		[Test]
		public void VerifyFindPersonInOrganizationWithContract()
		{
			SetupPersonsInOrganizationWithContract();

            Session.Evict(BusinessUnitFactory.BusinessUnitUsedInTest);
			ICollection<IPerson> res = target.FindPeopleInOrganization(new DateOnlyPeriod(2000, 1, 1, 2001, 1, 1), false);

			Assert.AreEqual(2, res.Count);

			IPerson person = res.FirstOrDefault(p => p.Name.FirstName == "hejhej");
			IPerson person2 = res.FirstOrDefault(p => p.Name.FirstName == "hejhej2");

		    Assert.IsTrue(LazyLoadingManager.IsInitialized(person.PersonPeriodCollection));
			
            var personPeriod = person.PersonPeriodCollection.First();
			Assert.IsTrue(LazyLoadingManager.IsInitialized(personPeriod.PersonContract));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(personPeriod.PersonContract.Contract));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(personPeriod.Team));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(personPeriod.Team.Site));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(personPeriod.Team.Site.BusinessUnit));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(personPeriod.PersonSkillCollection));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(personPeriod.ExternalLogOnCollection));
			Assert.IsTrue(LazyLoadingManager.IsInitialized(person.PersonSchedulePeriodCollection));
			Assert.IsTrue(LazyLoadingManager.IsInitialized(person.PersonSchedulePeriodCollection.First().ShiftCategoryLimitationCollection()));
 
			Assert.IsTrue(LazyLoadingManager.IsInitialized(person.PersonSchedulePeriodCollection));
			Assert.AreEqual(2, person.PersonSchedulePeriodCollection.Count);
			Assert.AreEqual(0, person2.PersonSchedulePeriodCollection.Count);
			Assert.AreEqual(3, person.PersonPeriodCollection.Count());
			Assert.AreEqual(1, person2.PersonPeriodCollection.Count());
		}

		[Test]
		public void VerifyFindPersonInOrganizationWithWorkflowControlSet()
		{
			SetupPersonsInOrganizationWithContract();
			ICollection<IPerson> res = target.FindPeopleInOrganization(new DateOnlyPeriod(2000, 1, 1, 2001, 1, 1), false);
			IPerson person = res.FirstOrDefault(p => p.Name.FirstName == "hejhej");
			Assert.IsTrue(LazyLoadingManager.IsInitialized(person.WorkflowControlSet));
		}

		[Test]
		public void ShouldLoadPeopleTeamSiteSchedulePeriodWorkflowControlSet()
		{
			SetupPersonsInOrganizationWithContract();
			var res = target.FindPeopleTeamSiteSchedulePeriodWorkflowControlSet(new DateOnlyPeriod(2000, 1, 1, 2001, 1, 1));

			var person = res.FirstOrDefault(p => p.Name.FirstName == "hejhej");
			var personPeriod = person.PersonPeriodCollection.First();
			Assert.IsTrue(LazyLoadingManager.IsInitialized(person.PersonPeriodCollection));
			Assert.IsTrue(LazyLoadingManager.IsInitialized(personPeriod.Team));
			Assert.IsTrue(LazyLoadingManager.IsInitialized(personPeriod.Team.Site));
			Assert.IsTrue(LazyLoadingManager.IsInitialized(person.PersonSchedulePeriodCollection));
			Assert.IsTrue(LazyLoadingManager.IsInitialized(person.WorkflowControlSet));
			Assert.IsTrue(LazyLoadingManager.IsInitialized(personPeriod.PersonSkillCollection));
		}

		[Test]
		public void VerifyPeopleSkillMatrixHandlesScenario()
		{
			IScenario scenOk = ScenarioFactory.CreateScenarioAggregate();
			IScenario scenNo = ScenarioFactory.CreateScenarioAggregate();
			PersistAndRemoveFromUnitOfWork(scenOk);
			PersistAndRemoveFromUnitOfWork(scenNo);

			IActivity act = new Activity("sdf");
			PersistAndRemoveFromUnitOfWork(act);
			ISkillType skType = SkillTypeFactory.CreateSkillType();
			PersistAndRemoveFromUnitOfWork(skType);
			ISkill skillWithValidDays = SkillFactory.CreateSkill("skillWithValidDays", skType, 10);
			skillWithValidDays.Activity=act;
			skillWithValidDays.TimeZone = (TimeZoneInfo.Local);
			PersistAndRemoveFromUnitOfWork(skillWithValidDays);
			ISkillDay skillDayValid = SkillDayFactory.CreateSkillDay(skillWithValidDays, new DateTime(2000, 1, 6, 0, 0, 0, DateTimeKind.Utc), scenOk);
			PersistAndRemoveFromUnitOfWork(skillWithValidDays.WorkloadCollection);
			PersistAndRemoveFromUnitOfWork(skillDayValid);

			ISite site = SiteFactory.CreateSimpleSite("d"); 
			PersistAndRemoveFromUnitOfWork(site);
			ITeam team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.Description = new Description("sdf");
			PersistAndRemoveFromUnitOfWork(team);

			IPerson validPerson = PersonFactory.CreatePerson("valid");
			validPerson.AddPersonPeriod(new PersonPeriod(new DateOnly(2000, 2, 3), createPersonContract(), team));
			validPerson.AddSkill(new PersonSkill(skillWithValidDays, new Percent(1)), validPerson.PersonPeriodCollection.First());
			PersistAndRemoveFromUnitOfWork(validPerson);

			Assert.AreEqual(1, target.PeopleSkillMatrix(scenOk, new DateTimePeriod(2000, 1, 1, 2001, 1, 1)).Count());
			Assert.AreEqual(0, target.PeopleSkillMatrix(scenNo, new DateTimePeriod(2000, 1, 1, 2001, 1, 1)).Count());
		}

		[Test]
		public void VerifyPeopleSkillMatrix()
		{
			IScenario scen = ScenarioFactory.CreateScenarioAggregate();
			PersistAndRemoveFromUnitOfWork(scen);
			IActivity act = new Activity("sdf");
			PersistAndRemoveFromUnitOfWork(act);
			ISkillType skType = SkillTypeFactory.CreateSkillType();
			PersistAndRemoveFromUnitOfWork(skType);
			ISkill skillWithValidDays = new Skill("skillWithValidDays", "sdf", Color.Red, 10, skType) { Activity = act, TimeZone = (TimeZoneInfo.Local) };
			ISkill skillWithNonValidDays = new Skill("skillWithNonValidDays", "sdf", Color.Red, 10, skType) { Activity = act, TimeZone = (TimeZoneInfo.Local) };
			ISkill skillWithNoDays = new Skill("skillWithNonValidDays", "sdf", Color.Red, 10, skType) { Activity = act, TimeZone = (TimeZoneInfo.Local) };
			ISkill deletedSkill = new Skill("deletedSkill", "sdf", Color.Red, 10, skType) { Activity = act, TimeZone = (TimeZoneInfo.Local) };
			((IDeleteTag)deletedSkill).SetDeleted();

			PersistAndRemoveFromUnitOfWork(skillWithNoDays);
			PersistAndRemoveFromUnitOfWork(skillWithValidDays);
			PersistAndRemoveFromUnitOfWork(skillWithNonValidDays);
			PersistAndRemoveFromUnitOfWork(deletedSkill);

			ISkillDay skillDayValid = SkillDayFactory.CreateSkillDay(skillWithValidDays, new DateTime(2000, 1, 6, 0, 0, 0, DateTimeKind.Utc), scen);
			ISkillDay skillDayNonValid = SkillDayFactory.CreateSkillDay(skillWithNonValidDays, new DateTime(2002, 1, 6, 0, 0, 0, DateTimeKind.Utc), scen);
			ISkillDay skillDayNonValid2 = SkillDayFactory.CreateSkillDay(deletedSkill, new DateTime(2000, 1, 6, 0, 0, 0, DateTimeKind.Utc), scen);

			PersistAndRemoveFromUnitOfWork(scen);
			PersistAndRemoveFromUnitOfWork(skillWithValidDays.WorkloadCollection);
			PersistAndRemoveFromUnitOfWork(skillWithNonValidDays.WorkloadCollection);
			PersistAndRemoveFromUnitOfWork(skillWithNoDays.WorkloadCollection);
			PersistAndRemoveFromUnitOfWork(deletedSkill.WorkloadCollection);
			PersistAndRemoveFromUnitOfWork(skillDayValid);
			PersistAndRemoveFromUnitOfWork(skillDayNonValid2);
			PersistAndRemoveFromUnitOfWork(skillDayNonValid);

			ISite site = SiteFactory.CreateSimpleSite("d");
			PersistAndRemoveFromUnitOfWork(site);
			ITeam team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.Description = new Description("sdf");
			PersistAndRemoveFromUnitOfWork(team);

			var validPerson = PersonFactory.CreatePerson("valid");
			var validPersonPeriod = new PersonPeriod(new DateOnly(2000, 2, 3), createPersonContract(), team);
			validPerson.AddPersonPeriod(validPersonPeriod);
			validPerson.AddSkill(new PersonSkill(skillWithValidDays, new Percent(1)),validPersonPeriod);
			validPerson.AddSkill(new PersonSkill(skillWithNonValidDays, new Percent(1)),validPersonPeriod);

			var nonValidPerson1 = PersonFactory.CreatePerson("no1");
			var nonValidPersonPeriod1 = new PersonPeriod(new DateOnly(2000, 2, 3), createPersonContract(), team);
			nonValidPerson1.AddPersonPeriod(nonValidPersonPeriod1);
			nonValidPerson1.AddSkill(new PersonSkill(deletedSkill, new Percent(2)),nonValidPersonPeriod1);

			var nonValidPerson2 = PersonFactory.CreatePerson("no2");
			var nonValidPersonPeriod2 = new PersonPeriod(new DateOnly(2006, 2, 3), createPersonContract(), team);
			nonValidPerson2.AddPersonPeriod(nonValidPersonPeriod2);
			nonValidPerson2.AddSkill(new PersonSkill(skillWithValidDays, new Percent(2)),nonValidPersonPeriod2);

			var nonValidPerson3 = PersonFactory.CreatePerson("no3");
			var nonValidPersonPeriod3 = new PersonPeriod(new DateOnly(2000, 2, 3), createPersonContract(), team);
			nonValidPerson3.AddPersonPeriod(nonValidPersonPeriod3);
			nonValidPerson3.AddSkill(new PersonSkill(skillWithNonValidDays, new Percent(2)), nonValidPersonPeriod3);

			PersistAndRemoveFromUnitOfWork(validPerson);
			PersistAndRemoveFromUnitOfWork(nonValidPerson1);
			PersistAndRemoveFromUnitOfWork(nonValidPerson2);
			PersistAndRemoveFromUnitOfWork(nonValidPerson3);

			var res = target.PeopleSkillMatrix(scen, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			
			//1 pers med ett skill
			Assert.AreEqual(1, res.Count());
			foreach (var pair in res)
			{
				Assert.AreEqual(pair.Item1, validPerson.Id.Value);
				Assert.AreEqual(pair.Item2, skillWithValidDays.Id.Value);
			}
		}

		[Test]
		public void VerifyVerifyPeopleSkillMatrixWithMoreThanOnePersonPeriod()
		{
			IScenario scen = ScenarioFactory.CreateScenarioAggregate();
			PersistAndRemoveFromUnitOfWork(scen);
			IActivity act = new Activity("sdf");
			PersistAndRemoveFromUnitOfWork(act);
			ISkillType skType = SkillTypeFactory.CreateSkillType();
			PersistAndRemoveFromUnitOfWork(skType);
			ISkill skillWithValidDays1 = new Skill("skillWithValidDays1", "sdf", Color.Red, 10, skType) { Activity = act, TimeZone = (TimeZoneInfo.Local) };
			ISkill skillWithValidDays2 = new Skill("skillWithValidDays2", "sdf", Color.Red, 10, skType) { Activity = act, TimeZone = (TimeZoneInfo.Local) };


			PersistAndRemoveFromUnitOfWork(skillWithValidDays1);
			PersistAndRemoveFromUnitOfWork(skillWithValidDays2);

			ISkillDay skillDayValid1 = SkillDayFactory.CreateSkillDay(skillWithValidDays1, new DateTime(2000, 1, 6, 0, 0, 0, DateTimeKind.Utc), scen);
			ISkillDay skillDayValid2 = SkillDayFactory.CreateSkillDay(skillWithValidDays2, new DateTime(2000, 1, 2, 0, 0, 0, DateTimeKind.Utc), scen);
			ISkillDay skillDayValid3 = SkillDayFactory.CreateSkillDay(skillWithValidDays2, new DateTime(2000, 1, 6, 0, 0, 0, DateTimeKind.Utc), scen);


			PersistAndRemoveFromUnitOfWork(scen);
			PersistAndRemoveFromUnitOfWork(skillWithValidDays1.WorkloadCollection);
			PersistAndRemoveFromUnitOfWork(skillWithValidDays2.WorkloadCollection);

			PersistAndRemoveFromUnitOfWork(skillDayValid1);
			PersistAndRemoveFromUnitOfWork(skillDayValid2);
			PersistAndRemoveFromUnitOfWork(skillDayValid3);

			ISite site = SiteFactory.CreateSimpleSite("d");
			PersistAndRemoveFromUnitOfWork(site);
			ITeam team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.Description = new Description("sdf");
			PersistAndRemoveFromUnitOfWork(team);

			IPerson validPerson1 = PersonFactory.CreatePerson("valid1");
			IPersonPeriod period = new PersonPeriod(new DateOnly(2000, 1, 1), createPersonContract(), team);
			validPerson1.AddPersonPeriod(period);
			validPerson1.AddSkill(new PersonSkill(skillWithValidDays2, new Percent(1)), period);
			period = new PersonPeriod(new DateOnly(2000, 1, 4), createPersonContract(), team);
			validPerson1.AddPersonPeriod(period); 
			validPerson1.AddSkill(new PersonSkill(skillWithValidDays1, new Percent(1)),period);

			IPerson validPerson2 = PersonFactory.CreatePerson("valid2");
			period = new PersonPeriod(new DateOnly(2000, 1, 4), createPersonContract(), team);
			validPerson2.AddPersonPeriod(period);
			validPerson2.AddSkill(new PersonSkill(skillWithValidDays1, new Percent(1)), period);
			
			IPerson nonValidPerson1 = PersonFactory.CreatePerson("no1");
			period = new PersonPeriod(new DateOnly(2000, 1, 4), createPersonContract(), team);
			nonValidPerson1.AddPersonPeriod(period);
			nonValidPerson1.AddSkill(new PersonSkill(skillWithValidDays2, new Percent(1)),period);
			
			PersistAndRemoveFromUnitOfWork(validPerson1);
			PersistAndRemoveFromUnitOfWork(validPerson2);
			PersistAndRemoveFromUnitOfWork(nonValidPerson1);

			var res = target.PeopleSkillMatrix(scen, new DateTimePeriod(2000, 1, 5, 2001, 1, 7)).ToList();
			
			Assert.AreEqual(3, res.Count);
			var pair = new Tuple<Guid, Guid>(validPerson1.Id.Value, skillWithValidDays1.Id.Value);
			Assert.IsTrue(res.Contains(pair));
			pair = new Tuple<Guid, Guid>(validPerson2.Id.Value, skillWithValidDays1.Id.Value);
			Assert.IsTrue(res.Contains(pair));
			pair = new Tuple<Guid, Guid>(nonValidPerson1.Id.Value, skillWithValidDays2.Id.Value);
			Assert.IsTrue(res.Contains(pair));
			
		}

		[Test]
		public void VerifyPeopleSiteMatrix()
		{
			ISite siteWithMaxSeats = SiteFactory.CreateSimpleSite("d");
			siteWithMaxSeats.MaxSeats = 1;
			PersistAndRemoveFromUnitOfWork(siteWithMaxSeats);
			ISite otherSite = SiteFactory.CreateSimpleSite("o");
			PersistAndRemoveFromUnitOfWork(otherSite);
			ITeam team = TeamFactory.CreateSimpleTeam();
			team.Site = siteWithMaxSeats;
			team.Description = new Description("sdf");
			PersistAndRemoveFromUnitOfWork(team);
			ITeam otherTeam = TeamFactory.CreateSimpleTeam();
			otherTeam.Site = otherSite;
			otherTeam.Description = new Description("sdf");
			PersistAndRemoveFromUnitOfWork(otherTeam);

			IPerson validPerson1 = PersonFactory.CreatePerson("valid1");
			IPersonPeriod period = new PersonPeriod(new DateOnly(2000, 1, 1), createPersonContract(), team);
			validPerson1.AddPersonPeriod(period);
			period = new PersonPeriod(new DateOnly(2000, 1, 4), createPersonContract(), team);
			validPerson1.AddPersonPeriod(period);

			IPerson validPerson2 = PersonFactory.CreatePerson("valid2");
			period = new PersonPeriod(new DateOnly(2000, 1, 4), createPersonContract(), team);
			validPerson2.AddPersonPeriod(period);

			IPerson nonValidPerson1 = PersonFactory.CreatePerson("no1");
			period = new PersonPeriod(new DateOnly(2000, 1, 4), createPersonContract(), otherTeam);
			nonValidPerson1.AddPersonPeriod(period);

			PersistAndRemoveFromUnitOfWork(validPerson1);
			PersistAndRemoveFromUnitOfWork(validPerson2);
			PersistAndRemoveFromUnitOfWork(nonValidPerson1);

			IList<Guid> res = target.PeopleSiteMatrix(new DateTimePeriod(2000, 1, 5, 2001, 1, 7)).ToList();

			Assert.AreEqual(2, res.Count);
			Assert.IsTrue(res.Contains(validPerson1.Id.Value));
			Assert.IsTrue(res.Contains(validPerson2.Id.Value));
			Assert.IsFalse(res.Contains(nonValidPerson1.Id.Value));
			
		}

		private void SetupPersonsInOrganizationWithContract()
		{
			IPerson okPerson = PersonFactory.CreatePerson("hejhej");
			okPerson.WorkflowControlSet = _workflowControlSet;
			okPerson.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Utc));
			IPerson okPerson2 = PersonFactory.CreatePerson("hejhej2");
			IPerson noPerson1 = PersonFactory.CreatePerson("bajbaj");
			IPerson noPerson2 = PersonFactory.CreatePerson("bajbaj");
			IPerson noPerson3 = PersonFactory.CreatePersonWithBasicPermissionInfo("dra på ", "trissor");
			ITeam team = TeamFactory.CreateSimpleTeam("hola");
			ISite site = SiteFactory.CreateSimpleSite();
			site.AddTeam(team);
			IActivity act = new Activity("for test");
			ISkillType skType = SkillTypeFactory.CreateSkillType();
			ISkill skill = new Skill("for test", "sdf", Color.Blue, 3, skType);
			ISkill skill2 = new Skill("for test2", "sdf", Color.Blue, 3, skType);
			skill.Activity = act;
			skill2.Activity = act;
			skill.TimeZone = (TimeZoneInfo.Local);
			skill2.TimeZone = (TimeZoneInfo.Local);

			PersonPeriod okPeriod = new PersonPeriod(new DateOnly(2000, 1, 1), createPersonContract(), team);
			okPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			okPeriod.AddPersonSkill(new PersonSkill(skill2, new Percent(1)));
			PersonPeriod okPeriod2 = new PersonPeriod(new DateOnly(2000, 1, 1), createPersonContract(), team);
			okPeriod2.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			okPeriod2.AddPersonSkill(new PersonSkill(skill2, new Percent(1)));
			PersonPeriod noPeriod2 = new PersonPeriod(new DateOnly(2002, 1, 1), createPersonContract(), team);
			noPeriod2.AddPersonSkill(new PersonSkill(skill, new Percent(1)));

			SchedulePeriod schedulePeriod1 = new SchedulePeriod(okPeriod.StartDate, SchedulePeriodType.Month, 1);
			SchedulePeriod schedulePeriod2 = new SchedulePeriod(new DateOnly(okPeriod.StartDate.Date.AddYears(1)), SchedulePeriodType.Month, 1);
			var sCategory = new ShiftCategory("for test");
			schedulePeriod1.AddShiftCategoryLimitation(new ShiftCategoryLimitation(sCategory));
			schedulePeriod2.AddShiftCategoryLimitation(new ShiftCategoryLimitation(sCategory));

			okPerson.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1), createPersonContract(), team));
			okPerson.AddPersonPeriod(okPeriod);
			okPerson.AddPersonPeriod(new PersonPeriod(new DateOnly(2001, 1, 1), createPersonContract(), team));
			okPerson.AddSchedulePeriod(schedulePeriod1);
			okPerson.AddSchedulePeriod(schedulePeriod2);
			okPerson2.AddPersonPeriod(okPeriod2);

			noPerson2.AddPersonPeriod(noPeriod2);

			PersistAndRemoveFromUnitOfWork(sCategory);
			PersistAndRemoveFromUnitOfWork(act);
			PersistAndRemoveFromUnitOfWork(skType);
			PersistAndRemoveFromUnitOfWork(skill);
			PersistAndRemoveFromUnitOfWork(skill2);
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(noPerson3);
			PersistAndRemoveFromUnitOfWork(noPerson1);
			PersistAndRemoveFromUnitOfWork(noPerson2);
			PersistAndRemoveFromUnitOfWork(okPerson);
			PersistAndRemoveFromUnitOfWork(okPerson2);
		}
		[Test]
		public void VerifyFindPersonInOrganizationLight()
		{
			ITeam team = TeamFactory.CreateSimpleTeam("Team1");
			ITeam team2 = TeamFactory.CreateSimpleTeam("Team2");
			ISite site = SiteFactory.CreateSimpleSite();
			site.AddTeam(team);
			site.AddTeam(team2);
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(team2);

			createPersonWithRuleSets(team);
			createPersonWithRuleSets(team2);

			IPerson loadedPerson = new List<IPerson>(target.FindPeopleInOrganizationLight(new DateOnlyPeriod(2000, 1, 1, 2001, 1, 1)))[0];
			Assert.IsFalse(LazyLoadingManager.IsInitialized(loadedPerson.PersonPeriodCollection.First().RuleSetBag));
			Session.Clear();
			loadedPerson = new List<IPerson>(target.FindPeopleInOrganizationLight(new DateOnlyPeriod(2000, 1, 1, 2001, 1, 1)))[0];
			Assert.AreEqual(2, loadedPerson.PersonPeriodCollection.First().RuleSetBag.RuleSetCollection.Count);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldFindPeople()
        {
            var person = CreateAggregateWithCorrectBusinessUnit();

            IApplicationRole role = ApplicationRoleFactory.CreateRole("test", "role");
            ITeam team = TeamFactory.CreateSimpleTeam("sdf");
            ISite site = SiteFactory.CreateSimpleSite("sdf");
            team.Site = site;
						IContract ctr = new Contract("sdf");
            IPartTimePercentage pTime = new PartTimePercentage("sdf");
            IContractSchedule cSc = ContractScheduleFactory.CreateContractSchedule("sdf");
        	DateOnly date = new DateOnly(2000, 1, 2);

            PersistAndRemoveFromUnitOfWork(site);
            PersistAndRemoveFromUnitOfWork(team);
            PersistAndRemoveFromUnitOfWork(ctr);
            PersistAndRemoveFromUnitOfWork(pTime);
            PersistAndRemoveFromUnitOfWork(cSc);

            person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(date, PersonContractFactory.CreatePersonContract(ctr, pTime, cSc), team));
            foreach (IPersonPeriod personPeriod in person.PersonPeriodCollection)
            {
                PersistAndRemoveFromUnitOfWork(personPeriod.Team.Site);
                PersistAndRemoveFromUnitOfWork(personPeriod.Team);
            }

            PersistAndRemoveFromUnitOfWork(role);

            person.PermissionInformation.AddApplicationRole(role);

            PersistAndRemoveFromUnitOfWork(person);

            IPerson loadedPerson = new List<IPerson>(target.FindPeople(new List<IPerson> {person}))[0];
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loadedPerson.PersonPeriodCollection));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loadedPerson.PersonSchedulePeriodCollection));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loadedPerson.PermissionInformation.ApplicationRoleCollection));

            var fieldInfo =  loadedPerson.PermissionInformation.GetType().GetField("personInApplicationRole", BindingFlags.Instance | BindingFlags.NonPublic);
            var list = fieldInfo.GetValue(loadedPerson.PermissionInformation);

            Assert.IsTrue(LazyLoadingManager.IsInitialized(list));
        }

 
		[Test]
		public void VerifyFindPersonInOrganizationWithContractIncludedNoYesRuleSetBagInfo()
		{
			ITeam team = TeamFactory.CreateSimpleTeam("hola");
			ITeam team2 = TeamFactory.CreateSimpleTeam("hola2");
			ISite site = SiteFactory.CreateSimpleSite();
			site.AddTeam(team);
			site.AddTeam(team2);
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(team2);

			createPersonWithRuleSets(team);
			createPersonWithRuleSets(team2);

			IPerson loadedPerson = new List<IPerson>(target.FindPeopleInOrganization(new DateOnlyPeriod(2000, 1, 1, 2001, 1, 1), false))[0];
			Assert.IsFalse(LazyLoadingManager.IsInitialized(loadedPerson.PersonPeriodCollection.First().RuleSetBag));
			Session.Clear();
			loadedPerson = new List<IPerson>(target.FindPeopleInOrganization(new DateOnlyPeriod(2000, 1, 1, 2001, 1, 1), true))[0];
			Assert.IsTrue(LazyLoadingManager.IsInitialized(loadedPerson.PersonPeriodCollection.First().RuleSetBag));
			Assert.IsTrue(LazyLoadingManager.IsInitialized(loadedPerson.PersonPeriodCollection.First().RuleSetBag.RuleSetCollection));
			Assert.IsTrue(LazyLoadingManager.IsInitialized(loadedPerson.PersonPeriodCollection.First().RuleSetBag.RuleSetCollection[0].ExtenderCollection));
			Assert.IsTrue(LazyLoadingManager.IsInitialized(loadedPerson.PersonPeriodCollection.First().RuleSetBag.RuleSetCollection[0].LimiterCollection));
			Assert.AreEqual(2, loadedPerson.PersonPeriodCollection.First().RuleSetBag.RuleSetCollection.Count);
		}

		[Test]
		public void VerifyPersonPeriodCollectionIsSorted()
		{
			ITeam team = TeamFactory.CreateSimpleTeam("sdf");
			ISite site = SiteFactory.CreateSimpleSite("sdf");
			team.Site = site;            
			IContract ctr = new Contract("sdf");
			IPartTimePercentage pTime = new PartTimePercentage("sdf");
			IContractSchedule cSc = ContractScheduleFactory.CreateContractSchedule("sdf");

			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(ctr);
			PersistAndRemoveFromUnitOfWork(pTime);
			PersistAndRemoveFromUnitOfWork(cSc);

			IPerson per = PersonFactory.CreatePerson("sdf", "sdf");
			per.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 2), PersonContractFactory.CreatePersonContract(ctr, pTime, cSc), team));
            per.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), PersonContractFactory.CreatePersonContract(ctr, pTime, cSc), team));
			foreach (IPersonPeriod personPeriod in per.PersonPeriodCollection)
			{
				PersistAndRemoveFromUnitOfWork(personPeriod.Team.Site);
				PersistAndRemoveFromUnitOfWork(personPeriod.Team);
			}
			Assert.AreEqual(new DateOnly(2000,1,1), per.PersonPeriodCollection.First().StartDate);
			PersistAndRemoveFromUnitOfWork(per);
			per = Session.Get<Person>(per.Id);
			Assert.AreEqual(new DateOnly(2000, 1, 1), per.PersonPeriodCollection.First().StartDate);
			per.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(1999,1,1)));
			Assert.AreEqual(new DateOnly(1999, 1, 1), per.PersonPeriodCollection.First().StartDate);
			
		}

	  

		private void createPersonWithRuleSets(ITeam team)
		{
			IRuleSetBag bag = createBag();
			IPerson okPerson = PersonFactory.CreatePerson("with rule sets");
			IPersonPeriod periodWithRuleSet =
				new PersonPeriod(new DateOnly(1900, 1, 1), createPersonContract(), team);
			periodWithRuleSet.RuleSetBag = bag;
			IPersonPeriod periodWithRuleSet2 =
				new PersonPeriod(new DateOnly(1900, 1, 2), createPersonContract(), team);
			periodWithRuleSet2.RuleSetBag = bag;
			okPerson.AddPersonPeriod(periodWithRuleSet);
			okPerson.AddPersonPeriod(periodWithRuleSet2);

			PersistAndRemoveFromUnitOfWork(okPerson);
		}

		private IRuleSetBag createBag()
		{
			IActivity dummyActivity = new Activity("dummy");
			PersistAndRemoveFromUnitOfWork(dummyActivity);
			IShiftCategory shiftCat = ShiftCategoryFactory.CreateShiftCategory("dummy");
			PersistAndRemoveFromUnitOfWork(shiftCat);
			IRuleSetBag bag = new RuleSetBag();
			bag.Description = new Description("dummy");
			WorkShiftRuleSet ruleSet =
				new WorkShiftRuleSet(
					new WorkShiftTemplateGenerator(dummyActivity, new TimePeriodWithSegment(1, 1, 1, 1, 1),
												   new TimePeriodWithSegment(1, 1, 1, 1, 1), shiftCat));
			ruleSet.Description = new Description("dummy");
			ruleSet.AddExtender(
				new ActivityAbsoluteStartExtender(dummyActivity, new TimePeriodWithSegment(1, 1, 1, 1, 1),
												  new TimePeriodWithSegment(1, 1, 1, 11, 1)));
			ruleSet.AddExtender(
				new ActivityAbsoluteStartExtender(dummyActivity, new TimePeriodWithSegment(1, 1, 1, 1, 1),
												  new TimePeriodWithSegment(1, 1, 1, 11, 1)));
			ruleSet.AddLimiter(new ContractTimeLimiter(new TimePeriod(10, 10, 11, 11), new TimeSpan()));
			WorkShiftRuleSet ruleSet2 =
				new WorkShiftRuleSet(
					new WorkShiftTemplateGenerator(dummyActivity, new TimePeriodWithSegment(1, 1, 1, 1, 1),
									   new TimePeriodWithSegment(1, 1, 1, 1, 1), shiftCat));
			ruleSet2.AddExtender(
				new ActivityAbsoluteStartExtender(dummyActivity, new TimePeriodWithSegment(1, 1, 1, 1, 1),
												  new TimePeriodWithSegment(1, 1, 1, 11, 1)));
			ruleSet2.AddExtender(
				new ActivityAbsoluteStartExtender(dummyActivity, new TimePeriodWithSegment(1, 1, 1, 1, 1),
												  new TimePeriodWithSegment(1, 1, 1, 11, 1)));
			ruleSet2.AddLimiter(new ContractTimeLimiter(new TimePeriod(10, 10, 11, 11), new TimeSpan()));
			ruleSet2.Description = new Description("dummy");
			PersistAndRemoveFromUnitOfWork(ruleSet);
			PersistAndRemoveFromUnitOfWork(ruleSet2);
			bag.AddRuleSet(ruleSet);
			bag.AddRuleSet(ruleSet2);
			PersistAndRemoveFromUnitOfWork(bag);
			return bag;
		}


		[Test]
		public void VerifyCanLoadPeopleBelongToTeam()
		{
			ISite site1 = SiteFactory.CreateSimpleSite("Site1");
			ITeam team1 = TeamFactory.CreateSimpleTeam("Team1");
			site1.AddTeam(team1);

			ISite site2 = SiteFactory.CreateSimpleSite("Site2");
			ITeam team2 = TeamFactory.CreateSimpleTeam("Team2");
			site2.AddTeam(team2);

			IPartTimePercentage partTimePercentage = new PartTimePercentage("100%");
			IContractSchedule contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			IContract contract = new Contract("Full time");
	
			IPerson per1 = PersonFactory.CreatePerson("sumeda", "Herath");
			IPersonPeriod personPeriod1 =
				PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), new PersonContract(contract,partTimePercentage,contractSchedule), team1);
			per1.AddPersonPeriod(personPeriod1);
			IPersonPeriod personPeriod2 =
				PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2001, 1, 1), new PersonContract(contract, partTimePercentage, contractSchedule), team2);
			per1.AddPersonPeriod(personPeriod2);
			per1.AddSchedulePeriod(SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(2000, 1, 1)));
			//Person2
			IPerson per2 = PersonFactory.CreatePerson("Dinesh", "Ranasinghe");
			IPersonPeriod personPeriod3=
				PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), new PersonContract(contract, partTimePercentage, contractSchedule), team1);
			per2.AddPersonPeriod(personPeriod3);

			//Person3
			IPerson per3 = PersonFactory.CreatePerson("Madhuranga", "Pinnagoda");
			IPersonPeriod personPeriod4 =
				PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), new PersonContract(contract, partTimePercentage, contractSchedule), team2);
			per3.AddPersonPeriod(personPeriod4);
			
			PersistAndRemoveFromUnitOfWork(site1);
			PersistAndRemoveFromUnitOfWork(team1);

			PersistAndRemoveFromUnitOfWork(site2);
			PersistAndRemoveFromUnitOfWork(team2);

			PersistAndRemoveFromUnitOfWork(contractSchedule);
			PersistAndRemoveFromUnitOfWork(partTimePercentage);
			PersistAndRemoveFromUnitOfWork(contract);

			PersistAndRemoveFromUnitOfWork(per1);
			PersistAndRemoveFromUnitOfWork(per2);
			PersistAndRemoveFromUnitOfWork(per3);

			//load
			var testeriod = new DateOnlyPeriod(1999, 12, 31, 2059, 01, 01);
			IList<IPerson> testList = new List<IPerson>(new PersonRepository(UnitOfWork).FindPeopleBelongTeam(team1, testeriod));

			//verify
			testList.Remove(LoggedOnPerson);
			Assert.AreEqual(2, testList.Count);
			Assert.AreEqual(testList[0], per1);
			Assert.AreEqual(testList[1], per2);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(testList[1].PersonPeriodCollection));
			Assert.IsTrue(LazyLoadingManager.IsInitialized(testList[1].PersonPeriodCollection.First().Team));
			Assert.AreEqual(2, testList[0].PersonPeriodCollection.Count());
			Assert.AreEqual(1, testList[1].PersonPeriodCollection.Count());

			testList = new List<IPerson>(new PersonRepository(UnitOfWork).FindPeopleBelongTeamWithSchedulePeriod(team1, testeriod));
			Assert.AreEqual(2, testList.Count);
		}

		[Test]
		public void ShouldNotLoadPeopleBelongingToTeamAfterTerminalDate()
		{
			ISite site1 = SiteFactory.CreateSimpleSite("Site1");
			ITeam team1 = TeamFactory.CreateSimpleTeam("Team1");
			site1.AddTeam(team1);

			IPartTimePercentage partTimePercentage = new PartTimePercentage("100%");
			IContractSchedule contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			IContract contract = new Contract("Full time");

			IPerson per1 = PersonFactory.CreatePerson("sumeda", "Herath");
			IPersonPeriod personPeriod1 =
				PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), new PersonContract(contract, partTimePercentage, contractSchedule), team1);
			per1.AddPersonPeriod(personPeriod1);
			per1.TerminatePerson(new DateOnly(2002,1,1), _personAccountUpdater);

			PersistAndRemoveFromUnitOfWork(site1);
			PersistAndRemoveFromUnitOfWork(team1);

			PersistAndRemoveFromUnitOfWork(contractSchedule);
			PersistAndRemoveFromUnitOfWork(partTimePercentage);
			PersistAndRemoveFromUnitOfWork(contract);

			PersistAndRemoveFromUnitOfWork(per1);
			
			var testeriod = new DateOnlyPeriod(2002, 12, 31, 2059, 01, 01);
			IList<IPerson> testList = new List<IPerson>(new PersonRepository(UnitOfWork).FindPeopleBelongTeam(team1, testeriod));
			testList.Should().Be.Empty();
		}

		[Test]
		public void ShouldFindPeopleBelongTeamWithSchedulePeriod()
		{
			#region setup

			ISite site1 = SiteFactory.CreateSimpleSite("Site1");
			ITeam team1 = TeamFactory.CreateSimpleTeam("Team1");
			site1.AddTeam(team1);

			ISite site2 = SiteFactory.CreateSimpleSite("Site2");
			ITeam team2 = TeamFactory.CreateSimpleTeam("Team2");
			site2.AddTeam(team2);

			// Person1
			IPerson per1 = PersonFactory.CreatePerson("sumeda", "Herath");
			IPersonPeriod personPeriod1 =
				PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), team1);
			per1.AddPersonPeriod(personPeriod1);
			IPersonPeriod personPeriod2 =
				PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2001, 1, 1), team2);
			per1.AddPersonPeriod(personPeriod2);
			per1.AddSchedulePeriod(SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(2000, 1, 1)));
			//Person2
			IPerson per2 = PersonFactory.CreatePerson("Dinesh", "Ranasinghe");
			IPersonPeriod personPeriod3 =
				PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), team1);
			per2.AddPersonPeriod(personPeriod3);

			//Person3
			IPerson per3 = PersonFactory.CreatePerson("Madhuranga", "Pinnagoda");
			IPersonPeriod personPeriod4 =
				PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), team2);
			per3.AddPersonPeriod(personPeriod4);

			#endregion

			#region Persist

			PersistAndRemoveFromUnitOfWork(site1);
			PersistAndRemoveFromUnitOfWork(team1);

			PersistAndRemoveFromUnitOfWork(site2);
			PersistAndRemoveFromUnitOfWork(team2);

			PersistAndRemoveFromUnitOfWork(personPeriod1.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod1.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod1.PersonContract.Contract);

			PersistAndRemoveFromUnitOfWork(personPeriod2.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod2.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod2.PersonContract.Contract);

			PersistAndRemoveFromUnitOfWork(personPeriod3.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod3.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod3.PersonContract.Contract);

			PersistAndRemoveFromUnitOfWork(personPeriod4.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod4.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod4.PersonContract.Contract);

			PersistAndRemoveFromUnitOfWork(per1);
			PersistAndRemoveFromUnitOfWork(per2);
			PersistAndRemoveFromUnitOfWork(per3);

			#endregion

			var testeriod = new DateOnlyPeriod(1999, 12, 31, 2059, 01, 01);
			var testList = new List<IPerson>(new PersonRepository(UnitOfWork).FindPeopleBelongTeamWithSchedulePeriod(team1, testeriod));
			Assert.AreEqual(2, testList.Count);
		}
		
		[Test, ExpectedException(typeof(ConstraintViolationException))]
		public void VerifyCannotSaveTwoUsersWithSameApplicationAuthentication()
		{
			IPerson person1 = PersonFactory.CreatePersonWithBasicPermissionInfo("robink", "robink1");
			PersistAndRemoveFromUnitOfWork(person1);

			//Different password makes no difference!
			IPerson person2 = PersonFactory.CreatePersonWithBasicPermissionInfo("robink", "robink2");
			PersistAndRemoveFromUnitOfWork(person2);
		}

		[Test, ExpectedException(typeof(ConstraintViolationException))]
		public void VerifyCannotSaveTwoUsersWithSameDomainAuthentication()
		{
			IPerson person1 = PersonFactory.CreatePersonWithWindowsPermissionInfo("robink", "toptinet");
			PersistAndRemoveFromUnitOfWork(person1);

			IPerson person2 = PersonFactory.CreatePersonWithWindowsPermissionInfo("robink", "toptinet");
			PersistAndRemoveFromUnitOfWork(person2);
		}

		[Test]
		public void VerifyCanSaveTwoUsersWithSameNameAndDifferentDomain()
		{
			IPerson person1 = PersonFactory.CreatePersonWithWindowsPermissionInfo("robink", "toptinet1");
			PersistAndRemoveFromUnitOfWork(person1);

			IPerson person2 = PersonFactory.CreatePersonWithWindowsPermissionInfo("robink", "toptinet2");
			PersistAndRemoveFromUnitOfWork(person2);
		}

		[Test]
		public void VerifyFindPersonsWithGivenUserCredentials()
		{
			IPerson person1 = PersonFactory.CreatePersonWithWindowsPermissionInfo("sunil", "toptinet1");
			PersistAndRemoveFromUnitOfWork(person1);
			IPerson person2 = PersonFactory.CreatePersonWithWindowsPermissionInfo("kamal", "toptinet1");
			PersistAndRemoveFromUnitOfWork(person2);
			IPerson person3 = PersonFactory.CreatePersonWithWindowsPermissionInfo("nimal", "toptinet1");
			PersistAndRemoveFromUnitOfWork(person3);

			var pr = new PersonRepository(UnitOfWork);

			IList<IPerson> personList = new List<IPerson>(1);
			person1.WindowsAuthenticationInfo.WindowsLogOnName = "kamal";
			personList.Add(person1);

			var returned = pr.FindPersonsWithGivenUserCredentials(personList);
			Assert.That(returned.Count(), Is.GreaterThan(0));

		    personList[0].ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo {ApplicationLogOnName = "virajs", Password = "passadej"};
			returned = pr.FindPersonsWithGivenUserCredentials(personList);
            Assert.That(returned.Count(), Is.GreaterThan(0));
		}

		[Test]
		public void ShouldFindPersonsWithGivenUseCredentialsWithEmptyLogOnName()
		{
			IPerson person1 = PersonFactory.CreatePersonWithWindowsPermissionInfo("sunil", "toptinet1");
			PersistAndRemoveFromUnitOfWork(person1);
			IPerson person2 = PersonFactory.CreatePersonWithWindowsPermissionInfo("", "toptinet1");
			PersistAndRemoveFromUnitOfWork(person2);
			IPerson person3 = PersonFactory.CreatePersonWithWindowsPermissionInfo("nimal", "toptinet1");
			PersistAndRemoveFromUnitOfWork(person3);

			var pr = new PersonRepository(UnitOfWork);

			IList<IPerson> personList = new List<IPerson>(1);
			person1.WindowsAuthenticationInfo.WindowsLogOnName = "";
			personList.Add(person1);

			var returned = pr.FindPersonsWithGivenUserCredentials(personList);
			Assert.That(returned.Count(), Is.EqualTo(1));
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Samisk")]
		[Test, Explicit("This one fails on some enviroments. Xp os? Bug id 6318. Have a look at this again when NH is upgraded to 3.x ")]
		public void VerifyNumberOfActiveAgentsSamiskCulture()
		{
			//rk: tried to change to 2000dialect and used default INamingStrategy. None of these helps....
			var curr = Thread.CurrentThread.CurrentCulture;
			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo(2107);
				verifyNumberOfActiveAgents();
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = curr;
			}
		}

		[Test]
		public void VerifyNumberOfActiveAgentsNormalCulture()
		{
			verifyNumberOfActiveAgents();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void verifyNumberOfActiveAgents()
		{
			SetupPersonsInOrganizationWithContract();
			IList<IPerson> resTemp = new List<IPerson>(target.FindPeopleInOrganization(new DateOnlyPeriod(2000, 1, 1, 2001, 1, 1), false)); //returns 2

			//add pers ass
			IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
			scenario.DefaultScenario = true;
			IActivity act = new Activity("df");
			PersistAndRemoveFromUnitOfWork(scenario);
			PersistAndRemoveFromUnitOfWork(act);
			IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(act, 
																	resTemp[0],
																	new DateTimePeriod(2000, 1, 1, 2000, 1, 2),
																	scenario);
			PersistAndRemoveFromUnitOfWork(ass);

			//change logged on BU
			MockRepository newMock = new MockRepository();
			IState newStateMock = newMock.StrictMock<IState>();
			IBusinessUnit buTemp = BusinessUnitFactory.CreateSimpleBusinessUnit("dummy");
			PersistAndRemoveFromUnitOfWork(buTemp);
			StateHolderProxyHelper.ClearAndSetStateHolder(newMock, LoggedOnPerson, buTemp, SetupFixtureForAssembly.ApplicationData,
													 newStateMock);

			//add pers ass in another BU
			IContract ctr = new Contract("cf");
			IPartTimePercentage part = new PartTimePercentage("d");
			ISite site = SiteFactory.CreateSimpleSite("d");
			IContractSchedule ctrSched = ContractScheduleFactory.CreateContractSchedule("dd");
			PersistAndRemoveFromUnitOfWork(site);
			ITeam team = TeamFactory.CreateSimpleTeam("dd");
			team.Site = site;
			IPerson p = PersonFactory.CreatePerson("ff", "ff");
			p.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(1900,1,1), new PersonContract(ctr, part, ctrSched), team));
			PersistAndRemoveFromUnitOfWork(ctrSched);
			PersistAndRemoveFromUnitOfWork(part);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(ctr);
			PersistAndRemoveFromUnitOfWork(p);
			IScenario scenarioNew = ScenarioFactory.CreateScenarioAggregate("sdf",true);
			PersistAndRemoveFromUnitOfWork(scenarioNew);
			IPersonAssignment assNew = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(act,
																	p,
																	new DateTimePeriod(2000, 1, 1, 2000, 1, 2),
																	scenario);
			PersistAndRemoveFromUnitOfWork(assNew);
			Assert.AreEqual(2, target.NumberOfActiveAgents());
		}

		[Test]
		public void VerifyNumberOfActiveAgentsWhenNone()
		{
			IPersonRepository pr = new PersonRepository(UnitOfWork);
			Assert.AreEqual(0, pr.NumberOfActiveAgents());
		}

		[Test]
		public void VerifyFindPersonUser()
		{
			IPerson person1 = PersonFactory.CreatePersonWithWindowsPermissionInfo("sunil", "toptinet1");
			PersistAndRemoveFromUnitOfWork(person1);
			IPerson person2 = PersonFactory.CreatePersonWithBasicPermissionInfo("kamal", "pwd1");
			PersistAndRemoveFromUnitOfWork(person2);
			IPerson person3 = PersonFactory.CreatePerson("Fname", "lname");
			PersistAndRemoveFromUnitOfWork(person3);

			PersonRepository pr = new PersonRepository(UnitOfWork);

			IList<IPerson> personList = pr.FindPersonsThatAlsoAreUsers();
			Assert.IsTrue(personList.Contains(person1));
			Assert.IsTrue(personList.Contains(person2));
			Assert.IsFalse(personList.Contains(person3));
		}

        [Test]
        public void ShouldFindPersonByEmploymentNumber()
        {
            IPerson person = PersonFactory.CreatePerson("Fname", "lname");
            var employmentNumber = "987392";
            person.EmploymentNumber = employmentNumber;
            PersistAndRemoveFromUnitOfWork(person);

            var pr = new PersonRepository(UnitOfWork);
            IList<IPerson> personList = pr.FindPeopleByEmploymentNumber(employmentNumber).ToList();
            Assert.AreEqual(employmentNumber, personList[0].EmploymentNumber);
        }

		[Test]
		public void VerifyDuplicateExternalLogOnWorksDoesNotCreateDuplicatePeople()
		{
			ITeam team = TeamFactory.CreateSimpleTeam("hola");
			ITeam team2 = TeamFactory.CreateSimpleTeam("hola2");
			ISite site = SiteFactory.CreateSimpleSite();
			site.AddTeam(team);
			site.AddTeam(team2);

			IExternalLogOn logOn1 = ExternalLogOnFactory.CreateExternalLogOn();
			IExternalLogOn logOn2 = ExternalLogOnFactory.CreateExternalLogOn();

			IPerson person = PersonFactory.CreatePersonWithWindowsPermissionInfo("sunil", "toptinet1");
			IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), team);
			var personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2001, 3, 31), team);
			person.AddPersonPeriod(personPeriod1);
			person.AddPersonPeriod(personPeriod2);
			person.AddExternalLogOn(logOn1, personPeriod1);
			person.AddExternalLogOn(logOn2, personPeriod2);
			//IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc), team);
			//person.AddPersonPeriod(personPeriod2);

			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(team2);
			PersistAndRemoveFromUnitOfWork(personPeriod1.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod1.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(personPeriod1.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod2.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod2.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(personPeriod2.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(logOn1);
			PersistAndRemoveFromUnitOfWork(logOn2);
			PersistAndRemoveFromUnitOfWork(person);

			ICollection<IPerson> res = target.FindPeopleInOrganization(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 2), false);
			Assert.AreEqual(1, res.Count);
			IPerson per = res.First();
			Assert.AreEqual(2, per.PersonPeriodCollection.Count());
			Assert.AreEqual(2, per.PersonPeriodCollection.Sum(x => x.ExternalLogOnCollection.Count));
		}

        [Test]
        public void ShouldSaveAndLoadFirstDayOfWeek()
        {
            IPerson person = PersonFactory.CreatePerson("Fname", "lname");
            Assert.That(person.FirstDayOfWeek.Equals(DayOfWeek.Monday));
            person.FirstDayOfWeek = DayOfWeek.Saturday;
            PersistAndRemoveFromUnitOfWork(person);

            var pr = new PersonRepository(UnitOfWork);
            var loaded = pr.Load(person.Id.Value);
            Assert.That(loaded.FirstDayOfWeek.Equals(DayOfWeek.Saturday));
        }

		[Test]
		public void VerifyFindAllSortByName()
		{
			ISite site = SiteFactory.CreateSimpleSite("d");
			PersistAndRemoveFromUnitOfWork(site);
			ITeam team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.Description = new Description("sdf");
			PersistAndRemoveFromUnitOfWork(team);

			IPerson per1 = PersonFactory.CreatePerson("roger", "kratz");
			IPerson per2 = PersonFactory.CreatePerson("z", "balog");
			IPerson per3 = PersonFactory.CreatePerson("a", "balog");
			per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2000, 1, 1), createPersonContract(), team));
			per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2001, 1, 1), createPersonContract(), team));
			per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2002, 1, 1), createPersonContract(), team));

			PersistAndRemoveFromUnitOfWork(per1);
			PersistAndRemoveFromUnitOfWork(per2);
			PersistAndRemoveFromUnitOfWork(per3);

			//load
			IList<IPerson> testList = new List<IPerson>(
					new PersonRepository(UnitOfWork).FindAllSortByName());
			//verify
			testList.Remove(LoggedOnPerson);
			Assert.AreEqual(3, testList.Count);
			Assert.AreEqual(testList[0], per3);
			Assert.AreEqual(testList[1], per2);
			Assert.AreEqual(testList[2], per1);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(testList[1].PersonPeriodCollection));
		}

		[Test]
		public void ShouldNotFindPeopleInOtherBusinessUnits()
		{
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit();
			PersistAndRemoveFromUnitOfWork(businessUnit);
			var site = SiteFactory.CreateSimpleSite("d");
			site.SetBusinessUnit(businessUnit);
			PersistAndRemoveFromUnitOfWork(site);
			var team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.Description = new Description("sdf");
			PersistAndRemoveFromUnitOfWork(team);

			IPerson per1 = PersonFactory.CreatePerson("roger", "kratz");
			per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2000, 1, 1), createPersonContract(businessUnit), team));

			PersistAndRemoveFromUnitOfWork(per1);

			//load
			IList<IPerson> testList = new List<IPerson>(
					new PersonRepository(UnitOfWork).FindAllSortByName());
			//verify
			Assert.AreEqual(0, testList.Count);
		}

		[Test]
		public void ShouldSaveOptionalColumnValueOnPerson()
		{
			var column = new OptionalColumn("COL1"){TableName = "Person"};
			PersistAndRemoveFromUnitOfWork(column);
			IPerson per1 = PersonFactory.CreatePerson("roger", "kratz");
			per1.AddOptionalColumnValue(new OptionalColumnValue("A VALUE"),column );
			PersistAndRemoveFromUnitOfWork(per1);

			IList<IPerson> testList = new List<IPerson>(
					new PersonRepository(UnitOfWork).FindAllSortByName());
			Assert.AreEqual(testList[0], per1);
			Assert.That(testList[0].OptionalColumnValueCollection.Count,Is.EqualTo(1));
		}

		[Test]
		public void ShouldSaveLoginAttempt()
		{
			var rep = new PersonRepository(UnitOfWork);
			var model = new LoginAttemptModel{ClientIp = "172.168.1.1",Provider = "Win", Client = "Web",Result = "Success",UserCredentials = "aa"};
			var result = rep.SaveLoginAttempt(model);
			Assert.That(result,Is.EqualTo(1));
		}

		private static void verifyPermissionInfoIsLazy(bool expected, IPerson userRetOk)
		{
			Assert.AreEqual(expected,
				(LazyLoadingManager.IsInitialized(
					userRetOk.PermissionInformation.ApplicationRoleCollection)
				 &&
				 LazyLoadingManager.IsInitialized(
					userRetOk.PermissionInformation.ApplicationRoleCollection[0].ApplicationFunctionCollection)));
		}
	  
		private IPersonContract createPersonContract(IBusinessUnit otherBusinessUnit = null)
		{
			var pContract = PersonContractFactory.CreatePersonContract();
			if (otherBusinessUnit != null)
			{
				pContract.Contract.SetBusinessUnit(otherBusinessUnit);
				pContract.ContractSchedule.SetBusinessUnit(otherBusinessUnit);
				pContract.PartTimePercentage.SetBusinessUnit(otherBusinessUnit);
			}
			PersistAndRemoveFromUnitOfWork(pContract.Contract);
			PersistAndRemoveFromUnitOfWork(pContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(pContract.PartTimePercentage);
			return pContract;
		}

		private ApplicationRole createAndPersistApplicationRole()
		{
			ApplicationRole role = new ApplicationRole();
			role.DescriptionText = "description";
			role.Name = "name";
			role.SetBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
			ApplicationFunction fkn1 = new ApplicationFunction("FUNCTION1");
			ApplicationFunction fkn2 = new ApplicationFunction("FUNCTION2");
			AvailableData ad = new AvailableData();
			role.AddApplicationFunction(fkn1);
			role.AddApplicationFunction(fkn2);
			ad.ApplicationRole = role;
			PersistAndRemoveFromUnitOfWork(fkn1);
			PersistAndRemoveFromUnitOfWork(fkn2);
			PersistAndRemoveFromUnitOfWork(role);
			PersistAndRemoveFromUnitOfWork(ad);
			role.AvailableData = ad;
			return role;
		}

		private void createSiteAndTeam()
		{

			ISite site = SiteFactory.CreateSimpleSite("Site belonging to BU used in test");
			PersistAndRemoveFromUnitOfWork(site);
			ISite site2 = SiteFactory.CreateSimpleSite("Site2 belonging to BU used in test");
			PersistAndRemoveFromUnitOfWork(site2);
			ITeam team = TeamFactory.CreateSimpleTeam();
			team.Description = new Description("team1");
			site.AddTeam(team);
			PersistAndRemoveFromUnitOfWork(team);
			ITeam team2 = TeamFactory.CreateSimpleTeam();
			team2.Description = new Description("team2");
			site2.AddTeam(team2);
			PersistAndRemoveFromUnitOfWork(team2);
			Session.Refresh(BusinessUnitFactory.BusinessUnitUsedInTest);
		}

		private class justForTest : PersonRepository
		{
			public justForTest(IUnitOfWork unitOfWork)
				: base(unitOfWork)
			{
			}

			public ISession InternalSession
			{
				get
				{
					return Session;
				}
			}
		}

		protected override Repository<IPerson> TestRepository(IUnitOfWork unitOfWork)
		{
			return new PersonRepository(unitOfWork);
		}

		public override void CannotCallDatabaseWhenNotLoggedOn()
		{
			Assert.Ignore("PersonRepository should be available even if not logged on");
		}
	}
}
