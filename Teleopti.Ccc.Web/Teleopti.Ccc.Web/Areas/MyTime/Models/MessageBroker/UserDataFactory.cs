using System;
using System.Collections.Specialized;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Core;
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

		public UserData CreateViewModel()
		{
			var currentBu = _businessUnitProvider.Current();
			var appSettings = _configReader.AppSettings;
			var loggedOnUser = _loggedOnUser.CurrentUser();
			var userData = new UserData();
			if (currentBu != null)
				userData.BusinessUnitId = currentBu.Id.Value;
			userData.DataSourceName = _dataSource().DataSourceName;
			if (appSettings != null)
				userData.Url = EnableMyTimeMessageBroker ? replaceDummyHostName(appSettings) : "http://disabledmessagebroker/";
			if (loggedOnUser != null)
				userData.AgentId = loggedOnUser.Id.Value;
			return userData;
		}

		private static string replaceDummyHostName(NameValueCollection appSettings)
		{
			var url = appSettings[MessageBrokerUrlKey];
			if (!appSettings.GetBoolSetting("UseRelativeConfiguration")) return url;

			var uri = new Uri(url);
			return "/" + new Uri(uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)).MakeRelativeUri(uri);
		}
	}
}