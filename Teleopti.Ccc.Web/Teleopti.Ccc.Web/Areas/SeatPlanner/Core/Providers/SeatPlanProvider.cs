using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;


namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public class SeatPlanProvider : ISeatPlanProvider
	{
		private readonly ISeatPlanRepository _seatPlanRepository;
		protected SeatPlanProvider()
		{
		}

		public SeatPlanProvider(ISeatPlanRepository seatPlanRepository)
		{
			_seatPlanRepository = seatPlanRepository;
		}

		public SeatPlanViewModel Get(DateOnly date)
		{
			var seatPlan = _seatPlanRepository.GetSeatPlanForDate(date);

			if (seatPlan != null)
			{
				return new SeatPlanViewModel()
				{
					Id = seatPlan.Id.Value,
					Date = seatPlan.Date.Date,
					Status = (int)seatPlan.Status
				};
			}
		
			return null;
		}

		public List<SeatPlanViewModel> Get (DateOnlyPeriod dateOnlyPeriod)
		{
			return dateOnlyPeriod.DayCollection().Select (Get).Where (seatPlanModel => seatPlanModel != null).ToList();
		}
	}



		
	
}