using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeSeatMapRepository : ISeatMapLocationRepository, IEnumerable<ISeatMapLocation>
	{
		private readonly IList<ISeatMapLocation> _seatMaps = new List<ISeatMapLocation>();


		public FakeSeatMapRepository(ISeatMapLocation seatMap)
		{
			Has(seatMap);
		}

		public void Has(ISeatMapLocation note)
		{
			_seatMaps.Add(note);
		}
	
		public void Add (ISeatMapLocation entity)
		{
			throw new NotImplementedException();
		}

		public void Remove (ISeatMapLocation entity)
		{
			throw new NotImplementedException();
		}

		public ISeatMapLocation Get (Guid id)
		{
			return _seatMaps.FirstOrDefault();
		}

		public IList<ISeatMapLocation> LoadAll()
		{
			throw new NotImplementedException();
		}

		public ISeatMapLocation Load (Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange (IEnumerable<ISeatMapLocation> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		ISeatMapLocation ILoadAggregateByTypedId<ISeatMapLocation, Guid>.LoadAggregate (Guid id)
		{
			return LoadAggregate (id);
		}

		public ISeatMapLocation LoadRootSeatMap()
		{
			return _seatMaps.FirstOrDefault();
		}

		public ISeatMapLocation LoadAggregate (Guid id)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<ISeatMapLocation> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}