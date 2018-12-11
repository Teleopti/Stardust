using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Wfm.Api
{
	public class TokenHandler : OwinMiddleware
	{
		private readonly ITokenVerifier _tokenVerifier;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ILogOnOff _logOnOff;
		private readonly IAuthenticationQuerier _authenticationQuerier;

		public TokenHandler(OwinMiddleware next, ITokenVerifier tokenVerifier, IRepositoryFactory repositoryFactory, ILogOnOff logOnOff, IAuthenticationQuerier authenticationQuerier) : base(next)
		{
			_tokenVerifier = tokenVerifier;
			_repositoryFactory = repositoryFactory;
			_logOnOff = logOnOff;
			_authenticationQuerier = authenticationQuerier;
		}

		public override async Task Invoke(IOwinContext context)
		{
			var auth = context.Request.Headers["Authorization"];
			if (string.IsNullOrEmpty(auth))
			{
				context.Response.StatusCode = 401;
				return;
			}

			var token = auth.Replace("bearer", "").Replace("Bearer", "").Replace("BEARER", "").Replace(":", "").Trim();
			var verified = await _tokenVerifier.TryGetUserAsync(token);
			if (!verified.Item1)
			{
				context.Response.StatusCode = 401;
				return;
			}

			var user = verified.Item2;
			var result = _authenticationQuerier.TryLogon(new IdLogonClientModel {Id = user.UserId}, "API");
			if (!result.Success)
			{
				context.Response.StatusCode = 401;
				return;
			}
			
			using (var uow = result.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				var personRep = _repositoryFactory.CreatePersonRepository(uow);
				var person = personRep.Get(user.UserId);
				var businessUnit = _repositoryFactory.CreateBusinessUnitRepository(uow).LoadAllBusinessUnitSortedByName().First();
				_logOnOff.LogOn(result.DataSource, person, businessUnit);
			}
			
			// Call the next delegate/middleware in the pipeline
			await Next.Invoke(context);
		}
	}
}