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
			IEnumerable<ISeatMapLocation> locations, IEnumerable<ITeam> teams, DateTime startDate, DateTime endDate, int skip, int take)
		{
			Locations = locations;
			Teams = teams;
			StartDate = startDate;
			EndDate = endDate;
			Skip = skip;
			Take = take;
		}

		public IEnumerable<ISeatMapLocation> Locations { get; set; }
		public IEnumerable<ITeam> Teams { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int Skip { get; set; }
		public int Take { get; set; }
	}





}