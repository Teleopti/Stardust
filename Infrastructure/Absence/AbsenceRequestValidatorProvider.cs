using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Absence
{
	public class AbsenceRequestValidatorProvider : IAbsenceRequestValidatorProvider
	{
		private readonly IToggleManager _toggleManager;
		private readonly IGlobalSettingDataRepository _globalSettingsDataRepository;
		private readonly INow _now;

		public AbsenceRequestValidatorProvider(IToggleManager toggleManager
			, IGlobalSettingDataRepository globalSettingsDataRepository, INow now)
		{
			_toggleManager = toggleManager;
			_globalSettingsDataRepository = globalSettingsDataRepository;
			_now = now;
		}

		public IEnumerable<IAbsenceRequestValidator> GetValidatorList(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
		{
			var validators = absenceRequestOpenPeriod.GetSelectedValidatorList().ToList();

			if (_toggleManager.IsEnabled(Toggles.Wfm_Requests_Check_Expired_Requests_40274))
			{
				var requestExpirationValidator = new RequestExpirationValidator(_now, _globalSettingsDataRepository);
				if (!validators.Contains(requestExpirationValidator))
					validators.Add(requestExpirationValidator);
			}

			return validators;
		}
	}
}
