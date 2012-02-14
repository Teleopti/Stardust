using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// MultiplicatorDefinitionSet Repository
    /// </summary>
    public class MultiplicatorDefinitionSetRepository : Repository<IMultiplicatorDefinitionSet>, 
                                                        IMultiplicatorDefinitionSetRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiplicatorDefinitionSetRepository"/> class.
        /// </summary>
        /// <param name="uow">The uow.</param>
        public MultiplicatorDefinitionSetRepository(IUnitOfWork uow) : base(uow)
        {
        }

        public MultiplicatorDefinitionSetRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }

        public IList<IMultiplicatorDefinitionSet> LoadAllSortByName()
        {
            return LoadAll();
        }

        public IList<IMultiplicatorDefinitionSet> FindAllDefinitions()
        {
            IList<IMultiplicatorDefinitionSet> retList = Session.CreateCriteria(typeof(MultiplicatorDefinitionSet))
                       .List<IMultiplicatorDefinitionSet>();

            foreach (var multiplicatorDefinitionSet in retList)
            {
                LazyLoadingManager.Initialize(multiplicatorDefinitionSet.DefinitionCollection);
                foreach (var multiplicatorDefinition in multiplicatorDefinitionSet.DefinitionCollection)
                {
                    LazyLoadingManager.Initialize(multiplicatorDefinition.Multiplicator);
                }
            }
            return retList;
        }

        public IList<IMultiplicatorDefinitionSet> FindAllOvertimeDefinitions()
        {
            IList<IMultiplicatorDefinitionSet> retList = Session.CreateCriteria(typeof(MultiplicatorDefinitionSet))
                       .Add(Restrictions.Eq("MultiplicatorType",MultiplicatorType.Overtime))
                       .List<IMultiplicatorDefinitionSet>();
            return retList;
        }
    }
}
