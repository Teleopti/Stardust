using System;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface IPersonScheduleViewModelFactory
	{
		PersonScheduleViewModel CreateViewModel(Guid personId, DateTime date);
	}
}