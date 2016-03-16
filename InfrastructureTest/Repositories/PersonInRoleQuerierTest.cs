using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	public class PersonInRoleQuerierTest : DatabaseTest
	{
		[Test]
		public void ShouldReturnEmptyIfNoAssociationFound()
		{
			var role = ApplicationRoleFactory.CreateRole("role", "somerole");
			PersistAndRemoveFromUnitOfWork(role);
			var target = new PersonInRoleQuerier(new ThisUnitOfWork(UnitOfWork));
			target.GetPersonInRole(role.Id.Value).Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnPersonIfAssociationFound()
		{
			var person = PersonFactory.CreatePerson();
			var role = ApplicationRoleFactory.CreateRole("role", "desc");
			PersistAndRemoveFromUnitOfWork(role);
			person.PermissionInformation.AddApplicationRole(role);
			PersistAndRemoveFromUnitOfWork(person);
			var target = new PersonInRoleQuerier(new ThisUnitOfWork(UnitOfWork));
			target.GetPersonInRole(role.Id.Value).Should().Not.Be.Empty();
		}
	}
}
