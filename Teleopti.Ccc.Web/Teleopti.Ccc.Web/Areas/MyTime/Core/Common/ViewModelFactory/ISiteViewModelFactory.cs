using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.ViewModelFactory
{
	public interface ISiteViewModelFactory
	{
		IEnumerable<SelectOptionItem> CreateSiteOptionsViewModel(DateOnly date, string applicationFunctionPath);

		IEnumerable<SelectOptionItem> GetTeams(List<Guid> siteIds, DateOnly date, string applicationFunctionPath);
	}
}