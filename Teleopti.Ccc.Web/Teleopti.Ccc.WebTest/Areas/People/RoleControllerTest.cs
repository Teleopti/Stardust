using NUnit.Framework;
using SharpTestsEx;
using System.Linq;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Ccc.Web.Areas.People.Models;
using Teleopti.Ccc.WebTest.Areas.People.IoC;

namespace Teleopti.Ccc.WebTest.Areas.People
{
	[WebPeopleTest]
	public class RoleControllerTest
	{
		public RoleController Target;
		public FakeApplicationRoleRepository ApplicationRoleRepository;
		public FakePersonRepository PersonRepository;
		private IPerson _person1;
		private IPerson _person2;
		private IApplicationRole _role1;
		private IApplicationRole _role2;

		[SetUp]
		public void SetUp()
		{
			_person1 = PersonFactory.CreatePerson(new Name("Person1F", "Person1L")).WithId();
			_person2 = PersonFactory.CreatePerson(new Name("Person2F", "Person2L")).WithId();
			_role1 = new ApplicationRole().WithId();
			_role2 = new ApplicationRole().WithId();
		}

		[Test]
		public void FetchRolesShouldGetAllAvailableRoles()
		{
			var result = Target.FetchRoles();
			result.Count().Should().Be.EqualTo(0);
			ApplicationRoleRepository.Add(new ApplicationRole());
			result = Target.FetchRoles();

			result.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void FetchPersonsShouldFetchZeroPersonsOnEmptyCall()
		{
			var result = Target.FetchPersons(new FecthPersonsInputModel());

			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void FetchPersonsShouldReturnPersons()
		{
			/// Setup
			PersonRepository.Add(_person1);
			PersonRepository.Add(_person2);
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("Person3F", "Person3L")).WithId());

			/// Test
			var input = new FecthPersonsInputModel()
			{
				PersonIdList = new [] { _person1.Id.GetValueOrDefault(), _person2.Id.GetValueOrDefault() }.ToList()
			};
			
			var result = Target.FetchPersons(input);

			result.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void GrantOneRoleToOneUser()
		{
			/// Setup
			PersonRepository.Add(_person1);
			ApplicationRoleRepository.Add(_role1);

			/// Test
			var result = Target.GrantRoles(new GrantRolesInputModel() { Persons = new [] { _person1.Id.GetValueOrDefault() }, Roles = new [] { _role1.Id.GetValueOrDefault()} } );
			result.Should().Be.OfType<OkResult>();

			var newPerson = PersonRepository.Get(_person1.Id.GetValueOrDefault());
			newPerson.PermissionInformation.ApplicationRoleCollection.Should().Contain(_role1);
			newPerson.PermissionInformation.ApplicationRoleCollection.Count().Should().Be.EqualTo(1);

			// Calling GrantRoles a second time will not make any difference or create any errors.
			var result2 = Target.GrantRoles(new GrantRolesInputModel() { Persons = new[] { _person1.Id.GetValueOrDefault() }, Roles = new[] { _role1.Id.GetValueOrDefault() } });
			result2.Should().Be.OfType<OkResult>();

			newPerson = PersonRepository.Get(_person1.Id.GetValueOrDefault());
			newPerson.PermissionInformation.ApplicationRoleCollection.Should().Contain(_role1);
			newPerson.PermissionInformation.ApplicationRoleCollection.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void GrantMultipleRolesToMultipleUsers()
		{
			/// Setup
			PersonRepository.Add(_person1);
			PersonRepository.Add(_person2);
			ApplicationRoleRepository.Add(_role1);
			ApplicationRoleRepository.Add(_role2);

			/// Test
			var result = Target.GrantRoles(new GrantRolesInputModel()
			{
				Persons = new[] { _person1.Id.GetValueOrDefault(), _person2.Id.GetValueOrDefault() },
				Roles = new[] { _role1.Id.GetValueOrDefault(), _role2.Id.GetValueOrDefault() }
			});
			result.Should().Be.OfType<OkResult>();

			var newPerson1 = PersonRepository.Get(_person1.Id.GetValueOrDefault());
			newPerson1.PermissionInformation.ApplicationRoleCollection.Should().Contain(_role1);
			newPerson1.PermissionInformation.ApplicationRoleCollection.Should().Contain(_role2);
			newPerson1.PermissionInformation.ApplicationRoleCollection.Count().Should().Be.EqualTo(2);

			var newPerson2 = PersonRepository.Get(_person2.Id.GetValueOrDefault());
			newPerson2.PermissionInformation.ApplicationRoleCollection.Should().Contain(_role1);
			newPerson2.PermissionInformation.ApplicationRoleCollection.Should().Contain(_role2);
			newPerson2.PermissionInformation.ApplicationRoleCollection.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void RevokeOneRoleOnOneUser()
		{
			/// Setup
			_person1.PermissionInformation.AddApplicationRole(_role1);
			PersonRepository.Add(_person1);
			ApplicationRoleRepository.Add(_role1);

			/// Test
			_person1.PermissionInformation.ApplicationRoleCollection.Should().Contain(_role1);

			var result = Target.RevokeRoles(new RevokeRolesInputModel()
			{
				Persons = new[] { _person1.Id.GetValueOrDefault() },
				Roles = new[] { _role1.Id.GetValueOrDefault() }
			});
			result.Should().Be.OfType<OkResult>();

			var newPerson = PersonRepository.Get(_person1.Id.GetValueOrDefault());
			newPerson.PermissionInformation.ApplicationRoleCollection.Should().Not.Contain(_role1);
			newPerson.PermissionInformation.ApplicationRoleCollection.Count().Should().Be.EqualTo(0);

			// Calling RevokeRoles a second time will not make any difference or create any errors.
			var result2 = Target.RevokeRoles(new RevokeRolesInputModel()
			{
				Persons = new[] { _person1.Id.GetValueOrDefault() },
				Roles = new[] { _role1.Id.GetValueOrDefault() }
			});
			result.Should().Be.OfType<OkResult>();

			var newPerson2 = PersonRepository.Get(_person1.Id.GetValueOrDefault());
			newPerson2.PermissionInformation.ApplicationRoleCollection.Should().Not.Contain(_role1);
			newPerson2.PermissionInformation.ApplicationRoleCollection.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void RevokeMultipleRolesOnMultipleUsers()
		{
			/// Setup
			_person1.PermissionInformation.AddApplicationRole(_role1);
			_person1.PermissionInformation.AddApplicationRole(_role2);
			_person2.PermissionInformation.AddApplicationRole(_role1);
			_person2.PermissionInformation.AddApplicationRole(_role2);

			PersonRepository.Add(_person1);
			PersonRepository.Add(_person2);
			ApplicationRoleRepository.Add(_role1);
			ApplicationRoleRepository.Add(_role2);

			/// Test
			_person1.PermissionInformation.ApplicationRoleCollection.Should().Contain(_role1);
			_person1.PermissionInformation.ApplicationRoleCollection.Should().Contain(_role2);
			_person2.PermissionInformation.ApplicationRoleCollection.Should().Contain(_role2);
			_person2.PermissionInformation.ApplicationRoleCollection.Should().Contain(_role2);
			
			var result = Target.RevokeRoles(new RevokeRolesInputModel()
			{
				Persons = new[] { _person1.Id.GetValueOrDefault(), _person2.Id.GetValueOrDefault() },
				Roles = new[] { _role1.Id.GetValueOrDefault(), _role2.Id.GetValueOrDefault() }
			});
			result.Should().Be.OfType<OkResult>();

			var newPerson = PersonRepository.Get(_person1.Id.GetValueOrDefault());
			newPerson.PermissionInformation.ApplicationRoleCollection.Should().Not.Contain(_role1);
			newPerson.PermissionInformation.ApplicationRoleCollection.Should().Not.Contain(_role2);
			newPerson.PermissionInformation.ApplicationRoleCollection.Count().Should().Be.EqualTo(0);

			newPerson = PersonRepository.Get(_person2.Id.GetValueOrDefault());
			newPerson.PermissionInformation.ApplicationRoleCollection.Should().Not.Contain(_role1);
			newPerson.PermissionInformation.ApplicationRoleCollection.Should().Not.Contain(_role2);
			newPerson.PermissionInformation.ApplicationRoleCollection.Count().Should().Be.EqualTo(0);
		}
	}
}
