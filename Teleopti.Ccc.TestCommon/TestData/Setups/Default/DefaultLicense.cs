using System.IO;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Default
{
	public class DefaultLicense : IHashableDataSetup
	{
		private string _licenseText;

		private string licenseText
		{
			get { return _licenseText ?? (_licenseText = File.ReadAllText("Teleopti_RD.xml")); }
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var license = new License { XmlString = licenseText };
			var licenseRepository = LicenseRepository.DONT_USE_CTOR(currentUnitOfWork);
			licenseRepository.Add(license);
		}

		public int HashValue()
		{
			return licenseText.GetHashCode();
		}
	}
}
