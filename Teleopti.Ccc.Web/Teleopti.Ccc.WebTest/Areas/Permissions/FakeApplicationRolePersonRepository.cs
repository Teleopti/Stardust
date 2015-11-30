using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.WebTest.Areas.Permissions
{
	public class FakeApplicationRolePersonRepository : IApplicationRolePersonRepository
	{
		private readonly IList<IPersonInRole> _personInRole = new List<IPersonInRole>();

		public void AddFakeData(IPersonInRole personInRole)
		{
			_personInRole.Add(personInRole);
		}

		public IList<IPersonInRole> GetPersonsInRole(Guid roleId)
		{
			return _personInRole;
		}

		public IList<IPersonInRole> GetPersonsNotInRole(Guid roleId, ICollection<Guid> personsIds)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonInRole> Persons()
		{
			throw new NotImplementedException();
		}

		public IList<IRoleLight> RolesOnPerson(Guid selectedPerson)
		{
			throw new NotImplementedException();
		}

		public IList<IFunctionLight> FunctionsOnPerson(Guid selectedPerson)
		{
			throw new NotImplementedException();
		}

		public IList<IFunctionLight> Functions()
		{
			throw new NotImplementedException();
		}

		public IList<IPersonInRole> PersonsWithFunction(Guid selectedFunction)
		{
			throw new NotImplementedException();
		}

		public IList<IRoleLight> RolesWithFunction(Guid selectedFunction)
		{
			throw new NotImplementedException();
		}

		public IList<Guid> AvailableData(Guid selectedPerson)
		{
			throw new NotImplementedException();
		}

		public IList<int> DataRangeOptions(Guid selectedPerson)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonInRole> PersonsWithRoles(IList<Guid> roles)
		{
			throw new NotImplementedException();
		}

		public IList<IRoleLight> RolesWithData(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IRoleLight> RolesWithDataRange(int range)
		{
			throw new NotImplementedException();
		}
	}
}