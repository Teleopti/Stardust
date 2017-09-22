using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public interface IShiftExchangeOfferMapper
	{
		IPersonRequest Map(ShiftExchangeOfferForm form, ShiftExchangeOfferStatus status);
		IPersonRequest Map(ShiftExchangeOfferForm form, IPersonRequest request);
	}
}