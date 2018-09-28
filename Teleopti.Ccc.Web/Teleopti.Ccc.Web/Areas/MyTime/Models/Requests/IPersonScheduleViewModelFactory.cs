using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public interface IPersonScheduleViewModelFactory
	{
		PersonScheduleViewModel CreateViewModel(Guid personId, DateTime date);
	}
}