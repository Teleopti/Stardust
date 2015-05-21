﻿using System;
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
			result.Tenant.Name.Should().Be.EqualTo(Tenant.DefaultName);
		}

		[Test]
		public void NonExistingUserShouldFail()
		{
			var result = target.FindUserData("incorrectUserName");
			result.Should().Be.Null();
		}


		[SetUp]
		public void Setup_WillBeChangedWhenMovedAwayFromUnitOfWork()
		{
			correctIdentity = RandomName.Make();
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(ConnectionStringHelper.ConnectionStringUsedInTests);
			var tenant = new FindTenantByNameQuery(_tenantUnitOfWorkManager).Find(Tenant.DefaultName);
			personId = Guid.NewGuid();
			var pInfo = new PersonInfo(tenant, personId);
			pInfo.SetIdentity(correctIdentity);
			var personInfoPersister = new PersistPersonInfo(_tenantUnitOfWorkManager);
			personInfoPersister.Persist(pInfo);
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
			result.Tenant.Name.Should().Be.EqualTo(Tenant.DefaultName);
		}
		
		[Test]
		public void NonExistingUserShouldFail()
		{
			var result = target.FindUserData("incorrectUserName");
			result.Should().Be.Null();
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
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(ConnectionStringHelper.ConnectionStringUsedInTests);
			target = new IdentityUserQuery_OldSchema(_tenantUnitOfWorkManager);
		}

		[TearDown]
		public void Teardown_WillBeChangedWhenMovedAwayFromUnitOfWork()
		{
			_tenantUnitOfWorkManager.CancelAndDisposeCurrent();
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