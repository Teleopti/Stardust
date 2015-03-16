using System.Web.Http;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class UserController : ApiController
	{
		private readonly ICurrentIdentity _currentIdentity;

		public UserController(ICurrentIdentity currentIdentity)
		{
			_currentIdentity = currentIdentity;
		}

		[Route("api/Global/User/CurrentUser"), HttpGet]
		public object CurrentUser()
		{
			return new { UserName = _currentIdentity.Current().Name };
		}
	}
}