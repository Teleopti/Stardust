using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Web.Areas.People.Core.Persisters;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class ImportPeopleController : ApiController
	{
		private readonly IPeoplePersister _peoplePersister;

		public ImportPeopleController(IPeoplePersister peoplePersister)
		{
			_peoplePersister = peoplePersister;
		}

		[TenantUnitOfWork]
		[UnitOfWork, Route("api/People/ImportPeople"), HttpPost]
		public virtual IHttpActionResult ImportUsers([FromBody]RawUserData rawUsersData)
		{
			var result = _peoplePersister.Persist(rawUsersData.Users);

			return Ok(result);
		}
	}


	public class RawUserData
	{
		public IList<RawUser> Users { get; set; }
	}

	public class RawUser
	{
		public string Firstname { get; set; }
		public string Lastname { get; set; }
		public string WindowsUser { get; set; }
		public string ApplicationUserId { get; set; }
		public string Password { get; set; }
		public string Role { get; set; }
		public string ErrorMessage { get; set; }
	}
}