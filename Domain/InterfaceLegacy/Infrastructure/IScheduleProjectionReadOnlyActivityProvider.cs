using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface IScheduleProjectionReadOnlyActivityProvider
	{
		List<ISiteActivity> GetActivitiesBySite(ISite site, DateTimePeriod period, IScenario scenario, bool onlyShowActivitiesRequiringSeats);
	}

	public interface ISiteActivity
	{
		Guid PersonId { get; set; }
		Guid ActivityId { get; set; }
		Guid SiteId { get; set; }
		DateTime StartDateTime { get; set; }
		DateTime EndDateTime { get; set; }

		bool RequiresSeat { get; set; }
	}


}