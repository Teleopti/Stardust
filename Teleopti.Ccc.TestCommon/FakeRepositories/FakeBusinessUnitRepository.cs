﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeBusinessUnitRepository : IBusinessUnitRepository
	{
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly IList<IBusinessUnit> _businessUnits = new List<IBusinessUnit>();

		public FakeBusinessUnitRepository(ICurrentBusinessUnit currentBusinessUnit)
		{
			_currentBusinessUnit = currentBusinessUnit;
		}

		public void Has(IBusinessUnit businessUnit)
		{
			Add(businessUnit);
		}

		public void HasCurrentBusinessUnit()
		{
			Has(_currentBusinessUnit.Current());
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
			throw new NotImplementedException();
		}

	}
}
