using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for alarm types
    /// </summary>
    public class AlarmTypeRepository : Repository<IAlarmType>, IAlarmTypeRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlarmTypeRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public AlarmTypeRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

	    public AlarmTypeRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
	    {
	    }
    }
}