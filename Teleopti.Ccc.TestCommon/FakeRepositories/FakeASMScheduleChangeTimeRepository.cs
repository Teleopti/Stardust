using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeASMScheduleChangeTimeRepository : IASMScheduleChangeTimeRepository
	{
		private IList<ASMScheduleChangeTime> _times = new List<ASMScheduleChangeTime>();


		public ASMScheduleChangeTime GetScheduleChangeTime(Guid personId)
		{
			return _times.SingleOrDefault(time => time.PersonId == personId);
		}

		public void Save(params ASMScheduleChangeTime[] times)
		{
			times.ForEach(time =>
			{
				var orignal = GetScheduleChangeTime(time.PersonId);
				if (orignal == null)
					_times.Add(time);
				else
					orignal.TimeStamp = time.TimeStamp;
			});
		}
	}
}