using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Configuration
{
	public class ConfigurationValidator
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
			var stateGroups = _stateGroups.LoadAll().ToLookup(s => _businessUnits.Load(s.BusinessUnit.Value));
			var businessUnits = _businessUnits.LoadAll();

			return validateLoggedOutStateGroupInConfiguration(businessUnits, stateGroups)
				.Concat(validateDefaultStateGroupInConfiguration(businessUnits, stateGroups))
				.Concat(validateLoggedOutStateGroupInRtaService())
				.ToArray();
		}

		private static IEnumerable<ConfigurationValidationViewModel> validateDefaultStateGroupInConfiguration(IEnumerable<IBusinessUnit> businessUnits, ILookup<IBusinessUnit, IRtaStateGroup> stateGroups)
		{
			return from businessUnit in businessUnits
				let valid = stateGroups[businessUnit].Any(x => x.DefaultStateGroup)
				where !valid
				select new ConfigurationValidationViewModel
				{
					Resource = "DefaultStateGroupMissingInConfiguration",
					Data = new[] {businessUnit.Name}
				};
		}

		private static IEnumerable<ConfigurationValidationViewModel> validateLoggedOutStateGroupInConfiguration(IEnumerable<IBusinessUnit> businessUnits, ILookup<IBusinessUnit, IRtaStateGroup> stateGroups)
		{
			return from businessUnit in businessUnits
				let valid = stateGroups[businessUnit].Any(x => x.IsLogOutState)
				where !valid
				select new ConfigurationValidationViewModel
				{
					Resource = "LoggedOutStateGroupMissingInConfiguration",
					Data = new[] {businessUnit.Name}
				};
		}

		private IEnumerable<ConfigurationValidationViewModel> validateLoggedOutStateGroupInRtaService()
		{
			_stateMapper.Refresh();
			if (_stateMapper.LoggedOutStateGroupIds().IsEmpty())
				return new[]
				{
					new ConfigurationValidationViewModel
					{
						Resource = "LoggedOutStateGroupMissingInRtaService"
					}
				};
			return Enumerable.Empty<ConfigurationValidationViewModel>();
		}
	}
}