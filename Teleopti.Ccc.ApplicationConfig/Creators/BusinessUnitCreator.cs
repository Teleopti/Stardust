using System;
using System.Reflection;
using NHibernate;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Creators
{
    /// <summary>
    /// Creates and 
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2008-10-24
    /// </remarks>
    public class BusinessUnitCreator
    {
        private readonly IPerson _person;
        private readonly ISessionFactory _sessionFactory;

        public BusinessUnitCreator(IPerson person, ISessionFactory sessionFactory)
        {
            _person = person;
            _sessionFactory = sessionFactory;
        }

        /// <summary>
        /// Creates the specified business unit name.
        /// </summary>
        /// <param name="businessUnitName">Name of the business unit.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-10-24
        /// </remarks>
        public IBusinessUnit Create(string businessUnitName)
        {
            IBusinessUnit newBusinessUnit = new BusinessUnit(businessUnitName);

            DateTime nu = DateTime.Now;
            typeof(AggregateRoot)
                .GetField("_createdBy", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(newBusinessUnit, _person);
            typeof(AggregateRoot)
                .GetField("_createdOn", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(newBusinessUnit, nu);
            typeof(AggregateRoot)
                .GetField("_updatedBy", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(newBusinessUnit, _person);
            typeof(AggregateRoot)
                .GetField("_updatedOn", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(newBusinessUnit, nu);

            return newBusinessUnit;
        }

        /// <summary>
        /// Saves the specified new business unit.
        /// </summary>
        /// <param name="newBusinessUnit">The new business unit.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-10-24
        /// </remarks>
        public void Save(IBusinessUnit newBusinessUnit)
        {
            ISession session = _sessionFactory.OpenSession();
            session.Save(newBusinessUnit);
            session.Flush();
            session.Close();
        }
    }
}