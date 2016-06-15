using System;
using System.Collections.Generic;
using System.Configuration;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class SeatBookingReportCommand
	{
		public SeatBookingReportCommand()
		{
		}

		public SeatBookingReportCommand(
			IEnumerable<Guid> locations, IEnumerable<Guid> teams, DateTime startDate, DateTime endDate, int skip, int take, bool showUnseatedOnly)
		{
			Locations = locations;
			Teams = teams;
			StartDate = startDate;
			EndDate = endDate;
			Skip = skip;
			Take = take;
			ShowUnseatedOnly = showUnseatedOnly;
		}

		public IEnumerable<Guid> Locations { get; set; }
		public IEnumerable<Guid> Teams { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int Skip { get; set; }
		public int Take { get; set; }
		public bool ShowUnseatedOnly { get; set; }
	}





}