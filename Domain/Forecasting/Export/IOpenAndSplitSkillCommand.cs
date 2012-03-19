using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
	public interface IOpenAndSplitSkillCommand
	{
		void Execute(ISkill skill, DateOnlyPeriod period);
		void Execute(ISkill skill, DateOnlyPeriod period, IList<TimePeriod> openHoursList);
	}
}