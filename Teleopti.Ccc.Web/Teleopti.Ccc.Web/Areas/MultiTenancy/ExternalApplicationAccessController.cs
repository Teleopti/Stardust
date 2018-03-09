using System;
using System.Text;
using System.Web.Http;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy
{
	public class ExternalApplicationAccessController : ApiController
	{
		public const string Salt = "$2a$10$WbJpBWPWsrVOlMze27m0ne";

		private readonly IPersistExternalApplicationAccess _persistExternalApplicationAccess;
		private readonly ICurrentTenantUser _currentTenantUser;
		private readonly IFindExternalApplicationAccessByHash _findExternalApplicationAccessByHash;
		private readonly HashFromToken _hashFromToken;

		public ExternalApplicationAccessController(IPersistExternalApplicationAccess persistExternalApplicationAccess, ICurrentTenantUser currentTenantUser, IFindExternalApplicationAccessByHash findExternalApplicationAccessByHash, HashFromToken hashFromToken)
		{
			_persistExternalApplicationAccess = persistExternalApplicationAccess;
			_currentTenantUser = currentTenantUser;
			_findExternalApplicationAccessByHash = findExternalApplicationAccessByHash;
			_hashFromToken = hashFromToken;
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("api/token")]
		public virtual IHttpActionResult Add([FromBody]NewExternalApplicationModel model)
		{
			var access = new ExternalApplicationAccess(_currentTenantUser.CurrentUser().Id, model.Name, "");
			var token = Convert.ToBase64String(Encoding.UTF8.GetBytes((Guid.NewGuid() + Guid.NewGuid().ToString()).Replace("-", "")));
			access.SetHash(_hashFromToken.Generate(Salt, token));
			_persistExternalApplicationAccess.Persist(access);
			return Ok(new NewExternalApplicationResponseModel{Token = token});
		}

		[HttpPost]
		[TenantUnitOfWork]
		[NoTenantAuthentication]
		[Route("api/token/verify")]
		public virtual IHttpActionResult Verify([FromBody]string token)
		{
			var hash = _hashFromToken.Generate(Salt, token);
			var found = _findExternalApplicationAccessByHash.Find(hash);
			if (found == null)
			{
				return Ok();
			}
			return Ok(found.PersonId);
		}
	}
}