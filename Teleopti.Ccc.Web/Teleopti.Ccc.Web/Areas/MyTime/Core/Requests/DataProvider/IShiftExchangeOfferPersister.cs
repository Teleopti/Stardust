using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IShiftExchangeOfferPersister
	{
		RequestViewModel Persist(ShiftExchangeOfferForm form, ShiftExchangeOfferStatus status);
	}
}