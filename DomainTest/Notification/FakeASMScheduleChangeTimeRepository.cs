using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.DomainTest.Notification
{
	public class FakeASMScheduleChangeTimeRepository : IASMScheduleChangeTimeRepository
	{
		private IList<ASMScheduleChangeTime> _times = new List<ASMScheduleChangeTime>();
		public void Add(ASMScheduleChangeTime time)
		{
			_times.Add(time);
		}

		public ASMScheduleChangeTime GetScheduleChangeTime(Guid personId)
		{
			return _times.SingleOrDefault(time => time.PersonId == personId);
		}

		public void Update(ASMScheduleChangeTime time)
		{
			
		}
	}
}