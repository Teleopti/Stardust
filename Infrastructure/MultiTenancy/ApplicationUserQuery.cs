using System;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class ApplicationUserQuery : IApplicationUserQuery
	{
		private readonly ISession _session;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		private const string sql = @"
select Person, Password from ApplicationAuthenticationInfo
where ApplicationLogonName=:userName";


		//when moving - we cannot have these dependencies. need to change to something else
		public ApplicationUserQuery(ICurrentUnitOfWork currentUnitOfWork, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_session = currentUnitOfWork.Session();
		}

		public ApplicationUserQueryResult FindUserData(string userName)
		{
			var res = _session.CreateSQLQuery(sql)
				.SetString("userName", userName)
				.SetResultTransformer(Transformers.AliasToBean<queryResult>())
				.UniqueResult<queryResult>();
			if (res == null)
				return new ApplicationUserQueryResult {Success = false};
			
			return new ApplicationUserQueryResult
			{
				Success = true,
				PersonId = res.Person,
				Tennant = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().Name,
				Password = res.Password
			};
		}

		private class queryResult
		{
			public Guid Person { get; set; }
			public string Password { get; set; }
		}
	}
}