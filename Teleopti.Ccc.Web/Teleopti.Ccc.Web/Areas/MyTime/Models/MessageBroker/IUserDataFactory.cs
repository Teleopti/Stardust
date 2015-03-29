using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker
{
	public interface IUserDataFactory
	{
		UserData CreateViewModel(DateTime date);
	}
}