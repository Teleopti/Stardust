using System;
using System.Collections.Generic;
using Toggle.Net.Specifications;

namespace Teleopti.Ccc.IocCommon.Toggle
{
	public class LicenseSpecification : IToggleSpecification
	{
		private readonly string _licenseCustomerName;
		public const string NameKey = "name";

		public LicenseSpecification(string licenseCustomerName)
		{
			_licenseCustomerName = licenseCustomerName;
		}

		public bool IsEnabled(string currentUser, IDictionary<string, string> parameters)
		{
			return _licenseCustomerName.Equals(parameters[NameKey], StringComparison.OrdinalIgnoreCase) ||
					ToggleNetModule.DeveloperLicenseName.Equals(_licenseCustomerName);
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