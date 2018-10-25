using System.Net;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class DataTokenController : ApiController
	{
		private readonly IDataTokenManager _tokenManager;

		public DataTokenController(IDataTokenManager tokenManager)
		{
			_tokenManager = tokenManager;
		}

		[UnitOfWork, HttpPost, Route("api/Global/GetTokenForPersistApplicationLogonNames")]
		public virtual IHttpActionResult GetTokenForPersistApplicationLogonNames(PersonApplicationLogonInputModel updateMessage)
		{
			var returnValue = _tokenManager.GetTokenForPersistApplicationLogonNames(updateMessage);
			return returnValue == null ? (IHttpActionResult)StatusCode(HttpStatusCode.Forbidden) : Ok(returnValue);
		}

		[UnitOfWork, HttpPost, Route("api/Global/GetTokenForPersistIdentities")]
		public virtual IHttpActionResult GetTokenForPersistIdentities(PersonIdentitiesInputModel updateMessage)
		{
			var returnValue = _tokenManager.GetTokenForPersistIdentities(updateMessage);
			return returnValue == null ? (IHttpActionResult)StatusCode(HttpStatusCode.Forbidden) : Ok(returnValue);
		}
	}
}