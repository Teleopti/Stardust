using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Analytics.Transformer
{
	public interface IPersonPeriodTransformer
	{
		AnalyticsPersonPeriod Transform(IPerson person, IPersonPeriod personPeriod, out List<AnalyticsSkill> analyticsSkills);
	}
}