using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeBusinessUnitRepository : IBusinessUnitRepository
	{

		private IList<IBusinessUnit> _businessUnits = new List<IBusinessUnit>();

		public void Has(BusinessUnit businessUnit)
		{
			Add(businessUnit);
		}

		public void Add(IBusinessUnit entity)
		{
			_businessUnits.Add(entity);
		}

		public void Remove(IBusinessUnit entity)
		{
			throw new NotImplementedException();
		}

		public IBusinessUnit Get(Guid id)
		{
			return _businessUnits.Single(x => x.Id.Value == id);
		}

		public IList<IBusinessUnit> LoadAll()
		{
			return _businessUnits;
		}

		public IBusinessUnit Load(Guid id)
		{
			return _businessUnits.First(x => x.Id.Equals(id));
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IBusinessUnit> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public IList<IBusinessUnit> LoadAllBusinessUnitSortedByName()
		{
			return _businessUnits.OrderBy(b => b.Name).ToArray();
		}

		public IBusinessUnit LoadHierarchyInformation(IBusinessUnit businessUnit)
		{
			throw new NotImplementedException();
		}

		public IList<Guid> LoadAllPersonsWithExternalLogOn(Guid businessUnitId, DateOnly now)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<TimeZoneInfo> LoadAllTimeZones()
		{
			throw new NotImplementedException();
		}

	}
}
