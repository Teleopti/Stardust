﻿using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IPermissionProvider
	{
		bool HasApplicationFunctionPermission(string applicationFunctionPath);
		bool HasPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person);
		bool HasTeamPermission(string applicationFunctionPath, DateOnly date, ITeam team);
		bool HasSitePermission(string applicationfunctionpath, DateOnly today, ISite site);
		bool HasOrganisationDetailPermission(string applicationFunctionPath, DateOnly date, IPersonAuthorizationInfo personAuthorizationInfo);

		bool IsPersonSchedulePublished(DateOnly date,
			IPerson person, ScheduleVisibleReasons reason = ScheduleVisibleReasons.Published);
	}
}
