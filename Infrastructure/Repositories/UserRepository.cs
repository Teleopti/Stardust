using System.Collections.Generic;
using NHibernate;
using NHibernate.Expression;
using NHibernate.SqlCommand;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for User entity
    /// </summary>
    public class UserRepository : Repository<User>, IUserRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The uow.</param>
        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>
        /// Tries to find a basic authenticated user.
        /// </summary>
        /// <param name="logOnName">The logOnName.</param>
        /// <param name="password">The password.</param>
        /// <param name="foundUser">The found user.</param>
        /// <returns></returns>
        public bool TryFindBasicAuthenticatedUser(string logOnName, string password, out User foundUser)
        {
            foundUser = createLogonNameCriteria(logOnName)
                .Add(Expression.Eq("ai.Password", password))
                .UniqueResult<User>();

            if (foundUser == null)
            {
                return false;
            }
            else
            {
                temporarlyMethodUntilBugInHibernateIsFixed(foundUser);
                return true;
            }
        }

        /// <summary>
        /// Tries to find a windows authenticated user.
        /// </summary>
        /// <param name="logOnName">Name of the log on.</param>
        /// <param name="domainName">Name of the domain.</param>
        /// <param name="foundUser">The found user.</param>
        /// <returns></returns>
        public bool TryFindWindowsAuthenticatedUser(string domainName, string logOnName, out User foundUser)
        {
            foundUser = createLogonNameCriteria(logOnName)
                .Add(Expression.Eq("ai.DomainName", domainName))
                .UniqueResult<User>();
            if(foundUser==null)
            {
                return false;
            }
            else
            {
                temporarlyMethodUntilBugInHibernateIsFixed(foundUser);
                return true;
            }
        }


        /// <summary>
        /// Temporarlies the method until hibernate is fixed.
        /// Problems when lazy loading is used combined with inverse=true (?)
        /// </summary>
        /// <param name="user">The user.</param>
        /// <remarks>
        /// I think this is a bug. Should be fixed using NHibernateUtil.Initialize() instead.
        /// http://jira.nhibernate.org/browse/NH-1198
        /// Might also be fixed in the query itself. Continue tomorrow...
        /// Created by: rogerkr
        /// Created date: 2007-11-13
        /// </remarks>
        private static void temporarlyMethodUntilBugInHibernateIsFixed(User user)
        {
            int dummyCounter;
            dummyCounter = user.ApplicationRoleCollection.Count;
            foreach (BusinessUnit bu in user.BusinessUnitAccessCollection)
            {
                dummyCounter+=bu.TeamCollection().Count;  
            } 
        }   

        private ICriteria createLogonNameCriteria(string logOnName)
        {

            //todo
            //should be case sensitive in password but not on user name
            //what if sql server instance is case insensitive - should the username still be incasesensitive?
            return Session.CreateCriteria(typeof (User), "user")
                .CreateAlias("AuthenticationInfoCollection", "ai")
                .Add(Expression.Eq("ai.LogOnName", logOnName));
        }


        /// <summary>
        /// Gets a value indicating if user must be logged in to use repository.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if validation should occur; otherwise, <c>false</c>.
        /// </value>
        protected override bool ValidateUserLoggedOn
        {
            get { return false; }
        }
    }
}