﻿using System;
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

		public IEnumerable<Guid> GetTeamIds(List<Guid> siteIds)
		{
			var teamIds = new List<Guid>();

			foreach (var siteId in siteIds)
			{
				teamIds.AddRange(_siteProvider.GetTeamIdsUnderSite(siteId));
			}

			return teamIds;
		}
	}
}