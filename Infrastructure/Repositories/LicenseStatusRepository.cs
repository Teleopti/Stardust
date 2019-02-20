using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class LicenseStatusRepository : Repository<ILicenseStatus>, ILicenseStatusRepository
    {
		public LicenseStatusRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork, null, null)
		{ 
	    }

        public override void Add(ILicenseStatus entity)
        {
            if(!entity.Id.HasValue)
            {
                foreach (ILicenseStatus licenseStat in LoadAll())
                {
                    Session.Delete(licenseStat);
                }
            }
            base.Add(entity);
        }
    }
}
