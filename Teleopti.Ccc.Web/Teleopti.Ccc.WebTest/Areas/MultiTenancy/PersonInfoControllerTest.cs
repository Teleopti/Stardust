using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MultiTenancy;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy
{
	[TenantTest]
	public class PersonInfoControllerTest
	{
		public PersonInfoController Target;
		public FindLogonInfoFake FindLogonInfo;
		public TenantAuthenticationFake TenantAuthentication;
		public TenantUnitOfWorkFake TenantUnitOfWork;

		[Test]
		public void ShouldGetLogonInfoFromLogonName()
		{
			var logonInfo1 = new LogonInfo{PersonId = Guid.NewGuid(), LogonName = "test1"};
			FindLogonInfo.Add(logonInfo1);

			Target.LogonInfoFromLogonName(logonInfo1.LogonName).Result<LogonInfo>().Should().Be.EqualTo(logonInfo1);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}

		[Test]
		public void ShouldGetLogonInfoFromIdentity()
		{
			var logonInfo1 = new LogonInfo { PersonId = Guid.NewGuid(), Identity = "identity1"};
			FindLogonInfo.Add(logonInfo1);

			Target.LogonInfoFromIdentity(logonInfo1.Identity).Result<LogonInfo>().Should().Be.EqualTo(logonInfo1);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}
	}

	[TenantTest]
	public class PersonInfoControllerExtendedTest : ISetup
	{
		public PersonInfoController Target;
		public FindLogonInfoFake FindLogonInfo;
		public TenantAuthenticationFake TenantAuthentication;
		public TenantUnitOfWorkFake TenantUnitOfWork;
		public PersonInfoPersisterFake PersonInfoPersisterFake;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<PersistPersonInfo>().For<IPersistPersonInfo>();
			system.UseTestDouble<PersonInfoPersisterFake>().For<IPersonInfoPersister>();
		}

		[Test]
		public void ShouldPersistApplicationLogonNames()
		{
			var p1 = PersonFactory.CreatePersonWithGuid("Kalle", "Anka");
			var p2 = PersonFactory.CreatePersonWithGuid("Joakim", "Anka");

			var inputModel = new PersonApplicationLogonInputModel
			{
				People = new List<PersonApplicationLogonModel>
				{
					new PersonApplicationLogonModel {PersonId = p1.Id.GetValueOrDefault(), ApplicationLogonName = "aaa1"},
					new PersonApplicationLogonModel {PersonId = p2.Id.GetValueOrDefault(), ApplicationLogonName = "aaa2"}
				}
			};
			var result = Target.PersistApplicationLogonNames(inputModel);
			result.Should().Be.OfType<OkResult>();
			PersonInfoPersisterFake.PersistedData.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldFailWhenTryingToSaveAlreadyExistingAppLogonNames()
		{
			var p1 = PersonFactory.CreatePersonWithGuid("Kalle", "Anka");
			var p2 = PersonFactory.CreatePersonWithGuid("Joakim", "Anka");
			var p3 = PersonFactory.CreatePersonWithGuid("Knatte", "Anka");
			var p4 = PersonFactory.CreatePersonWithGuid("Fnatte", "Anka");

			var inputModel = new PersonApplicationLogonInputModel
			{
				People = new List<PersonApplicationLogonModel>
				{
					new PersonApplicationLogonModel {PersonId = p1.Id.GetValueOrDefault(), ApplicationLogonName = "aaa1"},
					new PersonApplicationLogonModel {PersonId = p2.Id.GetValueOrDefault(), ApplicationLogonName = "aaa2"},
					new PersonApplicationLogonModel {PersonId = p3.Id.GetValueOrDefault(), ApplicationLogonName = "aaa3"}
				}
			};
			var result = Target.PersistApplicationLogonNames(inputModel);
			result.Should().Be.OfType<OkResult>();
			PersonInfoPersisterFake.PersistedData.Count.Should().Be.EqualTo(3);

			var inputModelRoundTwo = new PersonApplicationLogonInputModel
			{
				People = new List<PersonApplicationLogonModel>
				{
					new PersonApplicationLogonModel {PersonId = p4.Id.GetValueOrDefault(), ApplicationLogonName = "aaa1"}, // Should Fail, same as p1
					new PersonApplicationLogonModel {PersonId = p3.Id.GetValueOrDefault(), ApplicationLogonName = "aaa3"}, // Should Succeed, same as before
					new PersonApplicationLogonModel {PersonId = p2.Id.GetValueOrDefault(), ApplicationLogonName = "aaa3"}, // Should Fail, same as p3
					new PersonApplicationLogonModel {PersonId = p1.Id.GetValueOrDefault(), ApplicationLogonName = "aaa4"}  // Should Succeed
				}
			};
			var result2 = Target.PersistApplicationLogonNames(inputModelRoundTwo);
			PersonInfoPersisterFake.PersistedData.Count.Should().Be.EqualTo(4);
			var contentResult = result2 as NegotiatedContentResult<PersonInfoGenericResultModel>;
			contentResult.StatusCode.Should().Be.EqualTo(HttpStatusCode.BadRequest);
			contentResult.Content.ResultList.Count.Should().Be.EqualTo(2);
		}
	}
}