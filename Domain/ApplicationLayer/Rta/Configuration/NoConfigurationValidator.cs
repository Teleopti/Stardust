using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Configuration
{
	public class NoConfigurationValidator : IConfigurationValidator
	{
		public IEnumerable<ConfigurationValidationViewModel> Validate()
		{
			return Enumerable.Empty<ConfigurationValidationViewModel>();
		}
	}
}