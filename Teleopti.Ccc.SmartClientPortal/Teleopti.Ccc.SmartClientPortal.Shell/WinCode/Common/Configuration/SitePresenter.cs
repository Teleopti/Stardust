using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
	public class SitePresenter
	{
		private readonly ISiteRepository _siteRepository;
		private readonly List<ISite> _siteCollection = new List<ISite>();
		private readonly ISiteView _siteView;

		public SitePresenter(ISiteView siteView, ISiteRepository siteRepository)
		{
			_siteView = siteView;
			_siteRepository = siteRepository;
		}

		public void OnPageLoad()
        {
            LoadSites();
			_siteView.LoadSiteGrid(_siteCollection);	
		}

		private void LoadSites()
		{
			// if we get the deleted ones
			var tmp = _siteRepository.LoadAll();
			_siteCollection.Clear();
            _siteCollection.AddRange(tmp.Where(site => !((IDeleteTag)site).IsDeleted).OrderBy(s => s.Description.Name));
		}
	}
}