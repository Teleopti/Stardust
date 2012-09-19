using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker
{
	public class UserDataFactory : IUserDataFactory
	{
		private readonly ICurrentBusinessUnitProvider _businessUnitProvider;

		public UserDataFactory(ICurrentBusinessUnitProvider businessUnitProvider)
		{
			_businessUnitProvider = businessUnitProvider;
		}

		public UserData CreateViewModel()
		{
			var ret = new UserData();
			ret.BusinessUnitId = _businessUnitProvider.CurrentBusinessUnit().Id.Value;
			return ret;
		}
	}
}