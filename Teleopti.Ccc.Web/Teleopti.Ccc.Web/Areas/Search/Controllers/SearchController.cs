using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Search.Controllers
{
	public class SearchController : ApiController
	{
		private readonly IPersonFinderReadOnlyRepository _searchRepository;
		private readonly IPermissionProvider _permissionProvider;

		public SearchController(IPersonFinderReadOnlyRepository searchRepository, IPermissionProvider permissionProvider)
		{
			_searchRepository = searchRepository;
			_permissionProvider = permissionProvider;
		}

		[UnitOfWork]
		[HttpGet,Route("api/Search/People")]
		public virtual IHttpActionResult GetResult(string keyword)
		{
			var search = new PersonFinderSearchCriteria(PersonFinderField.All, keyword, 20, DateOnly.MinValue, 0, 0);
			_searchRepository.Find(search);

			return Ok(search.DisplayRows.Where(r => r.RowNumber>0 && _permissionProvider.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage, DateOnly.Today, r)));
		}
	}
}