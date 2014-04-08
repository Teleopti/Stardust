using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Areas.Start.Core;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(4)]
	public class RegisterModelBindersTask : IBootstrapperTask
	{
		private readonly IEnumerable<IAuthenticationType> _authenticatorTypes;
		private readonly IApplicationAuthenticationType _applicationAuthenticationType;

		public RegisterModelBindersTask(IEnumerable<IAuthenticationType> authenticatorTypes, IApplicationAuthenticationType applicationAuthenticationType)
		{
			_authenticatorTypes = authenticatorTypes;
			_applicationAuthenticationType = applicationAuthenticationType;
		}

		public Func<ModelBinderDictionary> BindersGetter = () => ModelBinders.Binders;
 
		public Task Execute()
		{
			RegisterModelBinders(BindersGetter());
			return null;
		}

		public void RegisterModelBinders(ModelBinderDictionary binders)
		{
			var dateOnlyModelBinder = new DateOnlyModelBinder();
			var timeOfDayModelBinder = new TimeOfDayModelBinder();
			var nullableTimeOfDayModelBinder = new TimeOfDayModelBinder(nullable:true);
			var timeSpanModelBinder = new TimeSpanModelBinder();
			var nullableTimeSpanModelBinder = new TimeSpanModelBinder(nullable:true);
			var authenticationModelBinder = new AuthenticationModelBinder(_authenticatorTypes);
			var ssoSuthenticationModelBinder = new SsoAuthenticationModelBinder(_applicationAuthenticationType);

			binders[typeof (DateOnly?)] = dateOnlyModelBinder;
			binders[typeof (DateOnly)] = dateOnlyModelBinder;
			binders[typeof (TimeOfDay)] = timeOfDayModelBinder;
			binders[typeof(TimeOfDay?)] = nullableTimeOfDayModelBinder;
			binders[typeof(TimeSpan)] = timeSpanModelBinder;
			binders[typeof(TimeSpan?)] = nullableTimeSpanModelBinder;
			binders[typeof(IAuthenticationModel)] = authenticationModelBinder;
			binders[typeof(ApplicationAuthenticationModel)] = ssoSuthenticationModelBinder;
		}
	}
}