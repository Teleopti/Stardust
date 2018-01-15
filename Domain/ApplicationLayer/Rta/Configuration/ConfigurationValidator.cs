using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Configuration
{
	public class ConfigurationValidator : IConfigurationValidator
	{
		private readonly IRtaStateGroupRepository _stateGroups;
		private readonly IBusinessUnitRepository _businessUnits;

		public ConfigurationValidator(IRtaStateGroupRepository stateGroups, IBusinessUnitRepository businessUnits)
		{
			_stateGroups = stateGroups;
			_businessUnits = businessUnits;
		}

		public IEnumerable<ConfigurationValidationViewModel> Validate()
		{
			var stateGroups = _stateGroups.LoadAll();

			var loggedOutStateGroupMissingMessages = from businessUnit in _businessUnits.LoadAll()
				let valid = stateGroups.Where(x => x.BusinessUnit == businessUnit).Any(x => x.IsLogOutState)
				where !valid
				select new ConfigurationValidationViewModel
				{
					Resource = nameof(UserTexts.Resources.LoggedOutStateGroupMissingInConfiguration),
					Data = new[] {businessUnit.Name}
				};

			return loggedOutStateGroupMissingMessages.ToArray();

//			var random = new Random().Next(0, 100);
//			if (random < 25)
//				throw new Exception();
//			if (random < 50)
//				return Enumerable.Empty<ConfigurationValidationViewModel>();
//			if (random < 75)
//				return new[]
//				{
//					new ConfigurationValidationViewModel
//					{
//						Resource = "LoggedOutStateGroupMissingInConfiguration",
//						Data = new[] {"Blip blop unit", _dataSource.CurrentName()}
//					}
//				};
//			return new[]
//			{
//				new ConfigurationValidationViewModel
//				{
//					Resource = "LoggedOutStateGroupMissingInConfiguration",
//					Data = new[] {"Blip blop unit", _dataSource.CurrentName()}
//				},
//				new ConfigurationValidationViewModel
//				{
//					Resource = "LoggedOutStateGroupMissingInRtaService",
//					Data = new[] {"Blip blop unit", _dataSource.CurrentName()}
//				},
//				new ConfigurationValidationViewModel
//				{
//					Resource = "DefaultStateGroupMissingInConfiguration",
//					Data = new[] {"Blip blop unit", _dataSource.CurrentName()}
//				},
//				new ConfigurationValidationViewModel
//				{
//					Resource = "DefaultStateGroupMissingInRtaService",
//					Data = new[] {"Blip blop unit", _dataSource.CurrentName()}
//				},
//			};
		}
	}
}