using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
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
		private PeoplePersister target = new PeoplePersister(new PersistPersonInfoFake(), new PersonInfoMapperFake(), new FakeApplicationRoleRepository(), new FakePersonRepository(), new FakeLoggedOnUser(), new UserValidator());


		[Test]
		public void ValidateDataWhenPasswordIsEmpty()
		{
			var rawUserData = new List<RawUser>
			{
				new RawUser
				{
					Firstname = "Jenny",
					ApplicationUserId = "abc",
					Lastname = "Morgan",
					Password = "password",
					WindowsUser = "teleopti\\jennym"
				},
				new RawUser
				{
					Firstname = "Jan",
					ApplicationUserId = "abcde",
					Lastname = "Morgan",
					Password = "",
					WindowsUser = "teleopti\\janm"
				}
			};
			var errorData = target.Persist(rawUserData).ToList();
			Assert.AreEqual(1, errorData.Count());
			Assert.AreEqual("Jan", errorData.First().Firstname);
			Assert.AreEqual("empty password", errorData.First().ErrorMessage);
		}
		[Test]
		public void ValidateDataWhenApplicationLogonIsEmpty()
		{
			var rawUserData = new List<RawUser>
			{
				new RawUser
				{
					Firstname = "Jenny",
					ApplicationUserId = "",
					Lastname = "Morgan",
					Password = "password",
					WindowsUser = "teleopti\\jennym"
				},
				new RawUser
				{
					Firstname = "Jan",
					ApplicationUserId = "abcde",
					Lastname = "Morgan",
					Password = "password",
					WindowsUser = "teleopti\\janm"
				}
			};
			var errorData = target.Persist(rawUserData).ToList();
			Assert.AreEqual(1, errorData.Count());
			Assert.AreEqual("Jenny", errorData.First().Firstname);
			Assert.AreEqual("no application user id", errorData.First().ErrorMessage);
		}

		[Test]
		public void ValidateDataWhenWithoutLogonAccount()
		{
			var rawUserData = new List<RawUser>
			{
				new RawUser
				{
					Firstname = "Jenny",
					ApplicationUserId = "abc",
					Lastname = "Morgan",
					Password = "password",
					WindowsUser = "teleopti\\jennym"
				},
				new RawUser
				{
					Firstname = "Jan",
					ApplicationUserId = "",
					Lastname = "Morgan",
					Password = "psss",
					WindowsUser = ""
				}
			};

			var errorData = target.Persist(rawUserData).ToList();
			Assert.AreEqual(1, errorData.Count());
			Assert.AreEqual("Jan", errorData.First().Firstname);
			Assert.AreEqual("no logon account", errorData.First().ErrorMessage);
		}

		[Test]
		public void ValidateDataWhenFirstnameOrLastnameIsInvalid()
		{
			var rawUserData = new List<RawUser>
			{
				new RawUser
				{
					Firstname = "Firstname0",
					ApplicationUserId = "logon0",
					Lastname = "",
					Password = "password",
					WindowsUser = "teleopti\\winlogon0"
				},
				new RawUser
				{
					Firstname = "",
					ApplicationUserId = "logon1",
					Lastname = "",
					Password = "password",
					WindowsUser = "teleopti\\winlogon1"
				},
				new RawUser
				{
					Firstname = "ALooooooooooooongFirstname",
					ApplicationUserId = "logon2",
					Lastname = "Lastname2",
					Password = "password",
					WindowsUser = "teleopti\\winlogon2"
				},
				new RawUser
				{
					Firstname = "Firstname3",
					ApplicationUserId = "logon3",
					Lastname = "ALoooooooooooooongLastname",
					Password = "password",
					WindowsUser = "teleopti\\winlogon3"
				}
			};

			var errorData = target.Persist(rawUserData).ToList();
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
					Firstname = "Jenny",
					ApplicationUserId = "abc",
					Lastname = "Morgan",
					Password = "password",
					WindowsUser = "teleopti\\logon0"
				},
				new RawUser
				{
					Firstname = "",
					ApplicationUserId = "app1",
					Lastname = "",
					Password = "",
					WindowsUser = "teleopti\\logon1"
				}
			};

			var errorData = target.Persist(rawUserData).ToList();
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
					Firstname = "Jenny",
					ApplicationUserId = "abc",
					Lastname = "Morgan",
					Password = "password",
					Role = "Agent",
					WindowsUser = "teleopti\\logon0"
				},
				new RawUser
				{
					Firstname = "Firstname1",
					ApplicationUserId = "app1",
					Lastname = "",
					Password = "psss",
					Role = "agent,sss0, \"test,sss1\"",
					WindowsUser = "teleopti\\logon1"
				}
			};

			var roleRepo = new FakeApplicationRoleRepository();
			roleRepo.Add(new ApplicationRole {DescriptionText = "Agent"});

			target = new PeoplePersister(new PersistPersonInfoFake(), new PersonInfoMapperFake(), roleRepo, new FakePersonRepository(), new FakeLoggedOnUser(), new UserValidator());
			var errorData = target.Persist(rawUserData).ToList();
			Assert.AreEqual(1, errorData.Count());
			Assert.AreEqual("teleopti\\logon1", errorData.First().WindowsUser);
			Assert.AreEqual("role sss0, \"test,sss1\" not exist", errorData.First().ErrorMessage);
		}

		[Test]
		public void ValidateDataWhenApplicationUserIdTooLong()
		{
			var rawUserData = new List<RawUser>
			{
				new RawUser
				{
					Firstname = "Jenny",
					ApplicationUserId = "abc",
					Lastname = "Morgan",
					Password = "password",
					Role = "",
					WindowsUser = "teleopti\\jennym"
				},
				new RawUser
				{
					Firstname = "Jan",
					ApplicationUserId = "abcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabc",
					Lastname = "Morgan",
					Password = "psss",
					Role = "",
					WindowsUser = "teleopti\\jennym"
				}
			};

			var errorData = target.Persist(rawUserData).ToList();
			Assert.AreEqual(1, errorData.Count());
			Assert.AreEqual("Jan", errorData.First().Firstname);
			Assert.AreEqual("too long application user id", errorData.First().ErrorMessage);
		}

		[Test]
		public void ValidateDataWhenHavingDuplicatedLogonAccount()
		{
			var rawUserData = new List<RawUser>
			{
				new RawUser
				{
					Firstname = "Jenny",
					ApplicationUserId = "notExistingId",
					Lastname = "Morgan",
					Password = "password",
					Role = "role1",
					WindowsUser = ""
				},
				new RawUser
				{
					Firstname = "Jan",
					ApplicationUserId = "existingId",
					Lastname = "Morgan",
					Password = "psss",
					Role = "role2",
					WindowsUser = ""
				}
			};
			var fakeApplicationRoleRepository = new FakeApplicationRoleRepository();
			fakeApplicationRoleRepository.Add(new ApplicationRole{DescriptionText = "role1"});
			fakeApplicationRoleRepository.Add(new ApplicationRole{DescriptionText = "role2"});
			var personInfoPersister = new PersistPersonInfoFake();
			var personInfoMapper = new PersonInfoMapperFake();
			var person1 = new Person { Name = new Name("Jenny", "Morgan") };
			person1.SetId(Guid.NewGuid());
			var person2 = new Person { Name = new Name("Jan", "Morgan") };
			person2.SetId(Guid.NewGuid());
			var fakePersonRepository = new FakePersonRepository(new []{person1, person2});
			target = new PeoplePersister(personInfoPersister, personInfoMapper, fakeApplicationRoleRepository, fakePersonRepository, new FakeLoggedOnUser(), new UserValidator());
			var errorData = target.Persist(rawUserData).ToList();
			Assert.AreEqual(1, errorData.Count());
			Assert.AreEqual("Jan", errorData.First().Firstname);
			Assert.AreEqual("duplicated application user id", errorData.First().ErrorMessage);
		}

		[Test]
		public void ValidateDataWhenHavingDuplicatedLogonAccountAndNoRole()
		{
			var rawUserData = new List<RawUser>
			{
				new RawUser
				{
					Firstname = "Jenny",
					ApplicationUserId = "notExistingId",
					Lastname = "Morgan",
					Password = "password",
					Role = "",
					WindowsUser = ""
				},
				new RawUser
				{
					Firstname = "Jan",
					ApplicationUserId = "existingId",
					Lastname = "Morgan",
					Password = "psss",
					Role = "",
					WindowsUser = ""
				}
			};
			var fakeApplicationRoleRepository = new FakeApplicationRoleRepository();
			fakeApplicationRoleRepository.Add(new ApplicationRole{DescriptionText = "role1"});
			fakeApplicationRoleRepository.Add(new ApplicationRole{DescriptionText = "role2"});
			var personInfoPersister = new PersistPersonInfoFake();
			var personInfoMapper = new PersonInfoMapperFake();
			var person1 = new Person { Name = new Name("Jenny", "Morgan") };
			person1.SetId(Guid.NewGuid());
			var person2 = new Person { Name = new Name("Jan", "Morgan") };
			person2.SetId(Guid.NewGuid());
			var fakePersonRepository = new FakePersonRepository(new []{person1, person2});
			target = new PeoplePersister(personInfoPersister,personInfoMapper,fakeApplicationRoleRepository, fakePersonRepository, new FakeLoggedOnUser(), new UserValidator());
			var errorData = target.Persist(rawUserData).ToList();
			Assert.AreEqual(1, errorData.Count());
			Assert.AreEqual("Jan", errorData.First().Firstname);
			Assert.AreEqual("duplicated application user id", errorData.First().ErrorMessage);
		}
	}

	public class PersonInfoMapperFake:IPersonInfoMapper
	{
		public PersonInfo Map(PersonInfoModel personInfoModel)
		{
			var personInfo = new PersonInfo(new Tenant("Test"), personInfoModel.PersonId);
			personInfo.SetIdentity(personInfoModel.Identity);
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), personInfoModel.ApplicationLogonName,personInfoModel.Password);
			return personInfo;
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
