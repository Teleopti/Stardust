using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class FakePermissionProvider : IPermissionProvider
	{
		private static readonly List<string> applicationFunctions = new List<string>();

		public bool HasApplicationFunctionPermission(string applicationFunctionPath)
		{
			return applicationFunctions.Contains(applicationFunctionPath);
		}

		public bool HasPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person)
		{
			throw new NotImplementedException();
		}

		public bool HasTeamPermission(string applicationFunctionPath, DateOnly date, ITeam team)
		{
			throw new NotImplementedException();
		}

		public bool HasSitePermission(string applicationfunctionpath, DateOnly today, ISite site)
		{
			throw new NotImplementedException();
		}

		public bool HasOrganisationDetailPermission(string applicationFunctionPath, DateOnly date,
			IAuthorizeOrganisationDetail authorizeOrganisationDetail)
		{
			throw new NotImplementedException();
		}

		public bool IsPersonSchedulePublished(DateOnly date, IPerson person,
			ScheduleVisibleReasons reason = ScheduleVisibleReasons.Published)
		{
			throw new NotImplementedException();
		}

		public static void Permit(string applicationFunction)
		{
			if (!applicationFunctions.Contains(applicationFunction))
				applicationFunctions.Add(applicationFunction);
		}
	}
}