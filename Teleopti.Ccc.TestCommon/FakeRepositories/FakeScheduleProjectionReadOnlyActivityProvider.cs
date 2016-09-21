using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleProjectionReadOnlyActivityProvider : IScheduleProjectionReadOnlyActivityProvider
	{
		private List<ISiteActivity> _siteActivities = new List<ISiteActivity>();

		public void AddSiteActivity (ISiteActivity siteActivity)
		{
			_siteActivities.Add (siteActivity);
		}

		public List<ISiteActivity> GetActivitiesBySite (ISite site, DateTimePeriod period, IScenario scenario,
			bool onlyShowActivitiesRequiringSeats)
		{
			return _siteActivities.Where (siteActivity => siteActivity.SiteId == site.Id
														  && siteActivity.StartDateTime < period.EndDateTime && siteActivity.EndDateTime > period.StartDateTime
														  && siteActivity.RequiresSeat == onlyShowActivitiesRequiringSeats).ToList();

		}
	}
}
