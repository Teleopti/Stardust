using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker
{
	public class UserDataFactory : IUserDataFactory
	{
		private readonly ICurrentBusinessUnitProvider _businessUnitProvider;
		private readonly IDataSource _dataSource;
		private readonly IConfigReader _configReader;
		private readonly ILoggedOnUser _loggedOnUser;
		public const string MessageBrokerUrlKey = "MessageBroker";

		public UserDataFactory(ICurrentBusinessUnitProvider businessUnitProvider, 
												IDataSource dataSource, 
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
			var currentBu = _businessUnitProvider.CurrentBusinessUnit();
			var appSettings = _configReader.AppSettings;
			var loggedOnUser = _loggedOnUser.CurrentUser();
			var ret = new UserData();
			if(currentBu!=null)
				ret.BusinessUnitId = currentBu.Id.Value;
			ret.DataSourceName = _dataSource.DataSourceName;
			if(appSettings!=null)
				ret.Url = appSettings[MessageBrokerUrlKey];
			if(loggedOnUser!=null)
				ret.AgentId = loggedOnUser.Id.Value;
			return ret;
		}
	}
}