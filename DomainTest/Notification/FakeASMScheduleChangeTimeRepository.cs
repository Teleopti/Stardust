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


		public ASMScheduleChangeTime GetScheduleChangeTime(Guid personId)
		{
			return _times.SingleOrDefault(time => time.PersonId == personId);
		}

		public void Save(ASMScheduleChangeTime time)
		{
			var orignal = GetScheduleChangeTime(time.PersonId);
			if (orignal == null)
				_times.Add(time);
			else
				orignal.TimeStamp = time.TimeStamp;
		}
	}
}