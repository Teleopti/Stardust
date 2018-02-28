using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Rta.Configuration
{
	public interface IConfigurationValidator
	{
		IEnumerable<ConfigurationValidationViewModel> Validate();
	}
}