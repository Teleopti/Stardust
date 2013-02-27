using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeRequestMapper : IShiftTradeRequestMapper
	{
		public IPersonRequest Map(ShiftTradeRequestForm form)
		{
			var ret = new PersonRequest(new Person());
			ret.Subject = form.Subject;
			return ret;
		}
	}
}