﻿using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider
{
	public interface IPermissionProvider
	{
		bool HasApplicationFunctionPermission(string applicationFunctionPath);
		bool HasPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person);
		bool HasTeamPermission(string applicationFunctionPath, DateOnly date, ITeam team);
		bool HasOrganisationDetailPermission(string applicationFunctionPath, DateOnly date, IAuthorizeOrganisationDetail authorizeOrganisationDetail);

		bool IsPermittedToSeeSchedule(DateOnly date,
			IPerson person, bool includeUnpublished);
	}
}