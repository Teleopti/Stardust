using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker
{
	public class UserDataFactory : IUserDataFactory
	{
		private readonly ICurrentBusinessUnitProvider _businessUnitProvider;
		private readonly IDataSource _dataSource;

		public UserDataFactory(ICurrentBusinessUnitProvider businessUnitProvider, 
									IDataSource dataSource)
		{
			_businessUnitProvider = businessUnitProvider;
			_dataSource = dataSource;
		}

		public UserData CreateViewModel()
		{
			var currentBu = _businessUnitProvider.CurrentBusinessUnit();
			var ret = new UserData();
			if(currentBu!=null)
				ret.BusinessUnitId = _businessUnitProvider.CurrentBusinessUnit().Id.Value;
			ret.DataSourceName = _dataSource.DataSourceName;
			return ret;
		}
	}
}