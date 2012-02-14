using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
	public class SitePresenter
	{
		private readonly ISiteRepository _siteRepository;
		private IList<ISite> _siteCollection = new List<ISite>();
		private ISiteView _siteView;

		public SitePresenter(ISiteView siteView, ISiteRepository siteRepository)
		{
			_siteView = siteView;
			_siteRepository = siteRepository;
		}

		public void OnPageLoad()
		{
			_siteView.LoadSiteGrid(AllNotDeletedSites);	
		}

		private IList<ISite> AllNotDeletedSites
		{
			get
			{
				CheckSitesCollection();
				var tmp = from site in _siteCollection orderby site.Description.Name select site;
				return tmp.ToList();
			}
		}

		//if site has been added or deleted on another page we must chec this
		private void CheckSitesCollection()
		{
			if (_siteCollection.IsEmpty())
			{
				LoadSites();
				return;
			}

			var refreshedSites = _siteRepository.LoadAll();

			var notDeletedSites = refreshedSites.Where(site => !((IDeleteTag)site).IsDeleted).ToList();
			foreach (var notDeletedSite in notDeletedSites.Where(notDeletedSite => !_siteCollection.Contains(notDeletedSite)))
			{
				_siteCollection.Add(notDeletedSite);
			}

			var deletedSites = refreshedSites.Where(site => ((IDeleteTag)site).IsDeleted).ToList();
			foreach (var deletedSite in deletedSites.Where(deletedSite => _siteCollection.Contains(deletedSite)))
			{
				_siteCollection.Remove(deletedSite);
			}
		}

		private void LoadSites()
		{
			// if we get the deleted ones
			var tmp = _siteRepository.LoadAll();
			_siteCollection = tmp.Where(site => !((IDeleteTag)site).IsDeleted).ToList();
		}
	}
}