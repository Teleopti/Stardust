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
			return _seatPlans;
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

		public void UpdateStatusForDate (DateOnly date, SeatPlanStatus seatPlanStatus)
		{
			
		}

		public IEnumerator<ISeatPlan> GetEnumerator()
		{
			return _seatPlans.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Update (ISeatPlan seatPlan)
		{
			//_seatPlans.Select (plan => plan.Date==seatPlan.Date)
		}

		public ISeatPlan GetSeatPlanForDate (DateOnly date)
		{
			return _seatPlans.SingleOrDefault(seatPlan => seatPlan.Date == date);
		}
	}
}