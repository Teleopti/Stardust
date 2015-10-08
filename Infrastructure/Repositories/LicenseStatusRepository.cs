using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class LicenseStatusRepository : Repository<ILicenseStatus>, ILicenseStatusRepository
    {
        public LicenseStatusRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public LicenseStatusRepository(IUnitOfWorkFactory unitOfWorkFactory)
#pragma warning disable 618
            : base(unitOfWorkFactory)
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void AddRange(IEnumerable<ILicenseStatus> entityCollection)
        {
            if (entityCollection.Count() > 1)
                throw new DataSourceException("Attempted to add more than one status");
            if (entityCollection.Any())
            {
                foreach (ILicenseStatus licenseStat in entityCollection)
                {
                    Add(licenseStat);
                }
            }
        }

        public override bool ValidateUserLoggedOn
        {
            get
            {
                return false;
            }
        }
    }
}
