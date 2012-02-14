using System;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Task period converter
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2007-10-31
    /// </remarks>
    public class TaskPeriodConverter : CccConverter<TaskPeriod, global::Domain.ForecastData>
    {
        private readonly IRepository<TaskPeriod> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskPeriodConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-10-31
        /// </remarks>
        public TaskPeriodConverter(IUnitOfWork unitOfWork, Mapper<TaskPeriod, global::Domain.ForecastData> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new TaskPeriodRepository(unitOfWork);
        }


        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public override IRepository<TaskPeriod> Repository
        {
            get { return _rep; }
        }
    }
}
