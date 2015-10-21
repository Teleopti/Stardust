using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface IPersonalAvailableDataProvider
	{
		IEnumerable<ISite> AvailableSites(string functionPath, DateOnly date);
	}
	public class PersonalAvailableDataProvider : IPersonalAvailableDataProvider
	{
		private readonly ISiteRepository _siteRepository;
		private readonly IPermissionProvider _permissionProvider;

		public PersonalAvailableDataProvider(ISiteRepository siteRepository,
			IPermissionProvider permissionProvider)
		{
			_siteRepository = siteRepository;
			_permissionProvider = permissionProvider;
		}

		public IEnumerable<ISite> AvailableSites(string functionPath, DateOnly date)
		{
			var sites = _siteRepository.LoadAll();
			return sites.Where(x => _permissionProvider.HasSitePermission(functionPath, date, x));
		}
	}
}