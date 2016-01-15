using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Ccc.Web.Areas.People.Core.Persisters;
using Teleopti.Ccc.WebTest.Areas.Search;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.People
{
	[TestFixture]
	public class PeoplePersisterTest
	{
		private PeoplePersister target;

		[SetUp]
		public void Setup()
		{
			var fakeApplicationRoleRepository = new FakeApplicationRoleRepository();
			fakeApplicationRoleRepository.Add(new ApplicationRole
			{
				DescriptionText = "role1"
			});
			fakeApplicationRoleRepository.Add(new ApplicationRole
			{
				DescriptionText = "role2"
			});

			var personInfoPersister = new PersistPersonInfoFake();
			var personInfoMapper = new PersonInfoMapperFake();

			var person1 = new Person
			{
				Name = new Name("Jenny", "Morgan")
			};
			person1.SetId(Guid.NewGuid());

			var person2 = new Person
			{
				Name = new Name("Jan", "Morgan")
			};
			person2.SetId(Guid.NewGuid());

			var fakePersonRepository = new FakePersonRepositoryLegacy(person1, person2);

			target = new PeoplePersister(personInfoPersister, personInfoMapper, fakeApplicationRoleRepository,
				fakePersonRepository, new FakeLoggedOnUser(), new UserValidator());
		}

		[Test]
		public void ShouldNotPersistUserWithDuplicatedLogonAccount()
		{
			var rawUserData = new List<RawUser>
			{
				new RawUser
				{
					Firstname = "Jenny",
					ApplicationUserId = "notExistingId@teleopti.com",
					Lastname = "Morgan",
					Password = "password",
					Role = "role1",
					WindowsUser = ""
				},
				new RawUser
				{
					Firstname = "Jan",
					ApplicationUserId = "existingId@teleopti.com",
					Lastname = "Morgan",
					Password = "psss",
					Role = "role2",
					WindowsUser = ""
				}
			};
			var errorData = target.Persist(rawUserData).ToList();
			Assert.AreEqual(1, errorData.Count);
			Assert.AreEqual("Jan", errorData.First().Firstname);
			Assert.AreEqual("duplicated application user id", errorData.First().ErrorMessage);
		}

		[Test]
		public void ShouldNotPersistUserWithDuplicatedLogonAccountAndNoRole()
		{
			var rawUserData = new List<RawUser>
			{
				new RawUser
				{
					Firstname = "Jenny",
					ApplicationUserId = "notExistingId@teleopti.com",
					Lastname = "Morgan",
					Password = "password",
					Role = "",
					WindowsUser = ""
				},
				new RawUser
				{
					Firstname = "Jan",
					ApplicationUserId = "existingId@teleopti.com",
					Lastname = "Morgan",
					Password = "psss",
					Role = "",
					WindowsUser = ""
				}
			};

			var errorData = target.Persist(rawUserData).ToList();
			Assert.AreEqual(1, errorData.Count);
			Assert.AreEqual("Jan", errorData.First().Firstname);
			Assert.AreEqual("duplicated application user id", errorData.First().ErrorMessage);
		}
	}

	public class PersonInfoMapperFake : IPersonInfoMapper
	{
		public PersonInfo Map(PersonInfoModel personInfoModel)
		{
			var personInfo = new PersonInfo(new Tenant("Test"), personInfoModel.PersonId);
			personInfo.SetIdentity(personInfoModel.Identity);
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), personInfoModel.ApplicationLogonName,
				personInfoModel.Password);
			return personInfo;
		}
	}

	public class FakeTenantDataManager : ITenantDataManager
	{
		public SavePersonInfoResult SaveTenantData(TenantAuthenticationData tenantAuthenticationData)
		{
			var appLogonExists = tenantAuthenticationData.ApplicationLogonName == "existingId@teleopti.com";
			return new SavePersonInfoResult
			{
				Success = !appLogonExists,
				FailReason = appLogonExists ? "dupicated application user id" : null
			};
		}

		public void DeleteTenantPersons(IEnumerable<Guid> personsToBeDeleted)
		{
			throw new NotImplementedException();
		}
	}
}
