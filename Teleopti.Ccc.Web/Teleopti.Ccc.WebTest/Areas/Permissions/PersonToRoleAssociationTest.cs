using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Permissions;

namespace Teleopti.Ccc.WebTest.Areas.Permissions
{
	[PermissionsTest]
	public class PersonToRoleAssociationTest
	{
		public PersonToRoleAssociation Target;
		public FakePersonInRoleQuerier PersonInRole;
		public IPersonRepository PersonRepository;
		
		[Test]
		public void ShouldRemoveAssociationBetweenPersonAndRole()
		{
			var role = ApplicationRoleFactory.CreateRole("role", "desc");
			var roleId = Guid.NewGuid();
			role.SetId(roleId);
			var person1 = PersonFactory.CreatePersonWithId();
			person1.PermissionInformation.AddApplicationRole(role);
			PersonRepository.Add(person1);
			PersonInRole.AddFakeData(new List<Guid>(){person1.Id.Value});
			Target.RemoveAssociation(role);
			person1.PermissionInformation.ApplicationRoleCollection.Should().Be.Empty();
		}
	}

	public class FakePersonInRoleQuerier : IPersonInRoleQuerier
	{
		private List<Guid> _personIdList = new List<Guid>();

		public void AddFakeData(List<Guid> personIdList)
		{
			_personIdList = personIdList;
		}
		
		public IEnumerable<Guid> GetPersonInRole(Guid roleId)
		{
			return _personIdList;
		}
	}
}
