using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeScheduleTagSetter : IScheduleTagSetter
	{
		public void SetTagOnScheduleDays(ScheduleModifier modifier, IEnumerable<IScheduleDay> scheduleParts)
		{
			throw new NotImplementedException();
		}

		public void ChangeTagToSet(IScheduleTag tag)
		{

		}
	}
}