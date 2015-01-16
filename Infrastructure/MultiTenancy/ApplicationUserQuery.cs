using System;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class ApplicationUserQuery : IApplicationUserQuery
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		private const string sql = @"
select auth.Person, auth.Password from ApplicationAuthenticationInfo auth
inner join Person p on p.Id=auth.Person
where ApplicationLogonName=:userName
and (p.TerminalDate is null or p.TerminalDate>getdate())";


		//when moving - we cannot have these dependencies. need to change to something else
		public ApplicationUserQuery(ICurrentUnitOfWork currentUnitOfWork, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_currentUnitOfWork = currentUnitOfWork;
		}

		public ApplicationUserQueryResult FindUserData(string userName)
		{
			var res = _currentUnitOfWork.Session().CreateSQLQuery(sql)
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