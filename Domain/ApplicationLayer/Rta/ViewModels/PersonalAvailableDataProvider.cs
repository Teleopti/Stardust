using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public interface IPersonalAvailableDataProvider
	{
		IEnumerable<ISite> AvailableSites(string functionPath, DateOnly date);
	}

	public class PersonalAvailableDataProvider : IPersonalAvailableDataProvider
	{
		private readonly ISiteRepository _siteRepository;
		private readonly ICurrentAuthorization _permissionProvider;

		public PersonalAvailableDataProvider(
			ISiteRepository siteRepository,
			ICurrentAuthorization permissionProvider)
		{
			_siteRepository = siteRepository;
			_permissionProvider = permissionProvider;
		}

		public IEnumerable<ISite> AvailableSites(string functionPath, DateOnly date)
		{
			var sites = _siteRepository.LoadAll();
			return sites.Where(x => _permissionProvider.Current().IsPermitted(functionPath, date, x));
		}
	}
}