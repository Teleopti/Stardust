using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeSeatMapRepository : ISeatMapLocationRepository, IEnumerable<ISeatMapLocation>
	{
		private readonly IList<ISeatMapLocation> _seatMaps = new List<ISeatMapLocation>();

		public FakeSeatMapRepository(params ISeatMapLocation[] seatMapLocations)
		{
			seatMapLocations.ForEach (Add);
		}
		
		public void Add (ISeatMapLocation entity)
		{
			_seatMaps.Add (entity);
		}

		public void Remove (ISeatMapLocation entity)
		{
			_seatMaps.Remove (entity);
		}

		public ISeatMapLocation Get (Guid id)
		{
			return _seatMaps.SingleOrDefault (seatMap => seatMap.Id.Value == id);
		}

		public IList<ISeatMapLocation> LoadAll()
		{
			throw new NotImplementedException();
		}

		public ISeatMapLocation Load (Guid id)
		{
			throw new NotImplementedException();
		}

		ISeatMapLocation ILoadAggregateByTypedId<ISeatMapLocation, Guid>.LoadAggregate (Guid id)
		{
			return LoadAggregate (id);
		}

		public ISeatMapLocation LoadRootSeatMap()
		{
			return _seatMaps.FirstOrDefault();
		}

		public IList<ISeatMapLocation> FindLocations(IList<Guid> locationIds )
		{
			return _seatMaps
				.Where(location => locationIds.Contains(location.Id.Value))
				.ToList();
		}

		public ISeatMapLocation LoadAggregate (Guid id)
		{
			return Get (id);
		}

		public IEnumerator<ISeatMapLocation> GetEnumerator()
		{
			return _seatMaps.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}