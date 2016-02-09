using System;
using System.Diagnostics;
using System.Globalization;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Security;
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

		public IPerson GetTheSystemUser()
		{
			var systemId = SystemUser.Id_AvoidUsing_This;
			
			 ISession session = _sessionFactory.OpenSession();
			 var person = session.CreateCriteria<IPerson>()
					  .Add(Restrictions.Eq("Id", systemId))
					  .SetFetchMode("PermissionInformation.PersonInApplicationRole", FetchMode.Join)
					  .UniqueResult<IPerson>();
			 foreach (var applicationRole in person.PermissionInformation.ApplicationRoleCollection)
			 {
				 Debug.Write(applicationRole.Name);
			 }
			session.Close();

				return person;
			
		}
		public IPerson Create(string firstName, string lastName, CultureInfo cultureInfo, TimeZoneInfo timeZone)
		{
			var person = new Person { Name = new Name(firstName, lastName) };
			person.PermissionInformation.SetDefaultTimeZone(timeZone);
			person.PermissionInformation.SetCulture(cultureInfo);
			person.PermissionInformation.SetUICulture(cultureInfo);


			_setChangeInfoCommand.Execute((AggregateRoot)person, person);
			_setChangeInfoCommand.Execute((PersonWriteProtectionInfo)person.PersonWriteProtection, person);

			foreach (var applicationRole in person.PermissionInformation.ApplicationRoleCollection)
			{
				Debug.Write(applicationRole.Name);
			}

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
	}
}