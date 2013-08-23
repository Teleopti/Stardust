using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using NHibernate;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for DayOffs
    /// </summary>
    public class PersonDayOffRepository : Repository<IPersonDayOff>, IPersonDayOffRepository
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonDayOffRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public PersonDayOffRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public PersonDayOffRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }

				public PersonDayOffRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }

        /// <summary>
        /// Loads the aggregate.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-06-25
        /// </remarks>
        public IPersonDayOff LoadAggregate(Guid id)
        {
            IPersonDayOff personDayOff = Session.CreateCriteria(typeof(PersonDayOff))
                        .Add(Restrictions.IdEq(id))
                        .UniqueResult<PersonDayOff>();

            return personDayOff;
        }

        /// <summary>
        /// Find days off for the given agents
        /// </summary>
        /// <param name="agents">Agents to search for days off</param>
        /// <returns></returns>
        public ICollection<IPersonDayOff> Find(IEnumerable<IPerson> agents)
        {
            ICollection<IPersonDayOff> retList = Session.CreateCriteria(typeof(PersonDayOff), "agentDayOff")
                .Add(Expression.In("Person", new List<IPerson>(agents)))
                .List<IPersonDayOff>();

            return retList;
        }

        /// <summary>
        /// Find days off for the given agents
        /// </summary>
        /// <param name="agents">Agents to search for days off</param>
        /// <param name="period">Date and time limit</param>
        /// <returns></returns>
        public ICollection<IPersonDayOff> Find(IEnumerable<IPerson> agents, DateTimePeriod period)
        {
            InParameter.NotNull("period", period);

            ICollection<IPersonDayOff> retList = Session.CreateCriteria(typeof (PersonDayOff), "AF")
                .Add(Expression.In("Person", new List<IPerson>(agents)))
                .Add(
                Expression.Between("DayOff.Anchor", period.StartDateTime, period.EndDateTime))
                .List<IPersonDayOff>();

            return retList;
        }

        /// <summary>
        /// Finds the specified agents.
        /// </summary>
        /// <param name="agents">The agents.</param>
        /// <param name="period">The period.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-03-06
        /// </remarks>
        public ICollection<IPersonDayOff> Find(IEnumerable<IPerson> agents, DateTimePeriod period,
                                              IScenario scenario)
        {
            InParameter.NotNull("period", period);
            InParameter.NotNull("scenario", scenario);

            var retList = new List<IPersonDayOff>();

            foreach (var agentList in agents.Batch(400))
            {
                retList.AddRange(Session.CreateCriteria(typeof(PersonDayOff), "AF")
                .Add(Expression.In("Person", agentList.ToArray()))
                .Add(Expression.And(
                    Expression.Between("DayOff.Anchor", period.StartDateTime, period.EndDateTime),
                    Expression.Eq("Scenario", scenario)
                         ))
                .List<IPersonDayOff>());
            }

            return retList;
        }


        /// <summary>
        /// Finds the specified period by date time period and scenario.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-12
        /// </remarks>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-12
        /// </remarks>
        public ICollection<IPersonDayOff> Find(DateTimePeriod period, IScenario scenario)
        {
            ICollection<IPersonDayOff> retList = Session.CreateCriteria(typeof(PersonDayOff), "AF")
                .Add(Expression.And(
                        Expression.Between("AF.DayOff.Anchor", period.StartDateTime, period.EndDateTime),
                        Expression.Eq("Scenario", scenario)))
                .List<IPersonDayOff>();


            return retList;
        }
    }
}
