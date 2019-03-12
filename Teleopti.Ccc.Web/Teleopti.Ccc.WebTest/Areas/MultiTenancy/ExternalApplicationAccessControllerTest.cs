using System;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.Web.Areas.MultiTenancy;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy
{
	[TenantTest]
	public class ExternalApplicationAccessControllerTest
	{
		public ExternalApplicationAccessController Target;
		public FakePersistExternalApplicationAccess Persister;
		public FakeFindExternalApplicationAccess Finder;
		public FindPersonInfoFake PersonInfoFinder;
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
			var personInfo = new PersonInfo(new Tenant("asdf"), personId);
			personInfo.RegenerateTenantPassword();

			PersonInfoFinder.Add(personInfo);
			CurrentTenantUser.Set(personInfo);

			var model = new NewExternalApplicationModel { Name = "HR system" };
			var addResult = (OkNegotiatedContentResult<NewExternalApplicationResponseModel>)Target.Add(model);

			var result = (OkNegotiatedContentResult<VerifiedExternalApplicationAccessToken>)Target.Verify(addResult.Content.Token);
			result.Content.PersonId.Should().Be.EqualTo(personId);
			result.Content.Tenant.Should().Be.EqualTo("asdf");
			result.Content.TenantPassword.Should().Be.EqualTo(personInfo.TenantPassword);
		}

		[Test]
		public void ShouldReturnEmptyResponseWhenTokenNotFound()
		{
			var personId = Guid.NewGuid();
			var personInfo = new PersonInfo(new Tenant("asdf"), personId);
			personInfo.RegenerateTenantPassword();

			PersonInfoFinder.Add(personInfo);
			CurrentTenantUser.Set(personInfo);

			var result = (OkResult)Target.Verify("asdf");
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnEmptyResponseWhenPersonInfoNotFound()
		{
			var personId = Guid.NewGuid();
			var personInfo = new PersonInfo(new Tenant("asdf"), personId);
			personInfo.RegenerateTenantPassword();

			CurrentTenantUser.Set(personInfo);

			var model = new NewExternalApplicationModel { Name = "HR system" };
			var addResult = (OkNegotiatedContentResult<NewExternalApplicationResponseModel>)Target.Add(model);

			var result = (OkResult)Target.Verify(addResult.Content.Token);
			
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldGetListOfExternalApplications()
		{
			var personId = Guid.NewGuid();
			CurrentTenantUser.Set(new PersonInfo(new Tenant("asdf"), personId));
			var model = new NewExternalApplicationModel { Name = "HR system" };
			Target.Add(model);

			var result = (OkNegotiatedContentResult<ExternalApplicationModel[]>)Target.Get();
			result.Content.Single().Name.Should().Be.EqualTo(model.Name);
		}

		[Test]
		public void ShouldRemoveExternalApplication()
		{
			var personId = Guid.NewGuid();
			CurrentTenantUser.Set(new PersonInfo(new Tenant("asdf"), personId));
			var model = new NewExternalApplicationModel { Name = "HR system" };
			Target.Add(model);

			var items = Finder.FindByPerson(personId);
			
			Target.Remove(items.Single().Id);
			Finder.FindByPerson(personId).Should().Be.Empty();
		}
	}
}