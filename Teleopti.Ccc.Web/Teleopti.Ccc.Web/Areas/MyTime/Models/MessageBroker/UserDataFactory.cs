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
		public const string MessageBrokerUrlKey = "MessageBroker";

		public UserDataFactory(ICurrentBusinessUnitProvider businessUnitProvider, 
									IDataSource dataSource, 
									IConfigReader configReader)
		{
			_businessUnitProvider = businessUnitProvider;
			_dataSource = dataSource;
			_configReader = configReader;
		}

		public UserData CreateViewModel()
		{
			var currentBu = _businessUnitProvider.CurrentBusinessUnit();
			var appSettings = _configReader.AppSettings;
			var ret = new UserData();
			if(currentBu!=null)
				ret.BusinessUnitId = _businessUnitProvider.CurrentBusinessUnit().Id.Value;
			ret.DataSourceName = _dataSource.DataSourceName;
			if(appSettings!=null)
				ret.Url = appSettings[MessageBrokerUrlKey];
			return ret;
		}
	}
}