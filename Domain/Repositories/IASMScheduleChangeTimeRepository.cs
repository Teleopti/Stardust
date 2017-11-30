using System;
using Teleopti.Ccc.Domain.Notification;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IASMScheduleChangeTimeRepository
	{
		ASMScheduleChangeTime GetScheduleChangeTime(Guid personId);
		void Save(ASMScheduleChangeTime time);
	}
}
