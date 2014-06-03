using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Toggle.Net.Specifications;

namespace Teleopti.Ccc.IocCommon.Toggle
{
	public class DefaultSpecification : IToggleSpecification
	{
		private readonly Func<ILicenseActivator> _licenseActivator;

		public DefaultSpecification(Func<ILicenseActivator> licenseActivator)
		{
			_licenseActivator = licenseActivator;
		}

		public bool IsEnabled(string currentUser, IDictionary<string, string> parameters)
		{
			var customerName = _licenseActivator().CustomerName;
			return ToggleNetModule.DeveloperLicenseName.Equals(customerName);
		}

		public void Validate(string toggleName, IDictionary<string, string> parameters)
		{
		}
	}
}