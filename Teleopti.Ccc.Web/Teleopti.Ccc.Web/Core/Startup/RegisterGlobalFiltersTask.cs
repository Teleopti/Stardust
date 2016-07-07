﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Owin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.RtaTool.Controllers;
using Teleopti.Ccc.Web.Areas.SSO.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Core.Logging;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(1)]
	public class RegisterGlobalFiltersTask : IBootstrapperTask
	{
		private readonly IErrorMessageProvider _errorMessageProvider;
		private readonly IAuthenticationModule _authenticationModule;
		private readonly IIdentityProviderProvider _identityProviderProvider;
		private readonly Log4NetLogger _log4NetLogger;
		private readonly ILoadAllTenantsUsers _loadAllTenantsUsers;

		public RegisterGlobalFiltersTask(IErrorMessageProvider errorMessageProvider,
			IAuthenticationModule authenticationModule, IIdentityProviderProvider identityProviderProvider,
			Log4NetLogger log4NetLogger, ILoadAllTenantsUsers loadAllTenantsUsers)
		{
			_errorMessageProvider = errorMessageProvider;
			_authenticationModule = authenticationModule;
			_identityProviderProvider = identityProviderProvider;
			_log4NetLogger = log4NetLogger;
			_loadAllTenantsUsers = loadAllTenantsUsers;
		}

		public Task Execute(IAppBuilder application)
		{
			registerGlobalFilters(GlobalFilters.Filters);
			return Task.FromResult(false);
		}

		private void registerGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new Log4NetMvCLogger(_log4NetLogger));
			filters.Add(new NoCacheFilterMvc());
			filters.Add(new AjaxHandleErrorAttribute(_errorMessageProvider));
			filters.Add(new TeleoptiPrincipalAuthorizeAttribute(_authenticationModule, _identityProviderProvider, _loadAllTenantsUsers, new List<Type>
			{
				typeof (ShareCalendarController),
				typeof (TestController),
				typeof (UrlController),
				typeof (OpenIdController),
				typeof (ApplicationController),
				typeof (ReturnController),
				typeof(TenantAdminInfoController)
			}));
		}
	}
}