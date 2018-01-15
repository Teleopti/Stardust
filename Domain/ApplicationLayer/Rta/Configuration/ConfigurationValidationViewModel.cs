using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Configuration
{
	public interface IConfigurationValidator
	{
		IEnumerable<ConfigurationValidationViewModel> Validate();
	}

	public class NoConfigurationValidator : IConfigurationValidator
	{
		public IEnumerable<ConfigurationValidationViewModel> Validate()
		{
			return Enumerable.Empty<ConfigurationValidationViewModel>();
		}
	}

	public class ConfigurationValidator : IConfigurationValidator
	{
		private readonly ICurrentDataSource _dataSource;

		public ConfigurationValidator(ICurrentDataSource dataSource)
		{
			_dataSource = dataSource;
		}

		public IEnumerable<ConfigurationValidationViewModel> Validate()
		{
			var random = new Random().Next(0, 100);
			if (random < 25)
				throw new Exception();
			if (random < 50)
				return Enumerable.Empty<ConfigurationValidationViewModel>();
			if (random < 75)
				return new[]
				{
					new ConfigurationValidationViewModel
					{
						Resource = "LoggedOutStateGroupMissingInConfiguration",
						Data = new[] {"Blip blop unit", _dataSource.CurrentName()}
					}
				};
			return new[]
			{
				new ConfigurationValidationViewModel
				{
					Resource = "LoggedOutStateGroupMissingInConfiguration",
					Data = new[] {"Blip blop unit", _dataSource.CurrentName()}
				},
				new ConfigurationValidationViewModel
				{
					Resource = "LoggedOutStateGroupMissingInRtaService",
					Data = new[] {"Blip blop unit", _dataSource.CurrentName()}
				},
				new ConfigurationValidationViewModel
				{
					Resource = "DefaultStateGroupMissingInConfiguration",
					Data = new[] {"Blip blop unit", _dataSource.CurrentName()}
				},
				new ConfigurationValidationViewModel
				{
					Resource = "DefaultStateGroupMissingInRtaService",
					Data = new[] {"Blip blop unit", _dataSource.CurrentName()}
				},
			};
		}
	}

	public class ConfigurationValidationViewModel
	{
		public string Resource;
		public IEnumerable<string> Data;
	}
}