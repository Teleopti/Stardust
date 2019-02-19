using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// MultiplicatorDefinitionSet Repository
    /// </summary>
    public class MultiplicatorDefinitionSetRepository : Repository<IMultiplicatorDefinitionSet>, 
                                                        IMultiplicatorDefinitionSetRepository, IProxyForId<IMultiplicatorDefinitionSet>
	{
#pragma warning disable 618
        public MultiplicatorDefinitionSetRepository(IUnitOfWork uow) : base(uow)
#pragma warning restore 618
        {
        }

				public MultiplicatorDefinitionSetRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork, null, null)
	    {
		    
	    }
		
    	public IList<IMultiplicatorDefinitionSet> FindAllShiftAllowanceDefinitions()
    	{
			IList<IMultiplicatorDefinitionSet> retList = Session.CreateCriteria(typeof(MultiplicatorDefinitionSet))
					  .Add(Restrictions.Eq("MultiplicatorType", MultiplicatorType.OBTime))
					  .List<IMultiplicatorDefinitionSet>();
			return retList;
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
