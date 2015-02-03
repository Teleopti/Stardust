using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.SSO.Models;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.SSO.Core
{
	public interface IApplicationAuthenticationType
	{
		string TypeString { get; }
		IEnumerable<IDataSource> DataSources();
		ApplicationAuthenticationModel BindModel(ModelBindingContext bindingContext);
	}

	public class ApplicationAuthenticationType : IApplicationAuthenticationType
	{
		private readonly Lazy<ISsoAuthenticator> _authenticator;
		private readonly Lazy<IDataSourcesProvider> _dataSourcesProvider;
		private readonly Lazy<ILogLogonAttempt> _logLogonAttempt;

		public ApplicationAuthenticationType(Lazy<ISsoAuthenticator> authenticator, Lazy<IDataSourcesProvider> dataSourcesProvider, Lazy<ILogLogonAttempt> logLogonAttempt)
		{
			_authenticator = authenticator;
			_dataSourcesProvider = dataSourcesProvider;
			_logLogonAttempt = logLogonAttempt;
		}

		public string TypeString { get { return "application"; } }

		public IEnumerable<IDataSource> DataSources()
		{
			return _dataSourcesProvider.Value.RetrieveDatasourcesForApplication();
		}

		public ApplicationAuthenticationModel BindModel(ModelBindingContext bindingContext)
		{
			return new ApplicationAuthenticationModel(_authenticator.Value, _logLogonAttempt.Value)
				{
					UserName = bindingContext.ValueProvider.GetValue("username").AttemptedValue,
					Password = bindingContext.ValueProvider.GetValue("password").AttemptedValue,
					DataSourceName = bindingContext.ValueProvider.GetValue("datasource").AttemptedValue
				};
		}
	}
}