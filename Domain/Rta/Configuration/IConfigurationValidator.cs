using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Configuration
{
	public interface IConfigurationValidator
	{
		IEnumerable<ConfigurationValidationViewModel> Validate();
	}
}