using System;
using System.Collections.Generic;
using NHibernate.Expression;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Time;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for agent contracts
    /// </summary>
    public class PersonContractRepository : Repository<PersonContract>, IPersonContractRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonContractRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public PersonContractRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        /// <summary>
        /// Find agent contracts for the given agents
        /// </summary>
        /// <param name="agents">Agents to search for agent contracts</param>
        /// <returns></returns>
        public ICollection<PersonContract> Find(IEnumerable<Person> agents)
        {
            InParameter.NotNull(agents, "agents");

            return Session.CreateCriteria(typeof(PersonContract), "agentContract")
                .Add(Expression.In("Person", new List<Person>(agents)))
                .List<PersonContract>();
        }

        /// <summary>
        /// Find agent contracts for the given agents
        /// </summary>
        /// <param name="agents">Agents to search for agent contracts</param>
        /// <param name="period">Date and time limit</param>
        /// <returns></returns>
        public ICollection<PersonContract> Find(IEnumerable<Person> agents, DateTimePeriod period)
        {
            InParameter.NotNull(agents, "agents");
            InParameter.NotNull(period, "period");

            IList<PersonContract> agentContracts = Session.CreateCriteria(typeof (PersonContract), "agentContract")
                .Add(Expression.In("Person", new List<Person>(agents)))
                .Add(Expression.Lt("StartDate", period.EndDateTime.Date))
                .Add(Expression.Or(Expression.Gt("TermDate", period.StartDateTime.Date), Expression.IsNull("TermDate")))
                .AddOrder(new Order("Person", true))
                .AddOrder(new Order("StartDate", false))
                .List<PersonContract>();

            DateTime currentEndDate = period.EndDateTime.Date;
            Person currentAgent = null;
            List<PersonContract> contractsToReturn = new List<PersonContract>();
            foreach (PersonContract contract in agentContracts)
            {
                if (currentAgent != contract.Person)
                {
                    currentEndDate = period.EndDateTime.Date;
                    currentAgent = contract.Person;
                }
                if (currentEndDate > period.StartDateTime.Date)
                    contractsToReturn.Add(contract);

                currentEndDate = contract.StartDate.Date.AddDays(-1);
            }

            return contractsToReturn;
        }
    }
}