using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class LicenseStatusRepository : Repository<ILicenseStatus>, ILicenseStatusRepository
    {
        public LicenseStatusRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }

				public LicenseStatusRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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
