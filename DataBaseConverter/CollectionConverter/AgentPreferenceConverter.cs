using System.Collections.Generic;
using Domain;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Agent preference converter
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-12-02
    /// </remarks>
    public class AgentPreferenceConverter : CccConverter<IPreferenceDay, AgentDayPreference>
    {
        private readonly IRepository<IPreferenceDay> _rep;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentPreferenceConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-12-02
        /// </remarks>
        public AgentPreferenceConverter(IUnitOfWork unitOfWork, Mapper<IPreferenceDay, AgentDayPreference> mapper)
            : base(unitOfWork, mapper)
        {
            _unitOfWork = unitOfWork;
            _rep = new PreferenceDayRepository(_unitOfWork);
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-12-02
        /// </remarks>
        public override IRepository<IPreferenceDay> Repository
        {
            get { return _rep; }
        }
        /// <summary>
        /// Converts the old preference and persists the new personRestriction.
        /// Override this method if not normal logic is used.
        /// </summary>
        /// <param name="entitiesToConvert">The entities to convert.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-12-04
        /// </remarks>
        public override void ConvertAndPersist(IEnumerable<AgentDayPreference> entitiesToConvert)
        {
            //map old preferences to new prersonRestrictions
            foreach (AgentDayPreference day in entitiesToConvert)
            {
                if (day != null)
                {
                    IPreferenceDay personRestriction = Mapper.Map(day);
                    Repository.Add(personRestriction);
                }
            }
            _unitOfWork.PersistAll();
        }
        
    }
}
