using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours
{
	public class SiteWithOpenHourProvider : ISiteWithOpenHourProvider
	{
		private readonly INow _now;
		private readonly ISiteRepository _siteRepository;
		private readonly ICurrentAuthorization _authorization;
		private readonly ILoggedOnUser _loggedOnUser;

		public SiteWithOpenHourProvider(
			INow now,
			ISiteRepository siteRepository,
			ICurrentAuthorization authorization,
			ILoggedOnUser loggedOnUser)
		{
			_now = now;
			_siteRepository = siteRepository;
			_authorization = authorization;
			_loggedOnUser = loggedOnUser;
		}

		public IEnumerable<SiteViewModel> GetSitesWithOpenHour()
		{
			var sites = allPermittedSites();
			return sites.Select(site => new SiteViewModel
			{
				Id = site.Id.Value,
				Name = site.Description.Name,
				OpenHours = openHours(site)
			}).ToList();
		}

		private IEnumerable<ISite> allPermittedSites()
		{
			var currentAuth = _authorization.Current();
			var userToday = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(),
				_loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone()));

			return _siteRepository.LoadAllOrderByName()
				.Where(s => currentAuth.IsPermitted(DefinedRaptorApplicationFunctionPaths.WebRequests, userToday, s));
		}

		private static IEnumerable<SiteOpenHourViewModel> openHours(ISite site)
		{
			return site.OpenHourCollection.Select(openHour => new SiteOpenHourViewModel
			{
				WeekDay = openHour.WeekDay,
				StartTime = openHour.TimePeriod.StartTime,
				EndTime = openHour.TimePeriod.EndTime,
				IsClosed = openHour.IsClosed
			}).ToList();
		}
	}
}