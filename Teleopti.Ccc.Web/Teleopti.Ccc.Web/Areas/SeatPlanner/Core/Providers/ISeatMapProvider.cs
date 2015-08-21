﻿using System;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public interface ISeatMapProvider
	{
		LocationViewModel Get(Guid? id, DateOnly? bookingDate = null);
	}
}