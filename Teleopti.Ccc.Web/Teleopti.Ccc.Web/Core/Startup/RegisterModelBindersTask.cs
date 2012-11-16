using System;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(3)]
	public class RegisterModelBindersTask : IBootstrapperTask
	{
		private readonly Lazy<IAuthenticator> _authenticator;

		public RegisterModelBindersTask(Lazy<IAuthenticator> authenticator)
		{
			_authenticator = authenticator;
		}

		public void Execute()
		{
			RegisterModelBinders(ModelBinders.Binders);
		}

		public void RegisterModelBinders(ModelBinderDictionary binders)
		{
			var dateOnlyModelBinder = new DateOnlyModelBinder();
			var timeOfDayModelBinder = new TimeOfDayModelBinder();
			var nullableTimeOfDayModelBinder = new TimeOfDayModelBinder(nullable:true);
			var timeSpanModelBinder = new TimeSpanModelBinder();
			var nullableTimeSpanModelBinder = new TimeSpanModelBinder(nullable:true);
			var authenticationModelBinder = new AuthenticationModelBinder(_authenticator);

			binders[typeof (DateOnly?)] = dateOnlyModelBinder;
			binders[typeof (DateOnly)] = dateOnlyModelBinder;
			binders[typeof (TimeOfDay)] = timeOfDayModelBinder;
			binders[typeof(TimeOfDay?)] = nullableTimeOfDayModelBinder;
			binders[typeof(TimeSpan)] = timeSpanModelBinder;
			binders[typeof(TimeSpan?)] = nullableTimeSpanModelBinder;
			binders[typeof(IAuthenticationModel)] = authenticationModelBinder;
		}
	}

	public class AuthenticationModelBinder : IModelBinder
	{
		private readonly Lazy<IAuthenticator> _authenticator;

		public AuthenticationModelBinder(Lazy<IAuthenticator> authenticator)
		{
			_authenticator = authenticator;
		}

		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			var type = bindingContext.ValueProvider.GetValue("type").AttemptedValue;
			if (type == "windows")
				return new WindowsAuthenticationModel(_authenticator.Value)
					{
						DataSourceName = bindingContext.ValueProvider.GetValue("datasource").AttemptedValue
					};
			if (type == "application")
			{
				return new ApplicationAuthenticationModel(_authenticator.Value)
					{
						UserName = bindingContext.ValueProvider.GetValue("username").AttemptedValue,
						Password = bindingContext.ValueProvider.GetValue("password").AttemptedValue,
						DataSourceName = bindingContext.ValueProvider.GetValue("datasource").AttemptedValue
					};
			}
			throw new NotImplementedException("Authentication type " + type + " not implemented");
		}
	}

	public interface IAuthenticationModel
	{
		AuthenticateResult AuthenticateUser();
	}

	public class WindowsAuthenticationModel : IAuthenticationModel
	{
		private readonly IAuthenticator _authenticator;
		public string DataSourceName { get; set; }

		public WindowsAuthenticationModel(IAuthenticator authenticator)
		{
			_authenticator = authenticator;
		}

		public AuthenticateResult AuthenticateUser()
		{
			return _authenticator.AuthenticateWindowsUser(DataSourceName);
		}
	}

	public class ApplicationAuthenticationModel : IAuthenticationModel
	{
		private readonly IAuthenticator _authenticator;
		public string DataSourceName { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }

		public ApplicationAuthenticationModel(IAuthenticator authenticator)
		{
			_authenticator = authenticator;
		}

		public AuthenticateResult AuthenticateUser()
		{
			return _authenticator.AuthenticateApplicationUser(DataSourceName, UserName, Password);
		}

	}
}