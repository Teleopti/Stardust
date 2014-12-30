using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public interface IShiftExchangeOfferMapper
	{
		IPersonRequest Map(ShiftExchangeOfferForm form, ShiftExchangeOfferStatus status);
	}
}