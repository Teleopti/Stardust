using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory
{
	public interface IRequestsShiftTradebulletinViewModelFactory
	{
		ShiftTradeScheduleViewModel CreateShiftTradeBulletinViewModel(ShiftTradeScheduleViewModelDataForAllTeams data);
	}
}