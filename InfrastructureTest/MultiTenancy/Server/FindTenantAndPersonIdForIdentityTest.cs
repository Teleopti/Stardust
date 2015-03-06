using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server
{
	public class FindTenantAndPersonIdForIdentityTest
	{
		[Test]
		public void ShouldFindIdentityLogon()
		{
			var identity = RandomName.Make();

			var pInfo = new PersonInfo(RandomName.Make()) {Id = Guid.NewGuid()};

			var identityUserQuery = MockRepository.GenerateStub<IIdentityUserQuery>();
			identityUserQuery.Stub(x => x.FindUserData(identity)).Return(pInfo);

			var target = new FindTenantAndPersonIdForIdentity(identityUserQuery, MockRepository.GenerateStub<IApplicationUserTenantQuery>());
			var result = target.Find(identity);
			result.Tenant.Should().Be.EqualTo(pInfo.Tenant);
			result.PersonId.Should().Be.EqualTo(pInfo.Id);
		}

		[Test]
		public void ShouldFindApplicationLogon()
		{
			var identity = RandomName.Make();

			var pInfo = new PersonInfo(RandomName.Make()) { Id = Guid.NewGuid() };

			var applicationQuery = MockRepository.GenerateStub<IApplicationUserTenantQuery>();
			applicationQuery.Stub(x => x.Find(identity)).Return(pInfo);

			var target = new FindTenantAndPersonIdForIdentity(MockRepository.GenerateStub<IIdentityUserQuery>(), applicationQuery);
			var result = target.Find(identity);
			result.Tenant.Should().Be.EqualTo(pInfo.Tenant);
			result.PersonId.Should().Be.EqualTo(pInfo.Id);
		}

		[Test]
		public void ShouldChooseIdentityIfExistInBoth()
		{
			var identity = RandomName.Make();

			var pInfo = new PersonInfo(RandomName.Make()) { Id = Guid.NewGuid() };


			var identityUserQuery = MockRepository.GenerateStub<IIdentityUserQuery>();
			identityUserQuery.Stub(x => x.FindUserData(identity)).Return(pInfo);

			var applicationQuery = MockRepository.GenerateStub<IApplicationUserTenantQuery>();
			applicationQuery.Stub(x => x.Find(identity)).Return(new PersonInfo());

			var target = new FindTenantAndPersonIdForIdentity(identityUserQuery, applicationQuery);
			var result = target.Find(identity);
			result.Tenant.Should().Be.EqualTo(pInfo.Tenant);
			result.PersonId.Should().Be.EqualTo(pInfo.Id);
		}

		[Test]
		public void ShouldReturnNullIfNotExisting()
		{
			var identity = RandomName.Make();

			var target = new FindTenantAndPersonIdForIdentity(MockRepository.GenerateStub<IIdentityUserQuery>(), MockRepository.GenerateStub<IApplicationUserTenantQuery>());
			target.Find(identity)
				.Should().Be.Null();
		}
	}
}