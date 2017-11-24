using System;
using Teleopti.Ccc.Domain.Notification;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IASMScheduleChangeTimeRepository
	{
		void Add(ASMScheduleChangeTime time);
		
		ASMScheduleChangeTime GetScheduleChangeTime(Guid personId);
		void Update(ASMScheduleChangeTime time);
	}
}
