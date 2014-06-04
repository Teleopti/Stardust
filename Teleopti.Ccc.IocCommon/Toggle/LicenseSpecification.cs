using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Toggle.Net.Specifications;

namespace Teleopti.Ccc.IocCommon.Toggle
{
	public class LicenseSpecification : IToggleSpecification
	{
		private readonly ILicenseActivatorProvider _licenseActivatorProvide;
		public const string NameKey = "name";

		public LicenseSpecification(ILicenseActivatorProvider licenseActivatorProvide)
		{
			_licenseActivatorProvide = licenseActivatorProvide;
		}

		public bool IsEnabled(string currentUser, IDictionary<string, string> parameters)
		{
			var licenseCustomerName = _licenseActivatorProvide.Current().CustomerName;
			return licenseCustomerName.Equals(parameters[NameKey], StringComparison.OrdinalIgnoreCase) ||
					ToggleNetModule.DeveloperLicenseName.Equals(licenseCustomerName);
		}

		public void Validate(string toggleName, IDictionary<string, string> parameters)
		{
			if (!parameters.ContainsKey(NameKey))
			{
				throw new InvalidSpecificationParameterException(string.Format("Parameter '{0}' must be set for toggle '{1}'!", NameKey, toggleName));
			}
		}
	}
}