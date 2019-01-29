using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeBusinessUnitRepository : IBusinessUnitRepository
	{
		private readonly Lazy<ICurrentBusinessUnit> _currentBusinessUnit;
		private readonly HashSet<IBusinessUnit> _businessUnits = new HashSet<IBusinessUnit>();

		private readonly IList<TimeZoneInfo> _timeZones = new List<TimeZoneInfo>();

		public FakeBusinessUnitRepository(Lazy<ICurrentBusinessUnit> currentBusinessUnit)
		{
			_currentBusinessUnit = currentBusinessUnit;
		}

		public void HasCurrentBusinessUnit()
		{
			Add(_currentBusinessUnit.Value.Current());
		}
		
		public void Has(IBusinessUnit businessUnit)
		{
			Add(businessUnit);
		}

		public void Add(IBusinessUnit entity)
		{
			_businessUnits.Add(entity);
		}

		public void AddTimeZone(TimeZoneInfo timeZone)
		{
			_timeZones.Add(timeZone);
		}

		public void Remove(IBusinessUnit entity)
		{
			throw new NotImplementedException();
		}

		public IBusinessUnit Get(Guid id)
		{
			return _businessUnits.Single(x => x.Id.Value == id);
		}

		public IEnumerable<IBusinessUnit> LoadAll()
		{
			return _businessUnits;
		}

		public IBusinessUnit Load(Guid id)
		{
			return _businessUnits.First(x => x.Id.Equals(id));
		}

		public IEnumerable<IBusinessUnit> LoadAllWithDeleted()
		{
			return _businessUnits;
		}

		public IList<IBusinessUnit> LoadAllBusinessUnitSortedByName()
		{
			return _businessUnits.OrderBy(b => b.Name).ToArray();
		}

		public IBusinessUnit LoadHierarchyInformation(IBusinessUnit businessUnit)
		{
			return businessUnit;
		}

		public IEnumerable<TimeZoneInfo> LoadAllTimeZones()
		{
			return _timeZones;
		}
	}
}
