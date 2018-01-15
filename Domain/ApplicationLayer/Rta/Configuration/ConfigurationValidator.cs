using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Configuration
{
	public class ConfigurationValidator : IConfigurationValidator
	{
		private readonly IRtaStateGroupRepository _stateGroups;
		private readonly IBusinessUnitRepository _businessUnits;
		private readonly StateMapper _stateMapper;

		public ConfigurationValidator(IRtaStateGroupRepository stateGroups, IBusinessUnitRepository businessUnits, StateMapper stateMapper)
		{
			_stateGroups = stateGroups;
			_businessUnits = businessUnits;
			_stateMapper = stateMapper;
		}

		public IEnumerable<ConfigurationValidationViewModel> Validate()
		{
			var stateGroups = _stateGroups.LoadAll();

			var messages = from businessUnit in _businessUnits.LoadAll()
				let valid = stateGroups.Where(x => x.BusinessUnit == businessUnit).Any(x => x.IsLogOutState)
				where !valid
				select new ConfigurationValidationViewModel
				{
					Resource = nameof(UserTexts.Resources.LoggedOutStateGroupMissingInConfiguration),
					Data = new[] {businessUnit.Name}
				};

			_stateMapper.Refresh();
			if (_stateMapper.LoggedOutStateGroupIds().IsEmpty())
				messages = messages.Append(new ConfigurationValidationViewModel
				{
					Resource = nameof(UserTexts.Resources.LoggedOutStateGroupMissingInRtaService)
				});

			return messages.ToArray();
		}
	}
}