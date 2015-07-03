using System;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class SeatPlanConfigurable : IDataSetup
	{
		public SeatPlan SeatPlan;

		public DateTime Date { get; set; }
		public SeatPlanStatus Status { get; set; }

		public void Apply (IUnitOfWork uow)
		{

			SeatPlan = new SeatPlan()
			{

				Date = new DateOnly (Date),
				Status = Status
			};

			new SeatPlanRepository (uow).Add (SeatPlan);
		}

	}
}