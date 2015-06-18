using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Interfaces.Domain;

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
			SeatPlanViewModel seatPlanViewModel = null;

			var seatPlan = _seatPlanRepository.GetSeatPlanForDate(date);

			if (seatPlan != null)
			{
				seatPlanViewModel = new SeatPlanViewModel()
				{
					Id = seatPlan.Id.Value,
					Date = seatPlan.Date.Date,
					Status = (int)seatPlan.Status
				};
			}
			return seatPlanViewModel;
		}

		public List<SeatPlanViewModel> Get (DateOnlyPeriod dateOnlyPeriod)
		{
			var seatPlanViewModels = new List<SeatPlanViewModel>();
			dateOnlyPeriod.DayCollection().ForEach (day => seatPlanViewModels.Add (Get (day)));
			return seatPlanViewModels;
		}
		
	}
}