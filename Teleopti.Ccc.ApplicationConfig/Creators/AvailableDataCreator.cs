using System;
using System.Reflection;
using NHibernate;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Creators
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2008-10-29
    /// </remarks>
    public class AvailableDataCreator
    {
        private readonly IPerson _person;
        private readonly ISessionFactory _sessionFactory;

        public AvailableDataCreator(IPerson person, ISessionFactory sessionFactory)
        {
            _person = person;
            _sessionFactory = sessionFactory;
        }

        /// <summary>
        /// Creates the specified application role.
        /// </summary>
        /// <param name="applicationRole">The application role.</param>
        /// <param name="availableDataRangeOption">The available data range option.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-10-29
        /// </remarks>
        public IAvailableData Create(IApplicationRole applicationRole, AvailableDataRangeOption availableDataRangeOption)
        {
            IAvailableData availableData = new AvailableData();
            availableData.ApplicationRole = applicationRole;
            availableData.AvailableDataRange = availableDataRangeOption;

            DateTime nu = DateTime.Now;
            typeof(AggregateRoot)
                .GetField("_createdBy", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(availableData, _person);
            typeof(AggregateRoot)
                .GetField("_createdOn", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(availableData, nu);
            typeof(AggregateRoot)
                .GetField("_updatedBy", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(availableData, _person);
            typeof(AggregateRoot)
                .GetField("_updatedOn", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(availableData, nu);

            return availableData;
        }

        /// <summary>
        /// Saves the specified available data.
        /// </summary>
        /// <param name="availableData">The available data.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-10-29
        /// </remarks>
        public void Save(IAvailableData availableData)
        {
            ISession session = _sessionFactory.OpenSession();
            session.Save(availableData);
            session.Flush();
            session.Close();
        }
    }
}