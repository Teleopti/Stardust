using System;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IShiftExchangeOfferPersister
	{
		Guid Persist(ShiftExchangeOfferForm form, ShiftExchangeOfferStatus status);
		Guid Persist(ShiftExchangeOffer offer);
	}
}