using System;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface IPersonScheduleDayViewModelFactory
	{
		PersonScheduleDayViewModel CreateViewModel(Guid personId, DateTime date);
	}
}