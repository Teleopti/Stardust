using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IBadgeCalculationRepository
	{
		Dictionary<Guid, int> LoadAgentsOverThresholdForAnsweredCalls(string timezoneCode, DateTime date, int answeredCallsThreshold,
			Guid businessUnitId);

		Dictionary<Guid, double> LoadAgentsOverThresholdForAdherence(AdherenceReportSettingCalculationMethod adherenceCalculationMethod,
			string timezoneCode, DateTime date, Percent adherenceThreshold, Guid businessUnitId);

		Dictionary<Guid, double> LoadAgentsUnderThresholdForAht(string timezoneCode, DateTime date, TimeSpan ahtThreshold, Guid businessUnitId);
	}
}