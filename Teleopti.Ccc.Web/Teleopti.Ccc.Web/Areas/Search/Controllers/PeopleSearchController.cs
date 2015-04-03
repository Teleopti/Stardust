using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.Search.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Search.Controllers
{
	public class PeopleSearchController : ApiController
	{
		private readonly IPersonFinderReadOnlyRepository _searchRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPersonRepository _personRepository;

		public PeopleSearchController(IPersonFinderReadOnlyRepository searchRepository, IPersonRepository personRepository, IPermissionProvider permissionProvider)
		{
			_searchRepository = searchRepository;
			_personRepository = personRepository;
			_permissionProvider = permissionProvider;
		}

		[UnitOfWork]
		[HttpGet, Route("api/Search/People")]
		public virtual IHttpActionResult GetResult(string keyword, int pageSize, int currentPageIndex)
		{
			var search = new PersonFinderSearchCriteria(PersonFinderField.All, keyword, pageSize, DateOnly.Today, 0, 0);
		    search.CurrentPage = currentPageIndex;
			_searchRepository.Find(search);
			var permittedPersonList =
				search.DisplayRows.Where(
					r =>
						r.RowNumber > 0 &&
						_permissionProvider.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage,
							DateOnly.Today, r));
			var personIdList = permittedPersonList.Select(x => x.PersonId);
			var peopleList = _personRepository.FindPeople(personIdList);
			var result = peopleList.Select(x => new PeopleSummary
			{
				FirstName = x.Name.FirstName,
				LastName = x.Name.LastName,
				EmploymentNumber = x.EmploymentNumber,
				PersonId = x.Id.Value,
				Email = x.Email
			});
			return Ok(result);
		}
	}
}