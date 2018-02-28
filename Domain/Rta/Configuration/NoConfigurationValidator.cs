using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Rta.Configuration
{
	public class NoConfigurationValidator : IConfigurationValidator
	{
		public IEnumerable<ConfigurationValidationViewModel> Validate()
		{
			return Enumerable.Empty<ConfigurationValidationViewModel>();
		}
	}
}