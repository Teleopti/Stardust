﻿using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Authentication.Services
{
	[TestFixture]
	public class AuthenticatorTest
	{
		private IDataSourcesProvider dataSourcesProvider;
		private IRepositoryFactory repositoryFactory;
		private Authenticator target;
		private ITokenIdentityProvider tokenIdentityProvider;
		private IIdentityUserQuery identityUserQuery;
		const string tenant = "Gurkmajonääääs";

		[SetUp]
		public void Setup()
		{
			dataSourcesProvider = MockRepository.GenerateMock<IDataSourcesProvider>();
			repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			tokenIdentityProvider = MockRepository.GenerateMock<ITokenIdentityProvider>();
			identityUserQuery = MockRepository.GenerateMock<IIdentityUserQuery>();
			target = new Authenticator(dataSourcesProvider, tokenIdentityProvider, repositoryFactory, identityUserQuery);
		}

		[Test]
		public void AuthenticateWindowsUserShouldReturnSuccessfulAuthenticationResult()
		{
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var personRep = MockRepository.GenerateMock<IPersonRepository>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var winAccount = new TokenIdentity { UserIdentifier = @"domain\user", OriginalToken = @"http://fake/domain#user" };
			var pInfo = new PersonInfo();
			pInfo.SetTenant_DoNotUseThisIfYouAreNotSureWhatYouAreDoing(tenant);

			IPerson person = null;

			dataSourcesProvider.Stub(x => x.RetrieveDataSourceByName(tenant)).Return(dataSource);
			dataSource.Stub(x => x.Application).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRep);
			identityUserQuery.Stub(x => x.FindUserData(winAccount.UserIdentifier)).Return(pInfo);

			tokenIdentityProvider.Stub(x => x.RetrieveToken()).Return(winAccount);

			personRep.Stub(x => x.TryFindIdentityAuthenticatedPerson(winAccount.UserIdentifier, out person)).Return(true);

			var result = target.LogonIdentityUser();
			result.Person.Should().Be.EqualTo(person);
			result.Successful.Should().Be.True();
			result.DataSource.Should().Be.SameInstanceAs(dataSource);
		}

		[Test]
		public void AuthenticateApplicationIdentityUserShouldReturnSuccessfulAuthenticationResult()
		{
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var personRep = MockRepository.GenerateMock<IPersonRepository>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var account = new TokenIdentity { DataSource = "Teleopti WFM", UserIdentifier = "user" };

			IPerson person = new Person();

			dataSourcesProvider.Stub(x => x.RetrieveDataSourceByName(tenant)).Return(dataSource);
			dataSource.Stub(x => x.Application).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRep);

			tokenIdentityProvider.Stub(x => x.RetrieveToken()).Return(account);

			personRep.Stub(x => x.TryFindBasicAuthenticatedPerson(account.UserIdentifier)).Return(person);

			var result = target.LogonApplicationUser(tenant);
			result.Person.Should().Be.EqualTo(person);
			result.Successful.Should().Be.True();
			result.DataSource.Should().Be.SameInstanceAs(dataSource);
		}
	}
}