﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Owin;
using Teleopti.Ccc.Web.Areas.MultiTenancy;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.Rta.Controllers;
using Teleopti.Ccc.Web.Areas.RtaTool.Controllers;
using Teleopti.Ccc.Web.Areas.SSO.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Toggle;
using Teleopti.Ccc.Web.Broker;
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

		public RegisterGlobalFiltersTask(IErrorMessageProvider errorMessageProvider, IAuthenticationModule authenticationModule, IIdentityProviderProvider identityProviderProvider)
		{
			_errorMessageProvider = errorMessageProvider;
			_authenticationModule = authenticationModule;
			_identityProviderProvider = identityProviderProvider;
		}

		public Task Execute(IAppBuilder application)
		{
			registerGlobalFilters(GlobalFilters.Filters);
			return null;
		}

		private void registerGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new AjaxHandleErrorAttribute(_errorMessageProvider));
			filters.Add(new TeleoptiPrincipalAuthorizeAttribute(_authenticationModule, _identityProviderProvider, new List<Type>
			{
				typeof (ShareCalendarController),
				typeof (TestController),
				typeof (UrlController),
				typeof (OpenIdController),
				typeof (ApplicationAuthenticationApiController),
				typeof (ToggleHandlerController),
				typeof (StateController),
				typeof (ActivityChangeController),
				typeof (MessageBrokerController),
				typeof (ApplicationController),
				typeof (AuthenticateController),
				typeof (ChangePasswordController),
				typeof(PersonInfoController), //TODO: tenant  - should have some kind of permission later....
				typeof(ConfigController)
			}));
		}
	}
}