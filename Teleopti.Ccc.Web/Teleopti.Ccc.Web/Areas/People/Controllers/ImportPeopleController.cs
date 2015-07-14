using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.People.Core;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class ImportPeopleController : ApiController
	{
		private readonly IPeoplePersister _peoplePersister;

		public ImportPeopleController(IPeoplePersister peoplePersister)
		{
			_peoplePersister = peoplePersister;
		}

		[UnitOfWork]
		[HttpPost, Route("api/People/ImportPeople/Users")]
		public virtual IHttpActionResult ImportUsers(RawUserData rawUsersData)
		{
			var result = _peoplePersister.Persist(rawUsersData.Users);

			return Ok(result);
		}
	}


	public class RawUserData
	{
		public IEnumerable<RawUser> Users { get; set; }
	}

	public class RawUser
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string WindowsUser { get; set; }
		public string ApplicationUserId { get; set; }
		public string Password { get; set; }
		public string Role { get; set; }
		public string ErrorMessage { get; set; }
	}
}