using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class SeatBookingConfigurable : IUserSetup
	{
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public DateTime BelongsToDate { get; set; }
		public string SeatName { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var seatBooking = new SeatBooking(user, new DateOnly(BelongsToDate), StartDateTime, EndDateTime);
			var seatRepo = new SeatMapLocationRepository(new ThisUnitOfWork(uow));
			var seatmap = seatRepo.LoadRootSeatMap();

			seatBooking.Seat = seatmap.Seats.Single(seat => seat.Name == SeatName);

			var repo = new SeatBookingRepository(new ThisUnitOfWork(uow));
			repo.Add(seatBooking);
		}
	}
	
}
