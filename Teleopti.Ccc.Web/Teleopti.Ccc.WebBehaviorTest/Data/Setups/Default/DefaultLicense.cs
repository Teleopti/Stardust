﻿using System.IO;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default
{
	public class DefaultLicense : IHashableDataSetup
	{
		private string _licenseText;

		private string licenseText
		{
			get { return _licenseText ?? (_licenseText = File.ReadAllText("TestLicense.xml")); }
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var license = new License { XmlString = licenseText };
			var licenseRepository = new LicenseRepository(currentUnitOfWork);
			licenseRepository.Add(license);
		}

		public int HashValue()
		{
			return licenseText.GetHashCode();
		}
	}
}