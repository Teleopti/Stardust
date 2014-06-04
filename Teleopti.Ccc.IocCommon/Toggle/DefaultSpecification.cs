using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Toggle.Net.Specifications;

namespace Teleopti.Ccc.IocCommon.Toggle
{
	public class DefaultSpecification : IToggleSpecification
	{
		private readonly ILicenseActivatorProvider _licenseActivatorProvider;

		public DefaultSpecification(ILicenseActivatorProvider licenseActivatorProvider)
		{
			_licenseActivatorProvider = licenseActivatorProvider;
		}

		public bool IsEnabled(string currentUser, IDictionary<string, string> parameters)
		{
			var customerName = _licenseActivatorProvider.Current().CustomerName;
			return ToggleNetModule.DeveloperLicenseName.Equals(customerName);
		}

		public void Validate(string toggleName, IDictionary<string, string> parameters)
		{
		}
	}
}