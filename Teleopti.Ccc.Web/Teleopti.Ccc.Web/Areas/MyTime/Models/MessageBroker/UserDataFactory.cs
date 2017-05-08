using System;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
			var loggedOnUser = _loggedOnUser.CurrentUser();
			var userData = new UserData();
			if (currentBu != null)
				userData.BusinessUnitId = currentBu.Id.Value;
			userData.DataSourceName = _dataSource().DataSourceName;
			userData.Url = EnableMyTimeMessageBroker ? replaceDummyHostName() : "http://disabledmessagebroker/";
			if (loggedOnUser != null)
				userData.AgentId = loggedOnUser.Id.Value;
			return userData;
		}

		private string replaceDummyHostName()
		{
			var url = _configReader.AppConfig(MessageBrokerUrlKey);
			if (!_configReader.ReadValue("UseRelativeConfiguration", false)) return url;

			var uri = new Uri(url);
			return "/" + new Uri(uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)).MakeRelativeUri(uri);
		}
	}
}