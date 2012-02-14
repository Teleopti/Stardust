using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Persists and converts
    /// </summary>
    /// <typeparam name="TNew">The type of the new.</typeparam>
    /// <typeparam name="TOld">The type of the old.</typeparam>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 10/29/2007
    /// </remarks>
    public abstract class CccConverter<TNew, TOld> where TNew : IEntity
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (CccConverter<TNew, TOld>));
        private readonly IUnitOfWork _unitOfWork;
        private readonly Mapper<TNew, TOld> _mapper;


        /// <summary>
        /// Initializes a new instance of the <see cref="CccConverter&lt;TNew, TOld&gt;"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        protected CccConverter(IUnitOfWork unitOfWork, Mapper<TNew, TOld> mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Gets the unit of work.
        /// </summary>
        /// <value>The unit of work.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
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
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public Mapper<TNew, TOld> Mapper
        {
            get { return _mapper; }
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public abstract IRepository<TNew> Repository { get; }


        /// <summary>
        /// Called when [persisted].
        /// Can be used eg to put stuff in mappedobjectpair instance
        /// </summary>
        /// <param name="pairCollection">The pair collection.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        protected virtual void OnPersisted(ObjectPairCollection<TOld, TNew> pairCollection)
        {
        }

        /// <summary>
        /// Converts the persist.
        /// Override this method if not normal logic is used.
        /// </summary>
        /// <param name="entitiesToConvert">The entities to convert.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public virtual void ConvertAndPersist(IEnumerable<TOld> entitiesToConvert)
        {
            ObjectPairCollection<TOld, TNew> pairList = new ObjectPairCollection<TOld, TNew>();
            using(PerformanceOutput.ForOperation("Mappings"))
            {
                foreach (TOld theOld in entitiesToConvert)
                {
                    TNew theNew = Mapper.Map(theOld);

                    if (theNew != null)
                    {
                        IRestrictionChecker checkRestrictions = theNew as IRestrictionChecker;
                        if (checkRestrictions != null)
                        {
                            try
                            {
                                checkRestrictions.CheckRestrictions();
                            }
                            catch (ValidationException validationException)
                            {
                                //Skip this entity due to validation error
                                Logger.Warn("This entity was skipped due to validatino error.",validationException);
                                continue;
                            }
                        }
                        if (!theNew.Id.HasValue)
                            Repository.Add(theNew);
                        else
                            if (typeof(IAggregateRoot).IsInstanceOfType(theNew))
                                _unitOfWork.Merge((IAggregateRoot)theNew);

                        pairList.Add(theOld, theNew);
                    }
                }
            }
            using (PerformanceOutput.ForOperation("PersistAll"))
            {
                UnitOfWork.PersistAll();
            }
            
            OnPersisted(pairList);
        }
    }
}
