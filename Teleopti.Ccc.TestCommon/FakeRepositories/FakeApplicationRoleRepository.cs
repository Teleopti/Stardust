using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeApplicationRoleRepository : IApplicationRoleRepository
	{
		private readonly List<IApplicationRole> _roles = new List<IApplicationRole>();

		public void Has(IApplicationRole entity)
		{
			Add(entity);
		}

		public void Add(IApplicationRole entity)
		{
			_roles.Add(entity);
		}

		public void Remove(IApplicationRole entity)
		{
			_roles.Remove(entity);
		}

		public IApplicationRole Get(Guid id)
		{
			return _roles.FirstOrDefault(r => r.Id.GetValueOrDefault() == id);
		}

		public IList<IApplicationRole> LoadAll()
		{
			return _roles;
		}

		public IApplicationRole Load(Guid id)
		{
			return _roles.FirstOrDefault(x => x.Id.Value == id);
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		public IList<IApplicationRole> LoadAllApplicationRolesSortedByName()
		{
			return _roles.OrderBy(x => x.Name).ToList();
		}

		public IList<IApplicationRole> LoadAllRolesByDescription(string name)
		{
			return _roles.Where(x => x.DescriptionText.ToLower().Contains(name.ToLower())).ToList();
		}

		public bool ExistsRoleWithDescription(string description)
		{
			return _roles.Any(x => x.DescriptionText.ToLower().Equals(description));
		}
	}
}
