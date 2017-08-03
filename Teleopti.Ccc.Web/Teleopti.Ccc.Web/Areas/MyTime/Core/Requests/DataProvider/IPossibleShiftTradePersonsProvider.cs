using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IPossibleShiftTradePersonsProvider
	{
		DatePersons RetrievePersons(ShiftTradeScheduleViewModelData shiftTradeArguments);

		DatePersons RetrievePersons(ShiftTradeScheduleViewModelData shiftTradeArguments,Guid[] personIds);
	}
}