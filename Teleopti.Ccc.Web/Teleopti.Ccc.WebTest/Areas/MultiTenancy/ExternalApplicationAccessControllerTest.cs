using System;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.Web.Areas.MultiTenancy;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy
{
	[TenantTest]
	public class ExternalApplicationAccessControllerTest
	{
		public ExternalApplicationAccessController Target;
		public FakePersistExternalApplicationAccess Persister;
		public CurrentTenantUserFake CurrentTenantUser;

		[Test]
		public void ShouldReturnATokenForNewExternalApplication()
		{
			var personId = Guid.NewGuid();
			CurrentTenantUser.Set(new PersonInfo(new Tenant("asdf"),personId));
			var model = new NewExternalApplicationModel {Name = "HR system"};
			var result = (OkNegotiatedContentResult<NewExternalApplicationResponseModel>)Target.Add(model);

			Persister.Storage.Single().Name.Should().Be.EqualTo(model.Name);
			Persister.Storage.Single().PersonId.Should().Be.EqualTo(personId);
			result.Content.Token.Should().Not.Be.NullOrEmpty();
		}

		[Test]
		public void ShouldGetToPersonIdFromToken()
		{
			var personId = Guid.NewGuid();
			CurrentTenantUser.Set(new PersonInfo(new Tenant("asdf"), personId));
			var model = new NewExternalApplicationModel { Name = "HR system" };
			var addResult = (OkNegotiatedContentResult<NewExternalApplicationResponseModel>)Target.Add(model);

			var result = (OkNegotiatedContentResult<Guid>)Target.Verify(addResult.Content.Token);
			result.Content.Should().Be.EqualTo(personId);
		}
	}
}
