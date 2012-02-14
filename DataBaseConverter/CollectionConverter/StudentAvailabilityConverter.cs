using System.Data;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    ///<summary>
    ///</summary>
    public class StudentAvailabilityConverter : CccConverter<IStudentAvailabilityDay, global::Domain.AgentDay>
    {
        private readonly IRepository<IStudentAvailabilityDay> _rep;

        ///<summary>
        /// Initializes a new instance of the <see cref="StudentAvailabilityConverter"/> class.
        ///</summary>
        ///<param name="unitOfWork"></param>
        ///<param name="mapper"></param>
        public StudentAvailabilityConverter(IUnitOfWork unitOfWork, Mapper<IStudentAvailabilityDay, global::Domain.AgentDay> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new StudentAvailabilityDayRepository(unitOfWork);
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        public override IRepository<IStudentAvailabilityDay> Repository
        {
            get { return _rep; }
        }

        /// <summary>
        /// Called when [persisted].
        /// Can be used eg to put stuff in mappedobjectpair instance
        /// </summary>
        /// <param name="pairCollection">The pair collection.</param>
        protected override void OnPersisted(ObjectPairCollection<global::Domain.AgentDay, IStudentAvailabilityDay> pairCollection)
        {
            Mapper.MappedObjectPair.StudentAvailabilityDay = pairCollection;
        }
    }
}
