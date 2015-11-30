using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.Permissions
{
	public class FakeApplicationFunctionRepository : IApplicationFunctionRepository
	{
		readonly IList<IApplicationFunction> _applicationFunctions = new List<IApplicationFunction>();
		public void Add(IApplicationFunction entity)
		{
			_applicationFunctions.Add(entity);
		}

		public void Remove(IApplicationFunction entity)
		{
			throw new NotImplementedException();
		}

		public IApplicationFunction Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IApplicationFunction> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IApplicationFunction Load(Guid id)
		{
			return _applicationFunctions.FirstOrDefault(x => x.Id == id);
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IApplicationFunction> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public IList<IApplicationFunction> GetAllApplicationFunctionSortedByCode()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IApplicationFunction> ExternalApplicationFunctions()
		{
			throw new NotImplementedException();
		}

		public IList<IApplicationFunction> GetChildFunctions(Guid id)
		{
			return _applicationFunctions.Where(x => x.Parent != null && x.Parent.Id == id).ToList();
		}
	}
}