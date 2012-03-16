using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for user details regarding the account
    /// </summary>
    public class UserDetailRepository : Repository<IUserDetail>, IUserDetailRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserDetailRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public UserDetailRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

    	public UserDetailRepository(IUnitOfWorkFactory unitOfWorkFactory)
			: base(unitOfWorkFactory)
    	{
    	}

        public IUserDetail FindByUser(IPerson user)
        {
            var userDetail = Session.CreateCriteria(typeof(UserDetail))
                        .Add(Restrictions.Eq("Person",user))
                        .UniqueResult<IUserDetail>();
            
            //todo: This can be removed later, might need a fix in people
            if (userDetail == null)
            {
                userDetail = new UserDetail(user);
                Add(userDetail);
            }

            return userDetail;
        }

        public override bool ValidateUserLoggedOn
        {
            get
            {
                return false;
            }
        }

        public IDictionary<IPerson, IUserDetail> FindAllUsers()
        {
            var userDetails = Session.CreateCriteria(typeof (UserDetail))
                .SetFetchMode("Person", FetchMode.Join)
                .List<IUserDetail>();

            return userDetails.ToDictionary(userDetail => userDetail.Person);
        }


        public IDictionary<IPerson, IUserDetail> FindByUsers(IEnumerable<IPerson> persons)
        {
            var userDetails = new List<IUserDetail>();
            foreach (var personBatch in persons.Batch(400))
            {
                userDetails.AddRange(Session.CreateCriteria(typeof(UserDetail))
                        .SetFetchMode("Person", FetchMode.Join)
                        .Add(Restrictions.In("Person", personBatch.ToArray()))
                        .List<IUserDetail>());   
            }

            return userDetails.ToDictionary(userDetail => userDetail.Person);
        }
    }
}
