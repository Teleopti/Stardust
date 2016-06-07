﻿using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;

namespace Teleopti.Ccc.Web.Filters
{
	public class LicenseAttribute : AuthorizeAttribute
	{
		private readonly string[] _licenses;

		public LicenseAttribute(string license)
		{
			_licenses = new []{license};
			Order = 3;
		}

		public ICurrentDataSource CurrentDataSource { get; set; }

		public override void OnAuthorization(AuthorizationContext filterContext)
		{
			if (_licenses != null && _licenses.Length > 0)
			{
				var currentName = CurrentDataSource.CurrentName();
				var hasLicense = DefinedLicenseDataFactory.HasLicense(currentName) &&
				                 _licenses.Any(
					                 l =>
						                 DefinedLicenseDataFactory.GetLicenseActivator(currentName)
							                 .EnabledLicenseOptionPaths.Contains(l));
				if (!hasLicense)
				{
					filterContext.HttpContext.Response.StatusCode = 403;
					filterContext.Result = new JsonResult
					{
						JsonRequestBehavior = JsonRequestBehavior.AllowGet
					};
				}
			}
			base.OnAuthorization(filterContext);
		}

		protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
		{
			return true;
		}
	}
}