using System.IO;
using log4net;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Default
{
	public class DefaultLicense : IHashableDataSetup
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(DefaultLicense));

		private string _licenseText;

		private string licenseText
		{
			get { return _licenseText ?? (_licenseText = File.ReadAllText("Teleopti_RD.xml")); }
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var license = new License { XmlString = licenseText };
			var licenseRepository = new LicenseRepository(currentUnitOfWork);
			licenseRepository.Add(license);
		}

		public int HashValue()
		{
			log.Debug("licenseText.GetHashCode() " + licenseText.GetHashCode());
			return licenseText.GetHashCode();
		}
	}
}
