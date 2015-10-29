using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IIntradaySkillStatusService
	{
		IEnumerable<SkillStatusModel> GetSkillStatusModels(DateTime now);
	}
}