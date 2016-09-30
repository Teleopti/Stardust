using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.ViewModelFactory
{
	public class SiteViewModelFactory:ISiteViewModelFactory
	{

		private readonly ISiteProvider _siteProvider;

		public SiteViewModelFactory(ISiteProvider siteProvider)
		{
			_siteProvider = siteProvider;
		}

		public IEnumerable<ISelectOption> CreateSiteOptionsViewModel(DateOnly date, string applicationFunctionPath)
		{
			var sites = _siteProvider.GetPermittedSites(date, applicationFunctionPath).ToList();
			var businessUnits = sites
				.Select(t => t.BusinessUnit)
				.Distinct();

			var options = new List<ISelectOption>();
			businessUnits.ForEach(s =>
			{
				var siteOptions = from t in sites
										where t.BusinessUnit == s
										select new SelectOptionItem
										{
											id = t.Id.ToString(),
											text = s.Description.Name + "/" + t.Description.Name
										};
				options.AddRange(siteOptions);
			});

			return options;
		}

		public IEnumerable<ISelectOption> GetTeams(List<Guid> siteIds, DateOnly date, string applicationFunctionPath)
		{
			var options = new List<ISelectOption>();

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