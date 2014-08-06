using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public interface IAgentBadgeCalculator
	{
		IEnumerable<IPerson> Calculate(IStatelessUnitOfWork unitOfWork, IEnumerable<IPerson> allPersons, int timezoneId, DateTime date,
			AdherenceReportSettingCalculationMethod adherenceCalculationMethod, int silverBadgeRate, int goldBadgeRate);
	}
}