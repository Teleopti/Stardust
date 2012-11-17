using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(3)]
	public class RegisterModelBindersTask : IBootstrapperTask
	{
		private readonly IEnumerable<IAuthenticationType> _authenticatorTypes;

		public RegisterModelBindersTask(IEnumerable<IAuthenticationType> authenticatorTypes)
		{
			_authenticatorTypes = authenticatorTypes;
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
			var authenticationModelBinder = new AuthenticationModelBinder(_authenticatorTypes);

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
		private readonly IEnumerable<IAuthenticationType> _types;

		public AuthenticationModelBinder(IEnumerable<IAuthenticationType> types)
		{
			_types = types;
		}

		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			var typeString = bindingContext.ValueProvider.GetValue("type").AttemptedValue;
			var type = _types.SingleOrDefault(t => t.TypeString == typeString);
			if (type == null)
				throw new NotImplementedException("Authentication type " + typeString + " not found");
			return type.BindModel(bindingContext);
		}
	}

	public interface IAuthenticationType
	{
		string TypeString { get; }
		IEnumerable<IDataSource> DataSources();
		IAuthenticationModel BindModel(ModelBindingContext bindingContext);
	}

	public class ApplicationAuthenticationType : IAuthenticationType
	{
		private readonly Lazy<IAuthenticator> _authenticator;
		private readonly Lazy<IDataSourcesProvider> _dataSourcesProvider;

		public ApplicationAuthenticationType(Lazy<IAuthenticator> authenticator, Lazy<IDataSourcesProvider> dataSourcesProvider)
		{
			_authenticator = authenticator;
			_dataSourcesProvider = dataSourcesProvider;
		}

		public string TypeString { get { return "application"; } }

		public IEnumerable<IDataSource> DataSources()
		{
			return _dataSourcesProvider.Value.RetrieveDatasourcesForApplication();
		}

		public IAuthenticationModel BindModel(ModelBindingContext bindingContext)
		{
			return new ApplicationAuthenticationModel(_authenticator.Value)
			       	{
			       		UserName = bindingContext.ValueProvider.GetValue("username").AttemptedValue,
			       		Password = bindingContext.ValueProvider.GetValue("password").AttemptedValue,
			       		DataSourceName = bindingContext.ValueProvider.GetValue("datasource").AttemptedValue
			       	};
		}
	}

	public class WindowsAuthenticationType : IAuthenticationType
	{
		private readonly Lazy<IAuthenticator> _authenticator;
		private readonly Lazy<IDataSourcesProvider> _dataSourcesProvider;

		public WindowsAuthenticationType(Lazy<IAuthenticator> authenticator, Lazy<IDataSourcesProvider> dataSourcesProvider)
		{
			_authenticator = authenticator;
			_dataSourcesProvider = dataSourcesProvider;
		}

		public string TypeString { get { return "windows"; } }

		public IEnumerable<IDataSource> DataSources()
		{
			return _dataSourcesProvider.Value.RetrieveDatasourcesForWindows();
		}

		public IAuthenticationModel BindModel(ModelBindingContext bindingContext)
		{
			return new WindowsAuthenticationModel(_authenticator.Value)
			       	{
			       		DataSourceName = bindingContext.ValueProvider.GetValue("datasource").AttemptedValue
			       	};
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