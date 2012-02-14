using System.Collections.Generic;
using NHibernate.Expression;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for agents
    /// </summary>
    public class AgentRepository : Repository<Agent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The UnitOfWork.</param>
        public AgentRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>
        /// Finds agent in organization, based on team.
        /// </summary>
        /// <param name="team">The team.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-19
        /// </remarks>
        public ICollection<Agent> FindInOrganization(Team team)
        {
            return null;
            //return Session.CreateCriteria(typeof (Agent), "agent")

            //    .CreateCriteria("TeamPeriodCollection")
            //        .Add(Expression.And(
            //            Expression.Eq("Id", team.Id) ,
            //            Expression.Eq("")
            //        )
            //        .Add(Expression.Eq("Id", team.Id))
            //    .List<Agent>();
        }

    }
}