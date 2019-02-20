using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for templates regarding extended preferences
    /// </summary>
    public class ExtendedPreferenceTemplateRepository : Repository<IExtendedPreferenceTemplate>, IExtendedPreferenceTemplateRepository
    {
	    public ExtendedPreferenceTemplateRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork, null, null)
	    {
		    
	    }

        public IList<IExtendedPreferenceTemplate> FindByUser(IPerson user)
        {
            var extendedPreferenceTemplates = Session.CreateCriteria(typeof(IExtendedPreferenceTemplate))
                        .Add(Restrictions.Eq("Person",user))
                        .AddOrder(Order.Asc("Name"))
                        .List<IExtendedPreferenceTemplate>();
            return extendedPreferenceTemplates;
        }

		public IExtendedPreferenceTemplate Find(Guid id)
		{
			var extendedPreferenceTemplate = Session.CreateCriteria(typeof(IExtendedPreferenceTemplate))
						.Add(Restrictions.Eq("Id", id))
						.UniqueResult<IExtendedPreferenceTemplate>();
			return extendedPreferenceTemplate;
		}
    }
}