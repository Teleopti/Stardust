using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
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
		private string createLongString(int length, char fillWith = 'A')
		{
			return new string(fillWith, length);
		}

		private PeoplePersister target = new PeoplePersister(new PersistPersonInfoFake(), new PersonInfoMapperFake(),
			new FakeApplicationRoleRepository(), new FakePersonRepository(), new FakeLoggedOnUser(), new UserValidator());

		[Test]
		public void ValidateDataWhenPasswordIsEmpty()
		{
			var rawUserData = new List<RawUser>
			{
				new RawUser
				{
					Firstname = "Jenny",
					ApplicationUserId = "abc@teleopti.com",
					Lastname = "Morgan",
					Password = "password",
					WindowsUser = ""
				},
				new RawUser
				{
					Firstname = "Jan",
					ApplicationUserId = "abcde@teleopti.com",
					Lastname = "Morgan",
					Password = "",
					WindowsUser = ""
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
					WindowsUser = "jennym@teleopti.com"
				},
				new RawUser
				{
					Firstname = "Jan",
					ApplicationUserId = "abcde@teleopti.com",
					Lastname = "Morgan",
					Password = "password",
					WindowsUser = "janm@teleopti.com"
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
					ApplicationUserId = "abc@teleopti.com",
					Lastname = "Morgan",
					Password = "password",
					WindowsUser = "jennym@teleopti.com"
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
					ApplicationUserId = "logon0@teleopti.com",
					Lastname = "",
					Password = "password",
					WindowsUser = "winlogon0@teleopti.com"
				},
				new RawUser
				{
					Firstname = "",
					ApplicationUserId = "logon1@teleopti.com",
					Lastname = "",
					Password = "password",
					WindowsUser = "winlogon1@teleopti.com"
				},
				new RawUser
				{
					Firstname = createLongString(26),
					ApplicationUserId = "logon2@teleopti.com",
					Lastname = "Lastname2",
					Password = "password",
					WindowsUser = "winlogon2@teleopti.com"
				},
				new RawUser
				{
					Firstname = "Firstname3",
					ApplicationUserId = "logon3@teleopti.com",
					Lastname = createLongString(26),
					Password = "password",
					WindowsUser = "winlogon3@teleopti.com"
				}
			};

			var errorData = target.Persist(rawUserData).ToList();
			Assert.AreEqual(3, errorData.Count());

			var firstInvalidPerson = errorData.First();
			Assert.AreEqual("logon1@teleopti.com", firstInvalidPerson.ApplicationUserId);
			Assert.AreEqual("both firstname and lastname are empty", firstInvalidPerson.ErrorMessage);

			var secondInvalidPerson = errorData.Second();
			Assert.AreEqual("logon2@teleopti.com", secondInvalidPerson.ApplicationUserId);
			Assert.AreEqual("too long firstname", secondInvalidPerson.ErrorMessage);

			var thirdInvalidPerson = errorData.Last();
			Assert.AreEqual("logon3@teleopti.com", thirdInvalidPerson.ApplicationUserId);
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
					ApplicationUserId = "abc@teleopti.com",
					Lastname = "Morgan",
					Password = "password",
					WindowsUser = "logon0@teleopti.com"
				},
				new RawUser
				{
					Firstname = "",
					ApplicationUserId = "app1@teleopti.com",
					Lastname = "",
					Password = "",
					WindowsUser = "logon1@teleopti.com"
				}
			};

			var errorData = target.Persist(rawUserData).ToList();
			Assert.AreEqual(1, errorData.Count());
			Assert.AreEqual("logon1@teleopti.com", errorData.First().WindowsUser);
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
					ApplicationUserId = "abc@teleopti.com",
					Lastname = "Morgan",
					Password = "password",
					Role = "Agent",
					WindowsUser = "logon0@teleopti.com"
				},
				new RawUser
				{
					Firstname = "Firstname1",
					ApplicationUserId = "app1@teleopti.com",
					Lastname = "",
					Password = "psss",
					Role = "agent,sss0, \"test,sss1\"",
					WindowsUser = "logon1@teleopti.com"
				}
			};

			var roleRepo = new FakeApplicationRoleRepository();
			roleRepo.Add(new ApplicationRole {DescriptionText = "Agent"});

			target = new PeoplePersister(new PersistPersonInfoFake(), new PersonInfoMapperFake(), roleRepo, new FakePersonRepository(), new FakeLoggedOnUser(), new UserValidator());
			var errorData = target.Persist(rawUserData).ToList();
			Assert.AreEqual(1, errorData.Count());
			Assert.AreEqual("logon1@teleopti.com", errorData.First().WindowsUser);
			Assert.AreEqual("role sss0, \"test,sss1\" not exist", errorData.First().ErrorMessage);
		}

		[Test]
		public void ValidateDataWhenApplicationUserIdTooLong()
		{
			const string emailSuffix = "@teleopti.com";
			var rawUserData = new List<RawUser>
			{
				new RawUser
				{
					Firstname = "Jenny",
					ApplicationUserId = "abc@teleopti.com",
					Lastname = "Morgan",
					Password = "password",
					Role = "",
					WindowsUser = "jennym@teleopti.com"
				},
				new RawUser
				{
					Firstname = "Jan",
					ApplicationUserId = createLongString(51 - emailSuffix.Length) + emailSuffix,
					Lastname = "Morgan",
					Password = "psss",
					Role = "",
					WindowsUser = "jennym@teleopti.com"
				}
			};

			var errorData = target.Persist(rawUserData).ToList();
			Assert.AreEqual(1, errorData.Count());
			Assert.AreEqual("Jan", errorData.First().Firstname);
			Assert.AreEqual("too long application user id", errorData.First().ErrorMessage);
		}

		[Test]
		public void ValidateDataWhenWindowsUserIdIsTooLong()
		{
			var rawUserData = new List<RawUser>
			{
				new RawUser
				{
					Firstname = "Jenny",
					ApplicationUserId = "abc@teleopti.com",
					Lastname = "Morgan",
					Password = "password",
					Role = "",
					WindowsUser = createLongString(101)
				},
				new RawUser
				{
					Firstname = "Jan",
					ApplicationUserId = "abca@teleopti.com",
					Lastname = "Morgan",
					Password = "psss",
					Role = "",
					WindowsUser = "jennym"
				}
			};

			var errorData = target.Persist(rawUserData).ToList();
			Assert.AreEqual(1, errorData.Count());
			Assert.AreEqual("Jenny", errorData.First().Firstname);
			Assert.AreEqual("too long windows user", errorData.First().ErrorMessage);
		}

		[Test]
		public void ValidateDataWhenApplicationUserIdNotValidEmailAddr()
		{
			var rawUserData = new List<RawUser>
			{
				new RawUser
				{
					Firstname = "Jenny",
					ApplicationUserId = "abc@teleopti.com",
					Lastname = "Morgan",
					Password = "password",
					Role = "",
					WindowsUser = "jennym@teleopti.com"
				},
				new RawUser
				{
					Firstname = "Jan",
					ApplicationUserId = "abca",
					Lastname = "Morgan",
					Password = "psss",
					Role = "",
					WindowsUser = "jennym@teleopti.com"
				}
			};

			var errorData = target.Persist(rawUserData).ToList();
			Assert.AreEqual(1, errorData.Count());
			Assert.AreEqual("Jan", errorData.First().Firstname);
			Assert.AreEqual("ApplicationUserId should be a valid email address", errorData.First().ErrorMessage);
		}

		[Test]
		public void ValidateDataWhenHavingDuplicatedLogonAccount()
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
			var fakeApplicationRoleRepository = new FakeApplicationRoleRepository();
			fakeApplicationRoleRepository.Add(new ApplicationRole{DescriptionText = "role1"});
			fakeApplicationRoleRepository.Add(new ApplicationRole{DescriptionText = "role2"});
			var personInfoPersister = new PersistPersonInfoFake();
			var personInfoMapper = new PersonInfoMapperFake();
			var person1 = new Person { Name = new Name("Jenny", "Morgan") };
			person1.SetId(Guid.NewGuid());
			var person2 = new Person { Name = new Name("Jan", "Morgan") };
			person2.SetId(Guid.NewGuid());
			var fakePersonRepository = new FakePersonRepository(person1, person2);
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
			var fakeApplicationRoleRepository = new FakeApplicationRoleRepository();
			fakeApplicationRoleRepository.Add(new ApplicationRole{DescriptionText = "role1"});
			fakeApplicationRoleRepository.Add(new ApplicationRole{DescriptionText = "role2"});
			var personInfoPersister = new PersistPersonInfoFake();
			var personInfoMapper = new PersonInfoMapperFake();
			var person1 = new Person { Name = new Name("Jenny", "Morgan") };
			person1.SetId(Guid.NewGuid());
			var person2 = new Person { Name = new Name("Jan", "Morgan") };
			person2.SetId(Guid.NewGuid());
			var fakePersonRepository = new FakePersonRepository(person1, person2);
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
			if (tenantAuthenticationData.ApplicationLogonName == "existingId@teleopti.com")
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
