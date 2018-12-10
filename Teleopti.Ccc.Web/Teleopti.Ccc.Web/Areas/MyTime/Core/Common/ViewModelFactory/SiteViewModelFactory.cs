using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.ViewModelFactory
{
	public class SiteViewModelFactory:ISiteViewModelFactory
	{

		private readonly ISiteProvider _siteProvider;

		public SiteViewModelFactory(ISiteProvider siteProvider)
		{
			_siteProvider = siteProvider;
		}

		public IEnumerable<SelectOptionItem> CreateSiteOptionsViewModel(DateOnly date, string applicationFunctionPath)
		{
			var sites = _siteProvider.GetShowListSites(date, applicationFunctionPath).ToList();

			var options = sites
				.Select(s => new SelectOptionItem
				{
					id = s.Id.ToString(),
					text = s.Description.Name
				})
				.OrderBy(s => s.text);

			return options;
		}

		public IEnumerable<SelectOptionItem> GetTeams(List<Guid> siteIds, DateOnly date, string applicationFunctionPath)
		{
			var options = new List<SelectOptionItem>();

			foreach (var siteId in siteIds)
			{
				var teams = _siteProvider.GetPermittedTeamsUnderSite(siteId, date, applicationFunctionPath).ToList();
				var teamOptions = from t in teams
										select new SelectOptionItem
										{
											id = t.Id.ToString(),
											text = t.Description.Name
										};
				options.AddRange(teamOptions);
			}

			return options;
		} 
	}
}