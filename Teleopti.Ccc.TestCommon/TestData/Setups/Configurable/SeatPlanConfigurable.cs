using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class SeatPlanConfigurable : IDataSetup
	{
		public SeatPlan SeatPlan;

		public DateTime Date { get; set; }
		public SeatPlanStatus Status { get; set; }

		public void Apply (ICurrentUnitOfWork currentUnitOfWork)
		{

			SeatPlan = new SeatPlan()
			{

				Date = new DateOnly (Date),
				Status = Status
			};

			new SeatPlanRepository (currentUnitOfWork).Add (SeatPlan);
		}

	}
}