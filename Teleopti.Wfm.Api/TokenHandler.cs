using System.Linq;
using System.Text;
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
		private readonly IAuthenticationTenantClient _authenticationQuerier;

		public TokenHandler(OwinMiddleware next, ITokenVerifier tokenVerifier, IRepositoryFactory repositoryFactory, ILogOnOff logOnOff, IAuthenticationTenantClient authenticationQuerier) : base(next)
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
			
			var token = new StringBuilder(auth).Replace("bearer", "").Replace("Bearer", "").Replace("BEARER", "").Replace(":", "").ToString().Trim();
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
				var businessUnit = result.Person.PermissionInformation.HasAccessToAllBusinessUnits()
					? _repositoryFactory.CreateBusinessUnitRepository(uow).LoadAllBusinessUnitSortedByName().First()
					: result.Person.PermissionInformation.BusinessUnitAccessCollection().FirstOrDefault();

				_logOnOff.LogOn(result.DataSource, result.Person, businessUnit);
			}
			
			// Call the next delegate/middleware in the pipeline
			await Next.Invoke(context);
		}
	}
}