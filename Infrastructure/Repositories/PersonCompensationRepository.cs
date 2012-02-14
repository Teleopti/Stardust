using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    ///<summary>
    /// Repository for PersonAssignment aggregate
    ///</summary>
    public class PersonCompensationRepository : Repository<PersonCompensation>, IPersonCompensationRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonCompensationRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The UnitOfWork.</param>
        public PersonCompensationRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        /// <summary>
        /// Finds the specified PersonCompensation.
        /// </summary>
        /// <param name="persons">The agents.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        public ICollection<PersonCompensation> Find(IEnumerable<Person> persons,
                                                   DateTimePeriod period)
        {
            InParameter.NotNull("persons", persons);

            return Session.CreateCriteria(typeof (PersonCompensation), "comp")
                .Add(Subqueries.Exists(GetAgentCompensationsInPeriod(period)
                                           .Add(Expression.In("Person", new List<Person>(persons)))))
                .List<PersonCompensation>();
        }

        /// <summary>
        /// Finds the specified AgentAbsences by the given Period param.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        public ICollection<PersonCompensation> Find(DateTimePeriod period)
        {
            return Session.CreateCriteria(typeof (PersonCompensation), "comp")
                .Add(Subqueries.Exists(GetAgentCompensationsInPeriod(period)))
                .List<PersonCompensation>();
        }


        private static DetachedCriteria GetAgentCompensationsInPeriod(DateTimePeriod period)
        {
            return DetachedCriteria.For<PersonCompensation>()
                .CreateAlias("LayerCollection", "layer")
                .SetProjection(Projections.Property("Id"))
                .Add(Property.ForName("Id").EqProperty("comp.Id"))
                .Add(Expression.Gt("layer.Period.Period.Maximum", period.StartDateTime))
                .Add(Expression.Lt("layer.Period.Period.Minimum", period.EndDateTime));
        }

        /// <summary>
        /// Gets the concrete type.
        /// Used when loading one instance by id
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-07-06
        /// </remarks>
        protected override Type ConcreteType
        {
            get { return typeof(PersonCompensation); }
        }
    }
}