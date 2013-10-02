using System.Linq;
using System.Xml.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Main
{
    public interface ILicenseStatusLoader
    {
        ILicenseStatusXml GetStatus();
    }

    public class LicenseStatusLoader : ILicenseStatusLoader
    {
        private readonly ILicenseStatusRepository _licenseStatusRepository;

        public LicenseStatusLoader( ILicenseStatusRepository licenseStatusRepository)
        {
            _licenseStatusRepository = licenseStatusRepository;
        }

        public ILicenseStatusXml GetStatus()
        {
            // if something goes wrong here the document is corrupt, handle that in some way ??
            var status = _licenseStatusRepository.LoadAll().First();
            return new LicenseStatusXml(XDocument.Parse(status.XmlString));
        }
    }
}