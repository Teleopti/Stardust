using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class ApplicationController : ApiController
	{
		private readonly IAreaWithPermissionPathProvider _areaWithPermissionPathProvider;

		public ApplicationController(IAreaWithPermissionPathProvider areaWithPermissionPathProvider)
		{
			_areaWithPermissionPathProvider = areaWithPermissionPathProvider;
		}

		[UnitOfWork, HttpGet, Route("api/Global/Application/Areas")]
		public virtual IEnumerable<object> GetAreas()
		{
			return _areaWithPermissionPathProvider.GetWfmAreasWithPermissions()
				.Select(a => new { Name = a.Name(), a.InternalName, _links = a.Links.ToArray() }).ToArray();
		}
	}
							   
}