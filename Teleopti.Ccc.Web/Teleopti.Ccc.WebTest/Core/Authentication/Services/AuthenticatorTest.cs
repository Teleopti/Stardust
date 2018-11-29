using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;


namespace Teleopti.Ccc.WebTest.Core.Authentication.Services
{
	[TestFixture]
	public class AuthenticatorTest
	{
		private IDataSourceForTenant dataSourceForTenant;
		private Authenticator target;
		private ITokenIdentityProvider tokenIdentityProvider;
		private IFindPersonInfoByIdentity _findPersonInfoByIdentity;
		private ILoadUserUnauthorized loadUserUnauthorized;
		private Tenant tenant;

		[SetUp]
		public void Setup()
		{
			tenant = new Tenant(RandomName.Make());
			dataSourceForTenant = MockRepository.GenerateMock<IDataSourceForTenant>();
			tokenIdentityProvider = MockRepository.GenerateMock<ITokenIdentityProvider>();
			_findPersonInfoByIdentity = MockRepository.GenerateMock<IFindPersonInfoByIdentity>();
			loadUserUnauthorized = MockRepository.GenerateStub<ILoadUserUnauthorized>();
			target = new Authenticator(dataSourceForTenant, tokenIdentityProvider, _findPersonInfoByIdentity, loadUserUnauthorized);
		}

		[Test]
		public void AuthenticateIdentityUserShouldReturnSuccessfulAuthenticationResult()
		{
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var winAccount = new TokenIdentity { UserIdentifier = @"domain\user", OriginalToken = @"http://fake/domain#user", IsPersistent = true};
			var personInfo = new PersonInfo(tenant, Guid.NewGuid());
			var person = new Person();

			dataSourceForTenant.Stub(x => x.Tenant(tenant.Name)).Return(dataSource);
			dataSource.Stub(x => x.Application).Return(unitOfWorkFactory);
			loadUserUnauthorized.Stub(x => x.LoadFullPersonInSeperateTransaction(unitOfWorkFactory, personInfo.Id)).Return(person);
			_findPersonInfoByIdentity.Stub(x => x.Find(winAccount.UserIdentifier, false)).Return(personInfo);

			tokenIdentityProvider.Stub(x => x.RetrieveToken()).Return(winAccount);

			var result = target.LogonIdentityUser();
			result.Person.Should().Be.SameInstanceAs(person);
			result.Successful.Should().Be.True();
			result.DataSource.Should().Be.SameInstanceAs(dataSource);
			result.TenantPassword.Should().Be.EqualTo(personInfo.TenantPassword);
			result.IsPersistent.Should().Be.True();
		}

		[Test]
		public void NonExistingUserShouldReturnFailure()
		{
			var identity = RandomName.Make();
			tokenIdentityProvider.Stub(x => x.RetrieveToken()).Return(new TokenIdentity {UserIdentifier = identity});
			_findPersonInfoByIdentity.Stub(x=> x.Find(identity, false)).Return(null);

			target.LogonIdentityUser().Successful
				.Should().Be.False();
		}

		[Test]
		public void TerminatedPersonShouldReturnFailure()
		{
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var winAccount = new TokenIdentity { UserIdentifier = @"domain\user", OriginalToken = @"http://fake/domain#user" };
			var personInfo = new PersonInfo(tenant, Guid.NewGuid());
			var person = new Person();
			person.TerminatePerson(DateOnly.Today.AddDays(-3), new PersonAccountUpdaterDummy());

			dataSourceForTenant.Stub(x => x.Tenant(tenant.Name)).Return(dataSource);
			dataSource.Stub(x => x.Application).Return(unitOfWorkFactory);
			_findPersonInfoByIdentity.Stub(x => x.Find(winAccount.UserIdentifier, false)).Return(personInfo);
			loadUserUnauthorized.Stub(x => x.LoadFullPersonInSeperateTransaction(unitOfWorkFactory, personInfo.Id)).Return(person);

			tokenIdentityProvider.Stub(x => x.RetrieveToken()).Return(winAccount);

			var result = target.LogonIdentityUser();
			result.Successful.Should().Be.False();
		}
	}
}