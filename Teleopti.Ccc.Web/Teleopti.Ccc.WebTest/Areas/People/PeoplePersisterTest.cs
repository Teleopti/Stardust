using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Ccc.Web.Areas.People.Core;
using Teleopti.Ccc.WebTest.Areas.Search;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.People
{
	[TestFixture]
	public class PeoplePersisterTest
	{
		private PeoplePersister target = new PeoplePersister(new FakeApplicationRoleRepository(), new FakeTenantDataManager(), new FakePersonRepository());


		[Test]
		public void ValidateDataWhenPasswordIsEmpty()
		{
			var rawUserData = new List<RawUser>
			{
				new RawUser
				{
					FirstName = "Jenny",
					ApplicationUserId = "abc",
					LastName = "Morgan",
					Password = "password",
					WindowsUser = "teleopti\\jennym"
				},
				new RawUser
				{
					FirstName = "Jan",
					ApplicationUserId = "abcde",
					LastName = "Morgan",
					Password = "",
					WindowsUser = "teleopti\\janm"
				}
			};
			var errorData = (IEnumerable<RawUser>)((dynamic)target).Persist(rawUserData).InvalidUsers;
			Assert.AreEqual(1, errorData.Count());
			Assert.AreEqual("Jan", errorData.First().FirstName);
			Assert.AreEqual("empty password", errorData.First().ErrorMessage);
		}

		[Test]
		public void ValidateDataWhenWithoutLogonAccount()
		{
			var rawUserData = new List<RawUser>
			{
				new RawUser
				{
					FirstName = "Jenny",
					ApplicationUserId = "abc",
					LastName = "Morgan",
					Password = "password",
					WindowsUser = "teleopti\\jennym"
				},
				new RawUser
				{
					FirstName = "Jan",
					ApplicationUserId = "",
					LastName = "Morgan",
					Password = "psss",
					WindowsUser = ""
				}
			};

			var errorData = (IEnumerable<RawUser>)((dynamic)target).Persist(rawUserData).InvalidUsers;
			Assert.AreEqual(1, errorData.Count());
			Assert.AreEqual("Jan", errorData.First().FirstName);
			Assert.AreEqual("no logon account", errorData.First().ErrorMessage);
		}

		[Test]
		public void ValidateDataWhenFirstNameOrLastNameIsInvalid()
		{
			var rawUserData = new List<RawUser>
			{
				new RawUser
				{
					FirstName = "FirstName0",
					ApplicationUserId = "logon0",
					LastName = "",
					Password = "password",
					WindowsUser = "teleopti\\winlogon0"
				},
				new RawUser
				{
					FirstName = "",
					ApplicationUserId = "logon1",
					LastName = "",
					Password = "password",
					WindowsUser = "teleopti\\winlogon1"
				},
				new RawUser
				{
					FirstName = "ALooooooooooooongFirstName",
					ApplicationUserId = "logon2",
					LastName = "LastName2",
					Password = "password",
					WindowsUser = "teleopti\\winlogon2"
				},
				new RawUser
				{
					FirstName = "FirstName3",
					ApplicationUserId = "logon3",
					LastName = "ALoooooooooooooongLastName",
					Password = "password",
					WindowsUser = "teleopti\\winlogon3"
				}
			};

			var errorData = (IEnumerable<RawUser>)((dynamic)target).Persist(rawUserData).InvalidUsers;
			Assert.AreEqual(3, errorData.Count());

			var firstInvalidPerson = errorData.First();
			Assert.AreEqual("logon1", firstInvalidPerson.ApplicationUserId);
			Assert.AreEqual("both firstname and lastname are empty", firstInvalidPerson.ErrorMessage);

			var secondInvalidPerson = errorData.Second();
			Assert.AreEqual("logon2", secondInvalidPerson.ApplicationUserId);
			Assert.AreEqual("too long firstname", secondInvalidPerson.ErrorMessage);

			var thirdInvalidPerson = errorData.Last();
			Assert.AreEqual("logon3", thirdInvalidPerson.ApplicationUserId);
			Assert.AreEqual("too long lastname", thirdInvalidPerson.ErrorMessage);
		}

		[Test]
		public void ValidateDataWhenNameIsInvalidAndPasswordIsInvalid()
		{
			var rawUserData = new List<RawUser>
			{
				new RawUser
				{
					FirstName = "Jenny",
					ApplicationUserId = "abc",
					LastName = "Morgan",
					Password = "password",
					WindowsUser = "teleopti\\logon0"
				},
				new RawUser
				{
					FirstName = "",
					ApplicationUserId = "",
					LastName = "",
					Password = "",
					WindowsUser = "teleopti\\logon1"
				}
			};

			var errorData = (IEnumerable<RawUser>)((dynamic)target).Persist(rawUserData).InvalidUsers;
			Assert.AreEqual(1, errorData.Count());
			Assert.AreEqual("teleopti\\logon1", errorData.First().WindowsUser);
			Assert.AreEqual("empty password; both firstname and lastname are empty", errorData.First().ErrorMessage);
		}

		[Test]
		public void ValidateDataWhenRoleIsInvalid()
		{
			var rawUserData = new List<RawUser>
			{
				new RawUser
				{
					FirstName = "Jenny",
					ApplicationUserId = "abc",
					LastName = "Morgan",
					Password = "password",
					Role = "Agent",
					WindowsUser = "teleopti\\logon0"
				},
				new RawUser
				{
					FirstName = "firstName1",
					ApplicationUserId = "",
					LastName = "",
					Password = "psss",
					Role = "agent,sss0, sss1",
					WindowsUser = "teleopti\\logon1"
				}
			};

			var roleRepo = new FakeApplicationRoleRepository();
			roleRepo.Add(new ApplicationRole {DescriptionText = "Agent"});

			target = new PeoplePersister(roleRepo, new FakeTenantDataManager(), new FakePersonRepository());
			var errorData = (IEnumerable<RawUser>)((dynamic)target).Persist(rawUserData).InvalidUsers;
			Assert.AreEqual(1, errorData.Count());
			Assert.AreEqual("teleopti\\logon1", errorData.First().WindowsUser);
			Assert.AreEqual("role sss0, sss1 not exist", errorData.First().ErrorMessage);
		}

		[Test]
		public void ValidateDataWhenApplicationUserIdTooLong()
		{
			var rawUserData = new List<RawUser>
			{
				new RawUser
				{
					FirstName = "Jenny",
					ApplicationUserId = "abc",
					LastName = "Morgan",
					Password = "password",
					Role = "",
					WindowsUser = "teleopti\\jennym"
				},
				new RawUser
				{
					FirstName = "Jan",
					ApplicationUserId = "abcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabc",
					LastName = "Morgan",
					Password = "psss",
					Role = "",
					WindowsUser = "teleopti\\jennym"
				}
			};

			var errorData = (IEnumerable<RawUser>)((dynamic)target).Persist(rawUserData).InvalidUsers;
			Assert.AreEqual(1, errorData.Count());
			Assert.AreEqual("Jan", errorData.First().FirstName);
			Assert.AreEqual("too long application user id", errorData.First().ErrorMessage);
		}

		[Test]
		public void ValidateDataWhenHavingDuplicatedLogonAccount()
		{
			var rawUserData = new List<RawUser>
			{
				new RawUser
				{
					FirstName = "Jenny",
					ApplicationUserId = "notExistingId",
					LastName = "Morgan",
					Password = "password",
					Role = "role1",
					WindowsUser = ""
				},
				new RawUser
				{
					FirstName = "Jan",
					ApplicationUserId = "existingId",
					LastName = "Morgan",
					Password = "psss",
					Role = "role2",
					WindowsUser = ""
				}
			};
			var fakeApplicationRoleRepository = new FakeApplicationRoleRepository();
			fakeApplicationRoleRepository.Add(new ApplicationRole{DescriptionText = "role1"});
			fakeApplicationRoleRepository.Add(new ApplicationRole{DescriptionText = "role2"});
			var fakeTenantDataManager = new FakeTenantDataManager();
			var person1 = new Person { Name = new Name("Jenny", "Morgan") };
			person1.SetId(Guid.NewGuid());
			var person2 = new Person { Name = new Name("Jan", "Morgan") };
			person2.SetId(Guid.NewGuid());
			var fakePersonRepository = new FakePersonRepository(new []{person1, person2});
			target = new PeoplePersister(fakeApplicationRoleRepository, fakeTenantDataManager, fakePersonRepository);
			var errorData = (IEnumerable<RawUser>)((dynamic)target).Persist(rawUserData).InvalidUsers;
			Assert.AreEqual(1, errorData.Count());
			Assert.AreEqual("Jan", errorData.First().FirstName);
			Assert.AreEqual("dupicated application user id", errorData.First().ErrorMessage);
		}
	}

	public class FakeTenantDataManager : ITenantDataManager
	{
		public SavePersonInfoResult SaveTenantData(TenantAuthenticationData tenantAuthenticationData)
		{
			if (tenantAuthenticationData.ApplicationLogonName == "existingId")
			{
				return new SavePersonInfoResult
				{
					Success = false,
					FailReason = "dupicated application user id"
				};
			}
			return new SavePersonInfoResult()
			{
				Success = true
			};
		}

		public void DeleteTenantPersons(IEnumerable<Guid> personsToBeDeleted)
		{
			throw new NotImplementedException();
		}
	}
}
