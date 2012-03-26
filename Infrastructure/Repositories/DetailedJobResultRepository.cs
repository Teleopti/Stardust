using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public interface IDetailedJobResultRepository : IRepository<IDetailedJobResult>
    {
        
    }
    class DetailedJobResultRepository : Repository<IDetailedJobResult>, IDetailedJobResultRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public DetailedJobResultRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public DetailedJobResultRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }


        
    }
}
