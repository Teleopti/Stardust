using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class FakePermissionProvider : IPermissionProvider
	{
		private readonly Dictionary<string, DateOnly?> applicationFunctions = new Dictionary<string, DateOnly?>();
		private bool enabled = false;

		public bool HasApplicationFunctionPermission(string applicationFunctionPath)
		{
			return applicationFunctions.ContainsKey(applicationFunctionPath);
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
			if (!enabled) return true;
			return applicationFunctions.ContainsKey(applicationFunctionPath)
				&& (!applicationFunctions[applicationFunctionPath].HasValue || applicationFunctions[applicationFunctionPath] <= date);
		}

		public bool IsPersonSchedulePublished(DateOnly date, IPerson person,
			ScheduleVisibleReasons reason = ScheduleVisibleReasons.Published)
		{
			throw new NotImplementedException();
		}

		public void Enable()
		{
			enabled = true;
		}

		public void Disable()
		{
			enabled = false;
		}

		public void Permit(string applicationFunction)
		{
			if (!applicationFunctions.ContainsKey(applicationFunction))
				applicationFunctions.Add(applicationFunction, null);
		}

		public void Permit(string applicationFunction, DateOnly date)
		{
			if (!applicationFunctions.ContainsKey(applicationFunction))
				applicationFunctions.Add(applicationFunction, date);
		}

		public void Reject(string applicationFunction)
		{
			if (applicationFunctions.ContainsKey(applicationFunction))
				applicationFunctions.Remove(applicationFunction);
		}
	}
}