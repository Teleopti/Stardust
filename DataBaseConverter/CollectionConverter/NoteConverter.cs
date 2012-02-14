using System.Collections.Generic;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Converts notes from 6.6 agentDay
    /// </summary>
    public class NoteConverter : CccConverter<INote, global::Domain.AgentDay>
    {
        private readonly IRepository<INote> _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        public NoteConverter(IUnitOfWork unitOfWork, Mapper<INote, global::Domain.AgentDay> mapper)
            : base(unitOfWork, mapper)
        {
            _repository = new NoteRepository(unitOfWork);
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public override IRepository<INote> Repository
        {
            get { return _repository; }
        }

    }
}
