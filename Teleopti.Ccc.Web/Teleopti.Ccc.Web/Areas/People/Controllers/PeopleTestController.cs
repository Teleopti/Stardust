using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class PeopleTestController : ApiController
	{
		private readonly ILoggedOnUser _loggonUser;
		private readonly IPersonRepository _personRepository;

		public PeopleTestController(ILoggedOnUser loggedOnUser, IPersonRepository personRepository)
		{
			_loggonUser = loggedOnUser;
			_personRepository = personRepository;
		}


		[UnitOfWork, HttpGet, Route("api/TestData/CurrentTeam")]
		public virtual IHttpActionResult GetLoggonUserTeam()
		{
			var currentDate = DateOnly.Today;
			var myTeam = _loggonUser.CurrentUser().MyTeam(currentDate);
			if (myTeam == null)
			{
				return Ok(new { });
			}
			var peopleInTeam = _personRepository.FindPeopleBelongTeam(myTeam, new DateOnlyPeriod(currentDate, currentDate));
			var results = peopleInTeam.Select(p => new
			{
				PersonId = p.Id.GetValueOrDefault(),
				FirstName = p.Name.FirstName,
				LastName = p.Name.LastName
			});
			return Ok(results);
		}
	}

}