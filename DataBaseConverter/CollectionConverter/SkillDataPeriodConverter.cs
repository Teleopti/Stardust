using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// SkillDataPeriod converter
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2007-11-02
    /// </remarks>
    public class SkillDataPeriodConverter : CccConverter<SkillDataPeriod, global::Domain.SkillData>
    {
        private readonly IRepository<SkillDataPeriod> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillDataPeriodConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-02
        /// </remarks>
        public SkillDataPeriodConverter(IUnitOfWork unitOfWork, Mapper<SkillDataPeriod, global::Domain.SkillData> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new SkillDataPeriodRepository(unitOfWork);
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-02
        /// </remarks>
        public override IRepository<SkillDataPeriod> Repository
        {
            get { return _rep; }
        }
    }
}
