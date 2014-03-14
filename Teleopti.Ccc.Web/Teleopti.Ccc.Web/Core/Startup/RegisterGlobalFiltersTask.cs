using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.SSO.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(1)]
	public class RegisterGlobalFiltersTask : IBootstrapperTask
	{
		private readonly IErrorMessageProvider _errorMessageProvider;
		private readonly IAuthenticationModule _authenticationModule;

		public RegisterGlobalFiltersTask(IErrorMessageProvider errorMessageProvider, IAuthenticationModule authenticationModule)
		{
			_errorMessageProvider = errorMessageProvider;
			_authenticationModule = authenticationModule;
		}

		public Task Execute()
		{
			registerGlobalFilters(GlobalFilters.Filters);
			return null;
		}

		//rk - org from global.asax
		private void registerGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new AjaxHandleErrorAttribute(_errorMessageProvider));
			filters.Add(new TeleoptiPrincipalAuthorizeAttribute(_authenticationModule, new List<Type>
			                                                    	{
																		typeof(ShareCalendarController),
																		typeof(TestController),
																		typeof(OpenIdController)
			                                                    	}));
			filters.Add(new CheckStartupResultAttribute());
		}
	}
}