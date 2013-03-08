using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for templates regarding extended preferences
    /// </summary>
    public class ExtendedPreferenceTemplateRepository : Repository<IExtendedPreferenceTemplate>, IExtendedPreferenceTemplateRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedPreferenceTemplateRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public ExtendedPreferenceTemplateRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

		public ExtendedPreferenceTemplateRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
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