using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class IntradaySkillStatusService : IIntradaySkillStatusService
	{
		public void GetSkillStatus()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<KeyValuePair<ISkill, IList<SkillTaskDetails>>> GetForecastedTasks()
		{
			throw new NotImplementedException();
		}
	}
}