using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.Search
{
	public class FakeApplicationRoleRepository : IApplicationRoleRepository
	{

		private List<IApplicationRole> _roles = new List<IApplicationRole>();

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
			return _roles.FirstOrDefault();
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

		public void AddRange(IEnumerable<IApplicationRole> entityCollection)
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
	}
}
