using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
	public interface IOpenAndSplitSkillCommand
	{
		void Execute(ISkill skill, DateOnlyPeriod period, IList<TimePeriod> openHoursList);
	}
}