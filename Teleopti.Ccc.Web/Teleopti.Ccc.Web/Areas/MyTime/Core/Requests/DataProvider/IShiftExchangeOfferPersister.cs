using System;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IShiftExchangeOfferPersister
	{
		Guid Persist(ShiftExchangeOfferForm form);
	}
}