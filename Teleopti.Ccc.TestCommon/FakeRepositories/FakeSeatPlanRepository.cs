using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


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

		public IEnumerable<ISeatPlan> LoadAll()
		{
			return _seatPlans;
		}

		public ISeatPlan Load (Guid id)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<ISeatPlan> GetEnumerator()
		{
			return _seatPlans.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void RemoveSeatPlanForDate (DateOnly date)
		{
			var seatPlan = GetSeatPlanForDate (date);
			if (seatPlan != null)
			{
				Remove (seatPlan);
			}
		}

		public ISeatPlan GetSeatPlanForDate (DateOnly date)
		{
			return _seatPlans.SingleOrDefault(seatPlan => seatPlan.Date == date);
		}
	}
}