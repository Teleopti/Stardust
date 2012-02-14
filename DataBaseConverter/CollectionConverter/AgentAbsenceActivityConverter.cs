using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Converting agent days to intra day absences
    /// </summary>
    public class AgentAbsenceActivityConverter
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Mapper<IList<IPersonAbsence>, global::Domain.AgentDay> _mapper;
        private readonly IRepository<IPersonAbsence> _rep;


        /// <summary>
        /// Initializes a new instance of the <see cref="AgentAbsenceActivityConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-08-09
        /// </remarks>
        public AgentAbsenceActivityConverter(IUnitOfWork unitOfWork, Mapper<IList<IPersonAbsence>, global::Domain.AgentDay> mapper)
        {

            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _rep = new PersonAbsenceRepository(_unitOfWork);
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-08-09
        /// </remarks>
        public virtual IRepository<IPersonAbsence> Repository
        {
            get { return _rep;  }
        }

        /// <summary>
        /// Gets the unit of work.
        /// </summary>
        /// <value>The unit of work.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-08-09
        /// </remarks>
        public IUnitOfWork UnitOfWork
        {
            get { return _unitOfWork; }
        }

        /// <summary>
        /// Gets the mapper.
        /// </summary>
        /// <value>The mapper.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-08-09
        /// </remarks>
        public Mapper<IList<IPersonAbsence>, global::Domain.AgentDay> Mapper
        {
            get { return _mapper; }
        }

        /// <summary>
        /// Converts the and persist.
        /// </summary>
        /// <param name="entitiesToConvert">The entities to convert.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-08-09
        /// </remarks>
        public void ConvertAndPersist(IEnumerable<global::Domain.AgentDay> entitiesToConvert)
        {
            foreach (global::Domain.AgentDay oldAgentDay in entitiesToConvert)
            {
                IList<IPersonAbsence> retList = _mapper.Map(oldAgentDay);
                foreach (IPersonAbsence personAbsence in retList)
                   Repository.Add(personAbsence);
            }

            UnitOfWork.PersistAll();
        }
    }
}
