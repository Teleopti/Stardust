﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(1)]
	public class RegisterGlobalFiltersTask : IBootstrapperTask
	{
		private static IErrorMessageProvider _errorMessageProvider;

		public RegisterGlobalFiltersTask(IErrorMessageProvider errorMessageProvider) 
		{
			_errorMessageProvider = errorMessageProvider;
		}

		public void Execute()
		{
			registerGlobalFilters(GlobalFilters.Filters);
		}

		//rk - org from global.asax
		private static void registerGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new AjaxHandleErrorAttribute(_errorMessageProvider));
			filters.Add(new TeleoptiPrincipalAuthorizeAttribute(new List<Type>
			                                                    	{
																		typeof(AuthenticationController),
																		typeof(AuthenticationApiController),
																		typeof(ApplicationAuthenticationApiController),
																		typeof(TestController)
			                                                    	}));
			filters.Add(new CheckStartupExceptionAttribute());
		}
	}
}