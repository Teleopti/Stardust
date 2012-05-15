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
    public class ApplicationRoleCreator
    {
        private readonly IPerson _person;
        private readonly IBusinessUnit _businessUnit;
        private readonly ISessionFactory _sessionFactory;
		private readonly SetChangeInfoCommand _setChangeInfoCommand = new SetChangeInfoCommand();

        public ApplicationRoleCreator(IPerson person, IBusinessUnit businessUnit, ISessionFactory sessionFactory)
        {
            _person = person;
            _businessUnit = businessUnit;
            _sessionFactory = sessionFactory;
        }

        /// <summary>
        /// Creates the specified application role name.
        /// </summary>
        /// <param name="applicationRoleName">Name of the application role.</param>
        /// <param name="description">The description.</param>
        /// <param name="builtIn">if set to <c>true</c> [built in].</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-10-29
        /// </remarks>
        public IApplicationRole Create(string applicationRoleName, string description, bool builtIn)
        {
            IApplicationRole applicationRole = new ApplicationRole { Name = applicationRoleName, BuiltIn = builtIn, DescriptionText = description };

			_setChangeInfoCommand.Execute((AggregateRoot)applicationRole,_person);
            typeof(ApplicationRole)
                .GetField("_businessUnit", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(applicationRole, _businessUnit);

            return applicationRole;
        }

        /// <summary>
        /// Saves the specified application role.
        /// </summary>
        /// <param name="applicationRole">The application role.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-10-29
        /// </remarks>
        public void Save(IApplicationRole applicationRole)
        {
            ISession session = _sessionFactory.OpenSession();
            session.Save(applicationRole);
            session.Flush();
            session.Close();
        }
    }
}