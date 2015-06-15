using System;
using System.Data.SqlClient;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Controllers
{
	public class UpgradeDatabasesController : ApiController
	{
		private readonly CheckDatabaseVersions _checkDatabaseVersions;

		public UpgradeDatabasesController(CheckDatabaseVersions checkDatabaseVersions)
		{
			_checkDatabaseVersions = checkDatabaseVersions;
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("api/UpgradeDatabases/GetVersions")]
		public virtual JsonResult<VersionResultModel> GetVersions(VersionCheckModel model)
		{
			return Json(_checkDatabaseVersions.GetVersions(model));

		}
	}
}
