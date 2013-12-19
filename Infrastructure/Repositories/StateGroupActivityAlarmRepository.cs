using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for state groups / activities alarm combinations
    /// </summary>
    public class StateGroupActivityAlarmRepository : Repository<IStateGroupActivityAlarm>, IStateGroupActivityAlarmRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateGroupActivityAlarmRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public StateGroupActivityAlarmRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

				public StateGroupActivityAlarmRepository(IUnitOfWorkFactory currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }

	    public StateGroupActivityAlarmRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
	    {
	    }

	    /// <summary>
        /// Loads all complete graph.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-21
        /// </remarks>
        public IList<IStateGroupActivityAlarm> LoadAllCompleteGraph()
        {
            return Session.CreateCriteria(typeof(StateGroupActivityAlarm))
                .SetFetchMode("Activity", FetchMode.Join)
                .SetFetchMode("StateGroup", FetchMode.Join)
                .SetFetchMode("AlarmType", FetchMode.Join)
                .SetResultTransformer(Transformers.DistinctRootEntity)
                .List<IStateGroupActivityAlarm>();
        }
    }
}