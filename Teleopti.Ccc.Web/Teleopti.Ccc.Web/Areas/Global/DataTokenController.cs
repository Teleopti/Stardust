using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.Global.Models;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class DataTokenController : ApiController
	{
		private readonly IAuthorization _principalAuthorization;
		private readonly ILoggedOnUser _loggonUser;
		private readonly IPersonFinderReadOnlyRepository _personFinder;
		private readonly SignatureCreator _signatureCreator;
		private readonly IDataTokenManager _tokenManager;

		public DataTokenController(IAuthorization principalAuthorization,
			ILoggedOnUser loggonUser,
			IPersonFinderReadOnlyRepository personFinder,
			SignatureCreator signatureCreator,
			IDataTokenManager tokenManager)
		{
			_principalAuthorization = principalAuthorization;
			_loggonUser = loggonUser;
			_personFinder = personFinder;
			_signatureCreator = signatureCreator;
			_tokenManager = tokenManager;
		}

		[UnitOfWork, HttpPost, Route("api/Global/GetTokenForPersistApplicationLogonNames")]
		public virtual IHttpActionResult GetTokenForPersistApplicationLogonNames(PersonApplicationLogonInputModel updateMessage)
		{
			var returnValue = _tokenManager.GetTokenForPersistApplicationLogonNames(updateMessage);
			return returnValue == null ? (IHttpActionResult)Unauthorized() : Ok(returnValue);
		}

		[UnitOfWork, HttpPost, Route("api/Global/GetTokenForPersistIdentities")]
		public virtual IHttpActionResult GetTokenForPersistIdentities(PersonIdentitiesInputModel updateMessage)
		{
			var returnValue = _tokenManager.GetTokenForPersistIdentities(updateMessage);
			return returnValue == null ? (IHttpActionResult)Unauthorized() : Ok(returnValue);
		}
	}
}