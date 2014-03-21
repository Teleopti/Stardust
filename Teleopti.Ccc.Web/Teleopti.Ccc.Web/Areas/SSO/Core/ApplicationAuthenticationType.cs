using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.SSO.Core
{
	public class ApplicationAuthenticationType : IIdentityProviderAuthenticationType
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

	public interface IIdentityProviderAuthenticationType
	{
		string TypeString { get; }
		IEnumerable<IDataSource> DataSources();
		IAuthenticationModel BindModel(ModelBindingContext bindingContext);
	}
}