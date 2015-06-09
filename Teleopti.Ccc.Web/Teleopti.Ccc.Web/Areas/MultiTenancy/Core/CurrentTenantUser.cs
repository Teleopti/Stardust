using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	//would like two different impl here but hard now when web + tenant is same process...
	public class CurrentTenantUser : ICurrentTenantUser
	{
		private readonly ICurrentHttpContext _currentHttpContext;
		private readonly ISessionSpecificDataProvider _sessionDataProvider;
		private readonly IFindPersonInfoByCredentials _findPersonInfoByCredentials;

		public CurrentTenantUser(ICurrentHttpContext currentHttpContext, 
														ISessionSpecificDataProvider sessionDataProvider, 
														IFindPersonInfoByCredentials findPersonInfoByCredentials)
		{
			_currentHttpContext = currentHttpContext;
			_sessionDataProvider = sessionDataProvider;
			_findPersonInfoByCredentials = findPersonInfoByCredentials;
		}

		public PersonInfo CurrentUser()
		{
			var personInfoSetFromExternalCall = _currentHttpContext.Current().Items[TenantAuthentication.PersonInfoKey] as PersonInfo;
			if (personInfoSetFromExternalCall != null)
				return personInfoSetFromExternalCall;

			var sessionData = _sessionDataProvider.GrabFromCookie();
			return sessionData == null ? null : _findPersonInfoByCredentials.Find(sessionData.PersonId, sessionData.TenantPassword);
		}
	}
}