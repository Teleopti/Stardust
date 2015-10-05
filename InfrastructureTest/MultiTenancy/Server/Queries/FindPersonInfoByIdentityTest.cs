using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Queries
{
	public class FindPersonInfoByIdentityTest
	{
		[Test]
		public void ShouldFindIdentityLogon()
		{
			var identity = RandomName.Make();

			var pInfo = new PersonInfo();

			var identityUserQuery = MockRepository.GenerateStub<IIdentityUserQuery>();
			identityUserQuery.Stub(x => x.FindUserData(identity)).Return(pInfo);

			var target = new FindPersonInfoByIdentity(identityUserQuery, MockRepository.GenerateStub<IApplicationUserQuery>());
			var result = target.Find(identity, false);
			result.Tenant.Name.Should().Be.EqualTo(pInfo.Tenant.Name);
			result.Id.Should().Be.EqualTo(pInfo.Id);
		}

		[Test]
		public void ShouldFindApplicationLogon()
		{
			var identity = RandomName.Make();

			var pInfo = new PersonInfo();

			var applicationQuery = MockRepository.GenerateStub<IApplicationUserQuery>();
			applicationQuery.Stub(x => x.Find(identity)).Return(pInfo);

			var target = new FindPersonInfoByIdentity(MockRepository.GenerateStub<IIdentityUserQuery>(), applicationQuery);
			var result = target.Find(identity, true);
			result.Tenant.Name.Should().Be.EqualTo(pInfo.Tenant.Name);
			result.Id.Should().Be.EqualTo(pInfo.Id);
		}

		[Test]
		public void ShouldChooseIdentityIfIsApplicationLogonFalse()
		{
			var identity = RandomName.Make();

			var pInfo = new PersonInfo();


			var identityUserQuery = MockRepository.GenerateStub<IIdentityUserQuery>();
			identityUserQuery.Stub(x => x.FindUserData(identity)).Return(pInfo);

			var applicationQuery = MockRepository.GenerateStub<IApplicationUserQuery>();
			applicationQuery.Stub(x => x.Find(identity)).Return(new PersonInfo());

			var target = new FindPersonInfoByIdentity(identityUserQuery, applicationQuery);
			var result = target.Find(identity, false);
			result.Tenant.Name.Should().Be.EqualTo(pInfo.Tenant.Name);
			result.Id.Should().Be.EqualTo(pInfo.Id);
		}

		[Test]
		public void ShouldReturnNullIfNotExisting()
		{
			var identity = RandomName.Make();

			var target = new FindPersonInfoByIdentity(MockRepository.GenerateStub<IIdentityUserQuery>(), MockRepository.GenerateStub<IApplicationUserQuery>());
			target.Find(identity, true)
				.Should().Be.Null();
		}
	}
}