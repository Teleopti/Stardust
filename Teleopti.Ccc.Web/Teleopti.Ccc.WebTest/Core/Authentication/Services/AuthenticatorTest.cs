﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Authentication.Services
{
	[TestFixture]
	public class AuthenticatorTest
	{
		private IApplicationData applicationData;
		private IRepositoryFactory repositoryFactory;
		private Authenticator target;
		private ITokenIdentityProvider tokenIdentityProvider;
		private IFindTenantAndPersonIdForIdentity findTenantAndPersonIdForIdentity;
		const string tenant = "Gurkmajonääääs";

		[SetUp]
		public void Setup()
		{
			applicationData = MockRepository.GenerateMock<IApplicationData>();
			repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			tokenIdentityProvider = MockRepository.GenerateMock<ITokenIdentityProvider>();
			findTenantAndPersonIdForIdentity = MockRepository.GenerateMock<IFindTenantAndPersonIdForIdentity>();
			target = new Authenticator(applicationData, tokenIdentityProvider, repositoryFactory, findTenantAndPersonIdForIdentity);
		}

		[Test]
		public void AuthenticateIdentityUserShouldReturnSuccessfulAuthenticationResult()
		{
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var personRep = MockRepository.GenerateMock<IPersonRepository>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var winAccount = new TokenIdentity { UserIdentifier = @"domain\user", OriginalToken = @"http://fake/domain#user" };
			var tenantAndPersonId = new TenantAndPersonId {PersonId = Guid.NewGuid(), Tenant = tenant};
			var person = new Person();

			applicationData.Stub(x => x.Tenant(tenant)).Return(dataSource);
			dataSource.Stub(x => x.Application).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRep);
			findTenantAndPersonIdForIdentity.Stub(x => x.Find(winAccount.UserIdentifier)).Return(tenantAndPersonId);

			tokenIdentityProvider.Stub(x => x.RetrieveToken()).Return(winAccount);
			personRep.Stub(x => x.LoadPersonAndPermissions(tenantAndPersonId.PersonId)).Return(person);

			var result = target.LogonIdentityUser();
			result.Person.Should().Be.SameInstanceAs(person);
			result.Successful.Should().Be.True();
			result.DataSource.Should().Be.SameInstanceAs(dataSource);
		}

		[Test]
		public void NonExistingUserShouldReturnFailure()
		{
			var identity = RandomName.Make();
			tokenIdentityProvider.Stub(x => x.RetrieveToken()).Return(new TokenIdentity {UserIdentifier = identity});
			findTenantAndPersonIdForIdentity.Stub(x=> x.Find(identity)).Return(null);

			target.LogonIdentityUser().Successful
				.Should().Be.False();
		}

		[Test]
		public void TerminatedPersonShouldReturnFailure()
		{
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var personRep = MockRepository.GenerateMock<IPersonRepository>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var winAccount = new TokenIdentity { UserIdentifier = @"domain\user", OriginalToken = @"http://fake/domain#user" };
			var tenantAndPersonId = new TenantAndPersonId { PersonId = Guid.NewGuid(), Tenant = tenant };
			var person = new Person();
			person.TerminatePerson(DateOnly.Today.AddDays(-3), new PersonAccountUpdaterDummy());

			applicationData.Stub(x => x.Tenant(tenant)).Return(dataSource);
			dataSource.Stub(x => x.Application).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRep);
			findTenantAndPersonIdForIdentity.Stub(x => x.Find(winAccount.UserIdentifier)).Return(tenantAndPersonId);

			tokenIdentityProvider.Stub(x => x.RetrieveToken()).Return(winAccount);
			personRep.Stub(x => x.LoadPersonAndPermissions(tenantAndPersonId.PersonId)).Return(person);

			var result = target.LogonIdentityUser();
			result.Successful.Should().Be.False();
		}
	}
}