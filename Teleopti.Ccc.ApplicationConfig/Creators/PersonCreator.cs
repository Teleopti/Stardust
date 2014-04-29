using System;
using System.Diagnostics;
using System.Globalization;
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
		private readonly SetChangeInfoCommand _setChangeInfoCommand = new SetChangeInfoCommand();

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
        public IPerson Create(string firstName, string lastName, string logOnName, string password, CultureInfo cultureInfo, TimeZoneInfo timeZone)
        {
			ISession session = _sessionFactory.OpenSession();

			var person = session.CreateCriteria<IPerson>()
						.Add(Restrictions.Eq("ApplicationAuthenticationInfo.ApplicationLogOnName", logOnName))
						.SetFetchMode("PermissionInformation.PersonInApplicationRole", FetchMode.Join)
						.UniqueResult<IPerson>();
			if (person == null)
			{
				person = new Person {Name = new Name(firstName, lastName)};

				person.PermissionInformation.SetDefaultTimeZone(timeZone);
				person.PermissionInformation.SetCulture(cultureInfo);
				person.PermissionInformation.SetUICulture(cultureInfo);
				person.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo
				                                       	{
				                                       		ApplicationLogOnName = logOnName,
				                                       		Password = password
				                                       	};


				_setChangeInfoCommand.Execute((AggregateRoot) person, person);
				_setChangeInfoCommand.Execute((PersonWriteProtectionInfo)person.PersonWriteProtection, person);
			}
			
			foreach (var applicationRole in person.PermissionInformation.ApplicationRoleCollection)
			{
				Debug.Write(applicationRole.Name);
			}

        	session.Close();

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
        public void Save(IPerson person)
        {
            if (personNotSavedBefore(person))
            {
				ISession session = _sessionFactory.OpenSession();
                session.Save(person);
				session.Flush();
				session.Close();
            }
        }

    	private static bool personNotSavedBefore(IPerson person)
    	{
    		return !person.Id.HasValue;
    	}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool WindowsUserExists(IAuthenticationInfo windowsAuthInfo)
    	{
			ISession session = _sessionFactory.OpenSession();

			var person = session.CreateCriteria<IPerson>()
						.Add(Restrictions.Eq("AuthenticationInfo.Identity", windowsAuthInfo.Identity))
						.UniqueResult<IPerson>();

    		session.Close();

    		return person != null;
    	}
    }
}