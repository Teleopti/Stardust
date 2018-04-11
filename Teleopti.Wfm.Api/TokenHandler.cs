using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Authentication;

namespace Teleopti.Wfm.Api
{
	public class TokenHandler : OwinMiddleware
	{
		private readonly ITokenVerifier _tokenVerifier;
		private readonly IDataSourceForTenant _dataSourceForTenant;
		private readonly ILoadUserUnauthorized _loadUserUnauthorized;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ILogOnOff _logOnOff;

		public TokenHandler(OwinMiddleware next, ITokenVerifier tokenVerifier, IDataSourceForTenant dataSourceForTenant, ILoadUserUnauthorized loadUserUnauthorized, IRepositoryFactory repositoryFactory, ILogOnOff logOnOff) : base(next)
		{
			_tokenVerifier = tokenVerifier;
			_dataSourceForTenant = dataSourceForTenant;
			_loadUserUnauthorized = loadUserUnauthorized;
			_repositoryFactory = repositoryFactory;
			_logOnOff = logOnOff;
		}

		public override Task Invoke(IOwinContext context)
		{
			var auth = context.Request.Headers["Authorization"];
			if (string.IsNullOrEmpty(auth))
			{
				context.Response.StatusCode = 401;
				return Task.FromResult(false);
			}

			var token = auth.Replace("bearer", "").Replace("Bearer", "").Replace("BEARER", "").Replace(":", "").Trim();
			if (!_tokenVerifier.TryGetUser(token, out var user))
			{
				context.Response.StatusCode = 401;
				return Task.FromResult(false);
			}

			var dataSource = _dataSourceForTenant.Tenant(user.Tenant);
			var foundAppUser = _loadUserUnauthorized.LoadFullPersonInSeperateTransaction(dataSource.Application, user.UserId);
			if (foundAppUser.IsTerminated())
			{
				context.Response.StatusCode = 401;
				return Task.FromResult(false);
			}
			
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var personRep = _repositoryFactory.CreatePersonRepository(uow);
				var person = personRep.Get(user.UserId);
				var businessUnit = _repositoryFactory.CreateBusinessUnitRepository(uow).LoadAllBusinessUnitSortedByName().First();
				_logOnOff.LogOn(dataSource, person, businessUnit);
			}

			context.Request.User = new ClaimsPrincipal(new ClaimsIdentity("token", "nameidentifier", user.UserId.ToString()));

			// Call the next delegate/middleware in the pipeline
			return Next.Invoke(context);
		}
	}
}