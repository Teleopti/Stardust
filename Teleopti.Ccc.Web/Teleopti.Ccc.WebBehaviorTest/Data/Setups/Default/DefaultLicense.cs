using System.IO;
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
			get { return _licenseText ?? (_licenseText = File.ReadAllText("License.xml")); }
		}

		public void Apply(IUnitOfWork uow)
		{
			var license = new License { XmlString = licenseText };
			var licenseRepository = new LicenseRepository(uow);
			licenseRepository.Add(license);
		}

		public int HashValue()
		{
			return licenseText.GetHashCode();
		}
	}
}