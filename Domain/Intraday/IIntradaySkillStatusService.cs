using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IIntradaySkillStatusService
	{
		IEnumerable<SkillStatusModel> GetSkillStatusModels(DateTime now);
	}
}