using System;
using System.Collections.Specialized;
using System.Web;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker
{
	public class UserDataFactory : IUserDataFactory
	{
		private readonly ICurrentBusinessUnit _businessUnitProvider;
		private readonly Func<IDataSource> _dataSource;
		private readonly IConfigReader _configReader;
		private readonly ILoggedOnUser _loggedOnUser;
		public const string MessageBrokerUrlKey = "MessageBroker";
		public static bool EnableMyTimeMessageBroker = true;
		private const string DummyHostName = "dummy";

		public UserDataFactory(ICurrentBusinessUnit businessUnitProvider, 
												Func<IDataSource> dataSource, 
												IConfigReader configReader, 
												ILoggedOnUser loggedOnUser)
		{
			_businessUnitProvider = businessUnitProvider;
			_dataSource = dataSource;
			_configReader = configReader;
			_loggedOnUser = loggedOnUser;
		}

		public UserData CreateViewModel(HttpRequestBase context)
		{
			var currentBu = _businessUnitProvider.Current();
			var appSettings = _configReader.AppSettings;
			var loggedOnUser = _loggedOnUser.CurrentUser();
			var userData = new UserData();
			if (currentBu != null)
				userData.BusinessUnitId = currentBu.Id.Value;
			userData.DataSourceName = _dataSource().DataSourceName;
			if (appSettings != null)
				userData.Url = EnableMyTimeMessageBroker ? replaceDummyHostName(context, appSettings) : "http://disabledmessagebroker/";
			if (loggedOnUser != null)
				userData.AgentId = loggedOnUser.Id.Value;
			return userData;
		}

		private static string replaceDummyHostName(HttpRequestBase context, NameValueCollection appSettings)
		{
			var url = appSettings[MessageBrokerUrlKey];
			if (!url.Contains(DummyHostName)) return url;

			var uri = new Uri(url);
			var site = new Uri(context.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped));
			return new Uri(site,
				new Uri(uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)).MakeRelativeUri(uri)).ToString();
		}
	}
}