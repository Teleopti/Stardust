using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    ///<summary>
    /// Repository for PersonTimeActivity aggregate
    ///</summary>
    public class PersonTimeActivityRepository : Repository<PersonTimeActivity>, IPersonTimeActivityRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonTimeActivityRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The UnitOfWork.</param>
        public PersonTimeActivityRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        /// <summary>
        /// Finds the specified PersonCompensation.
        /// </summary>
        /// <param name="agents">The agents.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        public ICollection<PersonTimeActivity> Find(IEnumerable<IPerson> agents,
                                                   DateTimePeriod period)
        {
            InParameter.NotNull("agents", agents);

            return Session.CreateCriteria(typeof (PersonTimeActivity), "timeact")
                .Add(Subqueries.Exists(GetAgentTimeActivitiesInPeriod(period)
                                           .Add(Expression.In("Person", new List<IPerson>(agents)))))
                .List<PersonTimeActivity>();
        }

        /// <summary>
        /// Finds the specified AgentAbsences by the given Period param.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        public ICollection<PersonTimeActivity> Find(DateTimePeriod period)
        {
            return Session.CreateCriteria(typeof (PersonTimeActivity), "timeact")
                .Add(Subqueries.Exists(GetAgentTimeActivitiesInPeriod(period)))
                .List<PersonTimeActivity>();
        }

        /// <summary>
        /// Finds the specified agents.
        /// </summary>
        /// <param name="agents">The agents.</param>
        /// <param name="period">The period.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-07-02
        /// </remarks>
        public ICollection<PersonTimeActivity> Find(IEnumerable<IPerson> agents, DateTimePeriod period, IScenario scenario)
        {
            InParameter.NotNull("agents", agents);

            return Session.CreateCriteria(typeof(PersonTimeActivity), "timeact")
                .Add(Subqueries.Exists(GetAgentTimeActivitiesInPeriodAndScenario(period, scenario)
                                           .Add(Expression.In("Person", new List<IPerson>(agents)))))
                .List<PersonTimeActivity>();
        }

        /// <summary>
        /// Finds the specified period.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-07-02
        /// </remarks>
        public ICollection<PersonTimeActivity> Find(DateTimePeriod period, IScenario scenario)
        {
            InParameter.NotNull("period", period);
            InParameter.NotNull("scenario", scenario);

            return Session.CreateCriteria(typeof(PersonTimeActivity), "timeact")
                .Add(Subqueries.Exists(GetAgentTimeActivitiesInPeriodAndScenario(period, scenario)))
                .List<PersonTimeActivity>();

        }

        /// <summary>
        /// Gets the agent time activities in period and scenario.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-07-02
        /// </remarks>
        private static DetachedCriteria GetAgentTimeActivitiesInPeriodAndScenario(DateTimePeriod period, IScenario scenario)
        {
            return DetachedCriteria.For<PersonTimeActivity>()
                .CreateAlias("LayerCollection", "timelayer")
                .SetProjection(Projections.Property("Id"))
                .Add(Property.ForName("Id").EqProperty("timeact.Id"))
                .Add(Expression.Gt("timelayer.Period.Period.Maximum", period.StartDateTime))
                .Add(Expression.Lt("timelayer.Period.Period.Minimum", period.EndDateTime))
                .Add(Expression.Eq("Scenario", scenario));
        }


        private static DetachedCriteria GetAgentTimeActivitiesInPeriod(DateTimePeriod period)
        {
            return DetachedCriteria.For<PersonTimeActivity>()
                .CreateAlias("LayerCollection", "timelayer")
                .SetProjection(Projections.Property("Id"))
                .Add(Property.ForName("Id").EqProperty("timeact.Id"))
                .Add(Expression.Gt("timelayer.Period.Period.Maximum", period.StartDateTime))
                .Add(Expression.Lt("timelayer.Period.Period.Minimum", period.EndDateTime));
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
            get { return typeof(PersonTimeActivity); }
        }
    }
}
