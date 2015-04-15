using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server
{
	public class ApplicationUserQueryTest
	{
		private Guid personId;
		private string correctUserName;
		private IApplicationUserQuery target;
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;

		[Test]
		public void ShouldFindPersonId()
		{
			var result = target.FindUserData(correctUserName);
			result.PersonInfo.Id.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldFindTenant()
		{
			var result = target.FindUserData(correctUserName);
			result.PersonInfo.Tenant.Should().Be.EqualTo(Tenant.DefaultName);
		}

		[Test]
		public void ShouldFindPassword()
		{
			var result = target.FindUserData(correctUserName);
			result.PersonInfo.Password.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldFindUserDetails()
		{
			var tenantSession = _tenantUnitOfWorkManager.CurrentSession();
			var personInDatabase = tenantSession.Get<PersonInfo>(personId);

			var passwordPolicyForUser = new PasswordPolicyForUser(personInDatabase)
			{
				LastPasswordChange = DateTime.UtcNow,
				InvalidAttemptsSequenceStart = DateTime.UtcNow.AddHours(-1),
				InvalidAttempts = 73
			};
			tenantSession.Save(passwordPolicyForUser);
			tenantSession.Flush();


			var result = target.FindUserData(correctUserName);
			result.LastPasswordChange.Should().Be.IncludedIn(passwordPolicyForUser.LastPasswordChange.AddMinutes(-1), passwordPolicyForUser.LastPasswordChange.AddMinutes(1));
			result.InvalidAttemptsSequenceStart.Should().Be.IncludedIn(passwordPolicyForUser.InvalidAttemptsSequenceStart.AddMinutes(-1), passwordPolicyForUser.InvalidAttemptsSequenceStart.AddMinutes(1));
			result.InvalidAttempts.Should().Be.EqualTo(passwordPolicyForUser.InvalidAttempts);
			result.IsLocked.Should().Be.EqualTo(passwordPolicyForUser.IsLocked);
		}
		
		[Test]
		public void NonExistingUserShouldFail()
		{
			var result = target.FindUserData("incorrectUserName");
			result.Should().Be.Null();
		}

		[Test]
		public void TerminatedUserShouldFail()
		{
			var personInDatabase = _tenantUnitOfWorkManager.CurrentSession().Get<PersonInfo>(personId);
			personInDatabase.TerminalDate = DateOnly.Today.AddDays(-2);

			target.FindUserData(correctUserName)
				.Should().Be.Null();
		}

		[Test]
		public void UserOnTerminateDateShouldSucceed()
		{
			var personInDatabase = _tenantUnitOfWorkManager.CurrentSession().Get<PersonInfo>(personId);
			personInDatabase.TerminalDate = DateOnly.Today;

			target.FindUserData(correctUserName)
				.Should().Be.OfType<PasswordPolicyForUser>();
		}


		[Test]
		public void ShouldPersistPasswordPolicyIfNonExistingButUserExist()
		{
			var session = _tenantUnitOfWorkManager.CurrentSession();
			session.GetNamedQuery("passwordPolicyForUser").SetGuid("personInfoId", personId).UniqueResult().Should().Be.Null();
			target.FindUserData(correctUserName);
			session.GetNamedQuery("passwordPolicyForUser").SetGuid("personInfoId", personId).UniqueResult().Should().Not.Be.Null();
		}


		[SetUp]
		public void Setup_WillBeChangedWhenMovedAwayFromUnitOfWork()
		{
			correctUserName = RandomName.Make();
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForTest(ConnectionStringHelper.ConnectionStringUsedInTests);
			var tenant = new FindTenantByNameQuery(_tenantUnitOfWorkManager).Find(Tenant.DefaultName);
			var pInfo = new PersonInfo(tenant);
			pInfo.SetApplicationLogonName(correctUserName);
			pInfo.SetPassword(RandomName.Make());
			var personInfoPersister = new PersistPersonInfo(_tenantUnitOfWorkManager);
			personInfoPersister.Persist(pInfo);
			personId = pInfo.Id;
			_tenantUnitOfWorkManager.CurrentSession().Flush();

			target = new ApplicationUserQuery(_tenantUnitOfWorkManager, new ApplicationUserTenantQuery(_tenantUnitOfWorkManager));
		}

		[TearDown]
		public void Clean()
		{
			_tenantUnitOfWorkManager.Dispose();
		}
	}





	public class ApplicationUserQueryTest_OldSchema_RemoveMeWhenToggleIsGone : DatabaseTestWithoutTransaction
	{
		private Guid personId;
		private string correctUserName;
		private IApplicationUserQuery target;
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;

		[Test]
		public void ShouldFindPersonId()
		{
			var result = target.FindUserData(correctUserName);
			result.PersonInfo.Id.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldFindTenant()
		{
			var result = target.FindUserData(correctUserName);
			result.PersonInfo.Tenant.Should().Be.EqualTo(Tenant.DefaultName);
		}

		[Test]
		public void ShouldFindPassword()
		{
			var result = target.FindUserData(correctUserName);
			result.PersonInfo.Password.Should().Not.Be.Null();
			result.PersonInfo.Password.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldFindUserDetails()
		{
			var personInDatabase = Session.Get<Person>(personId);
			var userDetails = new UserDetail(personInDatabase)
			{
				LastPasswordChange = DateTime.UtcNow,
				InvalidAttemptsSequenceStart = DateTime.UtcNow.AddHours(-1),
				InvalidAttempts = 73
			};
			PersistAndRemoveFromUnitOfWork(userDetails);


			var result = target.FindUserData(correctUserName);
			result.LastPasswordChange.Should()
				.Be.IncludedIn(userDetails.LastPasswordChange.AddMinutes(-1), userDetails.LastPasswordChange.AddMinutes(1));
			result.InvalidAttemptsSequenceStart.Should()
				.Be.IncludedIn(userDetails.InvalidAttemptsSequenceStart.AddMinutes(-1),
					userDetails.InvalidAttemptsSequenceStart.AddMinutes(1));
			result.InvalidAttempts.Should().Be.EqualTo(userDetails.InvalidAttempts);
			result.IsLocked.Should().Be.EqualTo(userDetails.IsLocked);
		}

		[Test]
		public void NonExistingUserShouldFail()
		{
			var result = target.FindUserData("incorrectUserName");
			result.Should().Be.Null();
		}

		[Test]
		public void TerminatedUserShouldFail()
		{
			var personInDatabase = Session.Get<Person>(personId);
			personInDatabase.TerminatePerson(new DateOnly(DateTime.Now.AddDays(-2)), new PersonAccountUpdaterDummy());
			PersistAndRemoveFromUnitOfWork(personInDatabase);

			target.FindUserData(correctUserName)
				.Should().Be.Null();
		}

		[Test]
		public void UserOnTerminateDateShouldSucceed()
		{
			var personInDatabase = Session.Get<Person>(personId);
			personInDatabase.TerminatePerson(new DateOnly(DateTime.Today), new PersonAccountUpdaterDummy());
			PersistAndRemoveFromUnitOfWork(personInDatabase);

			target.FindUserData(correctUserName)
				.Should().Be.OfType<PasswordPolicyForUser>();
		}

		[Test]
		public void DeletedUserShouldFail()
		{
			var personInDatabase = Session.Get<Person>(personId);
			personInDatabase.SetDeleted();
			PersistAndRemoveFromUnitOfWork(personInDatabase);

			target.FindUserData(correctUserName)
				.Should().Be.Null();
		}


		[Test]
		public void ShouldPersistPasswordPolicyIfNonExistingButUserExist()
		{
			var session = _tenantUnitOfWorkManager.CurrentSession();
			session.GetNamedQuery("passwordPolicyForUser_OldSchema").SetGuid("personInfoId", personId).UniqueResult().Should().Be.Null();
			target.FindUserData(correctUserName);
			session.GetNamedQuery("passwordPolicyForUser_OldSchema")
				.SetGuid("personInfoId", personId)
				.UniqueResult()
				.Should()
				.Not.Be.Null();
		}


		[SetUp]
		public void Setup_WillBeChangedWhenMovedAwayFromUnitOfWork()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				correctUserName = "arna";
				var personInDatabase = PersonFactory.CreatePersonWithBasicPermissionInfo(correctUserName, "something");
				new PersonRepository(uow).Add(personInDatabase);
				uow.PersistAll();
				personId = personInDatabase.Id.Value;
			}
			_tenantUnitOfWorkManager =
				TenantUnitOfWorkManager.CreateInstanceForTest(ConnectionStringHelper.ConnectionStringUsedInTests);
			target = new ApplicationUserQuery_OldSchema(_tenantUnitOfWorkManager, new ApplicationUserTenantQuery_OldSchema(_tenantUnitOfWorkManager));
		}
	}

}