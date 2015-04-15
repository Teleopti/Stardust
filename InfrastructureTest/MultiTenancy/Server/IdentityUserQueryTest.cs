using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server
{
	public class IdentityUserQueryTest : DatabaseTestWithoutTransaction
	{
		private Guid personId;
		private string correctIdentity;
		private IIdentityUserQuery target;
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;

		[Test]
		public void ShouldFindPersonId()
		{
			var result = target.FindUserData(correctIdentity);
			result.Id.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldFindTenant()
		{
			var result = target.FindUserData(correctIdentity);
			result.Tenant.Should().Be.EqualTo(Tenant.DefaultName);
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


			target.FindUserData(correctIdentity)
				.Should().Be.Null();
		}

		[SetUp]
		public void Setup_WillBeChangedWhenMovedAwayFromUnitOfWork()
		{
			correctIdentity = RandomName.Make();
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForTest(ConnectionStringHelper.ConnectionStringUsedInTests);
			var tenant = new FindTenantByNameQuery(_tenantUnitOfWorkManager).Find(Tenant.DefaultName);
			var pInfo = new PersonInfo(tenant);
			pInfo.SetIdentity(correctIdentity);
			var personInfoPersister = new PersistPersonInfo(_tenantUnitOfWorkManager);
			personInfoPersister.Persist(pInfo);
			personId = pInfo.Id;
			_tenantUnitOfWorkManager.CurrentSession().Flush();
			target = new IdentityUserQuery(_tenantUnitOfWorkManager);
		}

		[TearDown]
		public void Teardown_WillBeChangedWhenMovedAwayFromUnitOfWork()
		{
			_tenantUnitOfWorkManager.Dispose();
		}
	}

	public class IdentityUserQueryTest_OldSchema_RemoveWhenToggleIsGone : DatabaseTestWithoutTransaction
	{
		private Guid personId;
		private string correctIdentity;
		private IIdentityUserQuery target;
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;

		[Test]
		public void ShouldFindPersonId()
		{
			var result = target.FindUserData(correctIdentity);
			result.Id.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldFindTenant()
		{
			var result = target.FindUserData(correctIdentity);
			result.Tenant.Should().Be.EqualTo(Tenant.DefaultName);
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

			target.FindUserData(correctIdentity)
				.Should().Be.Null();
		}

		[Test]
		public void DeletedUserShouldFail()
		{
			var personInDatabase = Session.Get<Person>(personId);
			personInDatabase.SetDeleted();
			PersistAndRemoveFromUnitOfWork(personInDatabase);

			target.FindUserData(correctIdentity)
				.Should().Be.Null();
		}

		[SetUp]
		public void Setup_WillBeChangedWhenMovedAwayFromUnitOfWork()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				correctIdentity = "arna";
				var personInDatabase = PersonFactory.CreatePersonWithIdentityPermissionInfo(correctIdentity);
				new PersonRepository(uow).Add(personInDatabase);
				uow.PersistAll();
				personId = personInDatabase.Id.Value;
			}
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForTest(ConnectionStringHelper.ConnectionStringUsedInTests);
			target = new IdentityUserQuery_OldSchema(_tenantUnitOfWorkManager);
		}

		[TearDown]
		public void Teardown_WillBeChangedWhenMovedAwayFromUnitOfWork()
		{
			_tenantUnitOfWorkManager.CancelCurrent();
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new PersonRepository(uow);
				var personInDatabase = rep.Get(personId);
				rep.Remove(personInDatabase);
				uow.FetchSession().CreateQuery("delete from UserDetail").ExecuteUpdate();
				uow.PersistAll();
			}
		}
	}
}