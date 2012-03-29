using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
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
    /// Created date: 2008-10-24
    /// </remarks>
    public class PersonCreator
    {
        private readonly ISessionFactory _sessionFactory;

        public PersonCreator(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        /// <summary>
        /// Creates the specified first name.
        /// </summary>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="logOnName">Name of the LogOn.</param>
        /// <param name="password">The password.</param>
        /// <param name="cultureInfo">The culture info.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-10-24
        /// </remarks>
        public IPerson Create(string firstName, string lastName, string logOnName, string password, CultureInfo cultureInfo, ICccTimeZoneInfo timeZone)
        {
            IPerson person = new Person {Name = new Name(firstName, lastName)};

            person.PermissionInformation.SetDefaultTimeZone(timeZone);
            person.PermissionInformation.SetCulture(cultureInfo);
            person.PermissionInformation.SetUICulture(cultureInfo);
            person.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo
                                                                             {
                                                                                 ApplicationLogOnName = logOnName,
                                                                                 Password = password
                                                                             };

            DateTime nu = DateTime.Now;
            typeof(AggregateRoot)
                .GetField("_createdBy", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(person, person);
            typeof(AggregateRoot)
                .GetField("_createdOn", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(person, nu);
            typeof(AggregateRoot)
                .GetField("_updatedBy", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(person, person);
            typeof(AggregateRoot)
                .GetField("_updatedOn", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(person, nu);

            typeof(PersonWriteProtectionInfo)
                .GetField("_createdBy", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(person.PersonWriteProtection, person);
            typeof(PersonWriteProtectionInfo)
                .GetField("_createdOn", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(person.PersonWriteProtection, nu);
            typeof(PersonWriteProtectionInfo)
                .GetField("_updatedBy", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(person.PersonWriteProtection, person);
            typeof(PersonWriteProtectionInfo)
                .GetField("_updatedOn", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(person.PersonWriteProtection, nu);
            return person;
        }

        /// <summary>
        /// Saves the specified person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-10-24
        /// </remarks>
        public bool Save(IPerson person)
        {
            bool personSaved = false;
            ISession session = _sessionFactory.OpenSession();

            IPerson foundPerson = (IPerson)session.CreateCriteria(typeof (IPerson))
                .Add(Restrictions.Eq("ApplicationAuthenticationInfo.ApplicationLogOnName",
                                   person.ApplicationAuthenticationInfo.ApplicationLogOnName))
                .UniqueResult();

            if (foundPerson == null)
            {
                session.Save(person);
                session.Flush();
                personSaved = true;
            }

            session.Close();
            return personSaved;
        }

        public IPerson FetchPerson(string applicationLogOnName)
        {
            ISession session = _sessionFactory.OpenSession();
            
            IPerson person = session.CreateCriteria(typeof(IPerson))
                        .Add(Restrictions.Eq("ApplicationAuthenticationInfo.ApplicationLogOnName", applicationLogOnName))
                        .SetFetchMode("PermissionInformation.PersonInApplicationRole", FetchMode.Join)
                        .UniqueResult<IPerson>();
            foreach (var applicationRole in person.PermissionInformation.ApplicationRoleCollection)
            {
                Debug.Write(applicationRole.Name);
            }
            session.Close();
            return person;
        }
    }
}