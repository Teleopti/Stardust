using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeSeatPlanRepository : ISeatPlanRepository, IEnumerable<ISeatPlan>
	{

		private readonly IList<ISeatPlan> _seatPlans = new List<ISeatPlan>();

		public void Add (ISeatPlan seatPlan)
		{
			_seatPlans.Add (seatPlan);
		}

		public void Remove(ISeatPlan seatPlan)
		{
			_seatPlans.Remove (seatPlan);
		}

		public ISeatPlan Get (Guid id)
		{
			return _seatPlans.SingleOrDefault(seatPlan => seatPlan.Id == id);
		}

		public IList<ISeatPlan> LoadAll()
		{
			throw new NotImplementedException();
		}

		public ISeatPlan Load (Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange (IEnumerable<ISeatPlan> entityCollection)
		{
			entityCollection.ForEach(Add);
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public ISeatPlan LoadAggregate (Guid id)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<ISeatPlan> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}