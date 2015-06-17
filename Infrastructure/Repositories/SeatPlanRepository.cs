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

		public SeatPlanRepository(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
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

		public void UpdateStatusForDate (DateOnly date, SeatPlanStatus seatPlanStatus)
		{

			var seatPlans = Session.Query<ISeatPlan>().Where (seatPlan => seatPlan.Date == date );
			foreach (var seatPlan in seatPlans)
			{
				seatPlan.Status = seatPlanStatus;
				Session.Update (seatPlan);
			}
			
		}
	}
}