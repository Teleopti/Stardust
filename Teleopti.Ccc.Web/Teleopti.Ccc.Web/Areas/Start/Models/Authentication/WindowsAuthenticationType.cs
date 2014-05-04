using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Models.Authentication
{
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
			return _dataSourcesProvider.Value.RetrieveDatasourcesForIdentity();
		}

		public IAuthenticationModel BindModel(ModelBindingContext bindingContext)
		{
			return new WindowsAuthenticationModel(_authenticator.Value)
				{
					DataSourceName = bindingContext.ValueProvider.GetValue("datasource").AttemptedValue
				};
		}
	}
}