﻿using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SeatPlanRepository : Repository<ISeatPlan>, ISeatPlanRepository
	{
		public SeatPlanRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
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
	}
}