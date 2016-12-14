using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface IPersonSkillsForScheduleDaysOvertimeProvider
	{
		IEnumerable<ISkill> Execute(IPersonPeriod personPeriod, IActivity activity);
	}
}