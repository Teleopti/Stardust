﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday.ApplicationLayer.ViewModels
{
	public class StaffingDataSeries
	{
		public StaffingDataSeries()
		{
			ForecastedStaffing = new double?[] { };
			UpdatedForecastedStaffing = new double?[] { };
			ActualStaffing = new double?[] { };
			ScheduledStaffing = new double?[] { };
			AbsoluteDifference = new double?[] { };
		}

		public DateOnly Date { get; set; }
		public DateTime[] Time { get; set; }
		public double?[] ForecastedStaffing { get; set; }
		public double?[] UpdatedForecastedStaffing { get; set; }
		public double?[] ActualStaffing { get; set; }
		public double?[] ScheduledStaffing { get; set; }
		public double?[] AbsoluteDifference { get; set; }

	}
}
