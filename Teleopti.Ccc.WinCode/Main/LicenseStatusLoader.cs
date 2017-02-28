using System.Linq;
using System.Xml.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Main
{
    public interface ILicenseStatusLoader
    {
        ILicenseStatusXml GetStatus(IUnitOfWork unitOfWork);
    }

    public class LicenseStatusLoader : ILicenseStatusLoader
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public LicenseStatusLoader(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public ILicenseStatusXml GetStatus(IUnitOfWork unitOfWork)
        {
            // if something goes wrong here the document is corrupt, handle that in some way ??
            var rep = _repositoryFactory.CreateLicenseStatusRepository(unitOfWork);
            var status = rep.LoadAll().First();
            return new LicenseStatusXml(XDocument.Parse(status.XmlString));
        }
    }
}