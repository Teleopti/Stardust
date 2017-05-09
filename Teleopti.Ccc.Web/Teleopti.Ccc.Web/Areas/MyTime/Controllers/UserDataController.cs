using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class UserDataController : ApiController
	{
		private readonly IUserDataFactory _userDataFactory;

		public UserDataController(IUserDataFactory userDataFactory)
		{
			_userDataFactory = userDataFactory;
		}

		[UnitOfWork]
		[HttpGet,Route("mytime/userdata/FetchUserData")]
		public virtual IHttpActionResult FetchUserData()
		{
			return Ok(_userDataFactory.CreateViewModel());
		}
	}
}