using System;
using System.Linq;
using NHibernate.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SeatPlanRepository : Repository<ISeatPlan>, ISeatPlanRepository
	{
		public SeatPlanRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
		{
		}

		public SeatPlanRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public ISeatPlan LoadAggregate (Guid id)
		{
			return Session.Query<ISeatPlan>()
				.FirstOrDefault(plan => plan.Id == id);
		}

		public ISeatPlan GetSeatPlanForDate (DateOnly date)
		{
			return 	Session.Query<ISeatPlan>().SingleOrDefault(seatPlan => seatPlan.Date == date );
		}

		public void RemoveSeatPlanForDate (DateOnly date)
		{
			var seatPlan = GetSeatPlanForDate (date);
			if (seatPlan != null)
			{
				Remove(seatPlan);
			}
		}


		public void Update (ISeatPlan existingSeatPlan)
		{
			Session.Update (existingSeatPlan);
		}
	}
}